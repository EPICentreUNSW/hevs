namespace HEVS
{
	/// <summary>
	/// Helper class to access cluster time via HEVS.Time
	/// </summary>
	public class Time
	{
		/// <summary>
		/// The current Unity time of the cluster's master.
		/// </summary>
		public static float time { get { return Cluster.time; } }
		/// <summary>
		/// The current Unity delta-time of the cluster's master.
		/// </summary>
		public static float deltaTime { get { return Cluster.deltaTime; } }
		/// <summary>
		/// The current Unity frame count of the cluster's master.
		/// </summary>
		public static int frameCount { get { return Cluster.frameCount; } }
		/// <summary>
		/// The current node's frames-per-second (FPS).
		/// </summary>
		public static double fps { get { return Cluster.fps; } }

		/// <summary>
		/// Methods for accessing Cluster profile timers.
		/// </summary>
		public class Profiler
		{
			/// <summary>
			/// The time it takes a master to complete the pre-sync broadcast, or a client to receive the pre-sync broadcast, in milliseconds.
			/// </summary>
			public static double preSync { get { return Cluster.preSyncTimeMS; } }
			/// <summary>
			/// The time it takes a master to complete the post-sync broadcast, or a client to receive the post-sync broadcast, in milliseconds.
			/// </summary>
			public static double postSync { get { return Cluster.postSyncTimeMS; } }
			/// <summary>
			/// The time it takes a master to sync the cluster frame swap, or a client to receive and complete a frame swap, in milliseconds.
			/// </summary>
			public static double frameSync { get { return Cluster.frameSyncTimeMS; } }
		}
	}
}