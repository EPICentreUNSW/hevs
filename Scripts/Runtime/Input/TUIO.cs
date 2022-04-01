using System;
using System.Collections.Generic;
using UnityEngine;
using TUIOsharp;
using TUIOsharp.DataProcessors;
using TUIOsharp.Entities;
using OSCsharp.Utils;

namespace HEVS
{
    /// <summary>
    /// Processes TUIO 1.1 input.
    /// </summary>
    public sealed class TUIODevice
    {
        private TuioServer server;
        private CursorProcessor cursorProcessor;
        private ObjectProcessor objectProcessor;
        private BlobProcessor blobProcessor;
        
        /// <summary>
        /// A list of current active cursors from this TUIO connection.
        /// </summary>
        public List<TuioCursor> cursors = new List<TuioCursor>();

        /// <summary>
        /// A list of current active blobs from this TUIO connection.
        /// </summary>
        public List<TuioBlob> blobs = new List<TuioBlob>();

        /// <summary>
        /// A list of current active objects from this TUIO connection.
        /// </summary>
        public List<TuioObject> objects = new List<TuioObject>();

        /// <summary>
        /// Access to the HEVS TUIO config settings for this TUIO connection.
        /// </summary>
        public Config.TUIODevice config { get; private set; }

        /// <summary>
        /// The ID of this TUIO connection.
        /// </summary>
        public string id { get { return config.id; } }

        /// <summary>
        /// Is this TUIO connection "connected".
        /// </summary>
        public bool connected { get; private set; }
        
        /// <summary>
        /// Connect TUIO through a specific port from config settings.
        /// </summary>
        /// <param name="config">The HEVS JSON TUIO config settings.</param>
        public void Connect(Config.TUIODevice config)
        {
            this.config = config;

            if (!UnityEngine.Application.isPlaying) return;
            if (server != null) Disconnect();

            server = new TuioServer(config.port);
            server.Connect();

            if ((config.supportedInput & TUIOInputType.Cursors) != 0)
            {
                cursorProcessor = new CursorProcessor();
                cursorProcessor.CursorAdded += OnCursorAdded;
                cursorProcessor.CursorUpdated += OnCursorUpdated;
                cursorProcessor.CursorRemoved += OnCursorRemoved;
                server.AddDataProcessor(cursorProcessor);
            }
            if ((config.supportedInput & TUIOInputType.Blobs) != 0)
            {
                blobProcessor = new BlobProcessor();
                blobProcessor.BlobAdded += OnBlobAdded;
                blobProcessor.BlobUpdated += OnBlobUpdated;
                blobProcessor.BlobRemoved += OnBlobRemoved;
                server.AddDataProcessor(blobProcessor);
            }
            if ((config.supportedInput & TUIOInputType.Objects) != 0)
            {
                objectProcessor = new ObjectProcessor();
                objectProcessor.ObjectAdded += OnObjectAdded;
                objectProcessor.ObjectUpdated += OnObjectUpdated;
                objectProcessor.ObjectRemoved += OnObjectRemoved;
                server.AddDataProcessor(objectProcessor);
            }

            connected = true;
        }

        /// <summary>
        /// Disconnect this TUIO connection.
        /// </summary>
        public void Disconnect()
        {
            if (server != null)
            {
                server.RemoveAllDataProcessors();
                server.Disconnect();
                server = null;

                connected = false;
            }
        }

        #region Event handlers
        private void OnCursorAdded(object sender, TuioCursorEventArgs e)
        {
            var entity = e.Cursor;
            lock (this)
            {
                cursors.Add(entity);
            }
        }

        private void OnCursorUpdated(object sender, TuioCursorEventArgs e)
        {
            var entity = e.Cursor;
            lock (this)
            {
            }
        }

        private void OnCursorRemoved(object sender, TuioCursorEventArgs e)
        {
            var entity = e.Cursor;
            lock (this)
            {
                if (!cursors.Contains(entity)) return;
                cursors.Remove(entity);
            }
        }

        private void OnBlobAdded(object sender, TuioBlobEventArgs e)
        {
            var entity = e.Blob;
            lock (this)
            {
                blobs.Add(entity);
            }
        }

        private void OnBlobUpdated(object sender, TuioBlobEventArgs e)
        {
            var entity = e.Blob;
            lock (this)
            {
            }
        }

        private void OnBlobRemoved(object sender, TuioBlobEventArgs e)
        {
            var entity = e.Blob;
            lock (this)
            {
                if (!blobs.Contains(entity)) return;
                blobs.Remove(entity);
            }
        }

        private void OnObjectAdded(object sender, TuioObjectEventArgs e)
        {
            var entity = e.Object;
            lock (this)
            {
                objects.Add(entity);
            }
        }

        private void OnObjectUpdated(object sender, TuioObjectEventArgs e)
        {
            var entity = e.Object;
            lock (this)
            {
            }
        }

        private void OnObjectRemoved(object sender, TuioObjectEventArgs e)
        {
            var entity = e.Object;
            lock (this)
            {
                if (!objects.Contains(entity)) return;
                objects.Remove(entity);
            }
        }
        #endregion
    }
}