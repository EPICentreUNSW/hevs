using HEVS.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace HEVS
{
    /// <summary>
    /// The state of the Replay system.
    /// </summary>
    public enum ReplayState
    {
        /// <summary>
        /// Unknown state - the Replay system is not active.
        /// </summary>
        Invalid,
        /// <summary>
        /// System is ready to start recording or playing back.
        /// </summary>
        Ready,
        /// <summary>
        /// System is currently recording input and commands.
        /// </summary>
        Recording,
        /// <summary>
        /// System is currently paused while recording.
        /// </summary>
        RecordingPaused,
        /// <summary>
        /// System is currently playing back recorded input and commands.
        /// </summary>
        Playing,
        /// <summary>
        /// System is currently paused while playing back.
        /// </summary>
        PlayingPaused
    }

    /// <summary>
    /// HEVS Replay system, which can record user input and HEVS events for playback.
    /// </summary>
    public class Replay
    {
        static Replay instance;

        public static Replay active { get { return instance; } set { instance = value; } }

        public ReplayState state { get; private set; } = ReplayState.Invalid;

        public bool isReady { get { return state == ReplayState.Ready; } }
        public bool isPlaying { get { return state == ReplayState.Playing; } }
        public bool isRecording { get { return state == ReplayState.Recording; } }
        public bool isPaused { get { return state == ReplayState.PlayingPaused || state == ReplayState.RecordingPaused; } }

        public bool isPlaybackDone { get { return !playbackReader.HasData; } }

        public ByteBufferWriter recordStream { get { return playbackWriter; } }
        public ByteBufferReader playbackStream { get { return playbackReader; } }

        public string filePath { get; private set; }

        public float startTime { get; private set; } = 0;
        float pausedTime = 0;

        public float currentTime { get { return 0; } }
        public int currentFrame { get { return 0; } }

        public float duration { get { return 0; } }

        ByteBufferWriter playbackWriter;
        ByteBufferReader playbackReader;

        const int WRITE_BUFFER_BYTE_LIMIT = 1024 * 1024 * 512;

        public Replay()
        {
            if (instance != null)
                Debug.LogError("HEVS: Replay instance already exists!");

            instance = this;
        }

        public Replay(string path)
        {
            if (instance != null)
                Debug.LogError("HEVS: Replay instance already exists!");

            instance = this;

            SetReplayPath(path);
        }

        ~Replay()
        {
            instance = null;
        }

        public void SetReplayPath(string path)
        {
            switch (state)
            {
                case ReplayState.Recording:
                case ReplayState.RecordingPaused:
                    {
                        StopRecording();
                    }
                    break;
                case ReplayState.Playing:
                case ReplayState.PlayingPaused:
                    {
                        StopPlayback();
                    }
                    break;
            }

            filePath = path;
            state = ReplayState.Ready;
        }

        public void Flush()
        {
            if (isRecording)
            {
                using (var stream = new FileStream(filePath, FileMode.Append))
                {
                    stream.Write(playbackWriter.AsArray(), 0, playbackWriter.Length);
                }

                playbackWriter.Clear();
            }
        }

        public void StartRecording()
        {
            // remove old file
            if (File.Exists(filePath))
                File.Delete(filePath);

            playbackWriter = new ByteBufferWriter(WRITE_BUFFER_BYTE_LIMIT);

            state = ReplayState.Recording;

            // write current cluster id first to the file
            playbackWriter.Write(Cluster._nextClusterID);

            using (var stream = new FileStream(filePath, FileMode.Append))
                stream.Write(playbackWriter.AsArray(), 0, playbackWriter.Length);
            playbackWriter.Clear();
        }

        public void PauseRecording(bool pause = false)
        {
            if (pause &&
                state == ReplayState.Recording)
            {
                state = ReplayState.RecordingPaused;

                // track time?
            }
            else if (!pause &&
                state == ReplayState.RecordingPaused)
            {
                state = ReplayState.Recording;

                // reset time?
            }
            else
            {
                Debug.LogError("HEVS: Invalid call to Cluster.PauseLogging()! No replay is currently logging!");
            }
        }

        public void StopRecording()
        {
            if (state == ReplayState.Recording ||
                state == ReplayState.RecordingPaused)
            {
                state = ReplayState.Ready;
            }
            else
            {
                Debug.LogError("HEVS: Invalid call to Cluster.StopLogging()! No replay is currently logging!");
            }
        }

        public void StartPlayback()
        {
            if (state != ReplayState.Ready)
            {
                Debug.LogError("HEVS: Invalid call to Cluster.StartPlayback()! A replay is already playing or being recorded!");
                return;
            }

            // read contents of file
            var file = File.OpenRead(filePath);
            if (file == null)
            {
                Debug.LogError("HEVS: Invalid replay path!");
                return;
            }

            file.Seek(0, SeekOrigin.End);
            var p = file.Position;
            file.Seek(0, SeekOrigin.Begin);
            byte[] data = new byte[p];
            file.Read(data, 0, (int)p);
            file.Close();

            // shove contents into buffer reader
            playbackReader = new ByteBufferReader(data);

            // remove all spawned objects (objects already in the scene don't count)
            Cluster.RemoveSpawnedObjects();

            // read cluster it
            Cluster._nextClusterID = playbackReader.ReadInt();

            // start playback
            state = ReplayState.Playing;

            startTime = UnityEngine.Time.time;

            // HACK HACK
            // Need to do better with this
            var particles = GameObject.FindObjectsOfType<ParticleSystem>();
            foreach (var particle in particles)
            {
                particle.Stop();
                particle.Clear();
                particle.Play();
            }
        }

        public void PausePlayback(bool pause = true)
        {
            if (pause &&
                state == ReplayState.Playing)
            {
                state = ReplayState.PlayingPaused;

                // track the time we paused so that we can adjust when we resume
                pausedTime = UnityEngine.Time.time;

                // disable physic simulation
                Physics.autoSimulation = false;

                // disable particle systems
                var particles = GameObject.FindObjectsOfType<ParticleSystem>();
                foreach (var particle in particles)
                    particle.Pause();
            }
            else if (!pause &&
                state == ReplayState.PlayingPaused)
            {
                state = ReplayState.Playing;

                // adjust playback time
                startTime = UnityEngine.Time.time - (pausedTime - startTime);

                // resume physics
                Physics.autoSimulation = true;

                // resume particles
                var particles = GameObject.FindObjectsOfType<ParticleSystem>();
                foreach (var particle in particles)
                    particle.Play();
            }
            else
            {
                Debug.LogError("HEVS: Invalid call to Cluster.PausePlayback()");
            }
        }

        public void StopPlayback()
        {
            if (state != ReplayState.Playing &&
                state != ReplayState.PlayingPaused)
            {
                Debug.LogError("HEVS: Invalid call to Cluster.StopPlayback()! No replay is currently playing!");
                return;
            }

            // undo pause conditions
            if (state == ReplayState.PlayingPaused)
                PausePlayback(false);

            state = ReplayState.Ready;
            //    Physics.autoSimulation = true;

            /*    var rigidBodies = GameObject.FindObjectsOfType<Rigidbody>();
                foreach (Rigidbody rb in rigidBodies)
                {
                    if (rb.GetComponent<ClusterObject>() != null)
                    {
                        rb.isKinematic = false;
                        rb.velocity = Vector3.zero;
                        rb.ResetInertiaTensor();
                    }
                }*/
        }



        void Step()
        {
            PreStep();
            PostStep();
        }

        void PreStep()
        {

        }

        void PostStep()
        {

        }
    }
}