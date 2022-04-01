using UnityEngine;

namespace HEVS
{
    /// <summary>
    /// HEVS component that tags a GameObject as an object within a clustered environment.
    /// The GameObject is given a cluster ID and can have its Transform synchronised from 
    /// the master node to all client nodes. The GameObject can also synchronise state 
    /// changes, such as when the GameObject is activated/deactivated/destroyed.
    /// </summary>
	[DisallowMultipleComponent]
    [AddComponentMenu("HEVS/Cluster Object")]
    public sealed class ClusterObject : MonoBehaviour
    {
        /// <summary>
        /// ClusterObject state definition.
        /// </summary>
		public enum State
        {
            /// <summary>
            /// The GameObject is active.
            /// </summary>
            Active,
            /// <summary>
            /// The GameObject is inactive.
            /// </summary>
            Inactive,
            /// <summary>
            /// The GameObject has been destroyed.
            /// </summary>
            Destroyed
        }

        /// <summary>
        /// The unique cluster ID of this GameObject.
        /// </summary>
        public int clusterID = -1;

        /// <summary>
        /// Flag for specifying if the GameObject's state should be synchronised.
        /// </summary>
		public bool updateStateOnClients = true; 

        /// <summary>
        /// The cluster ID of the GameObject's parent (must also contain a ClusterObject component).
        /// </summary>
        public int parentID {
            get {
                if (transform.parent && transform.parent.GetComponent<ClusterObject>())
                    return transform.parent.GetComponent<ClusterObject>().clusterID;
                else
                    return -1;
            }
        }

		int oldParentID;

        /// <summary>
        /// Flags specifying which parts of the transform will be synched from the master to all clients.
        /// </summary>
        public int transformFlags = (int)TransformFlags.All;

		void Awake()
		{
			if(GetComponent<Core>())
			{
				updateStateOnClients = false; 
			}
		}

        void Start()
        {
            if (clusterID == -1)
                clusterID = Cluster.nextClusterID;
 
            Cluster.RegisterObject(clusterID, this);
            RPC.RegisterObject(this);

            oldParentID = parentID;
		}

        void OnDestroy()
        {
			ChangeClientState(State.Destroyed);

            Cluster.DeregisterObject(this);
			RPC.DeregisterObject(this);
        }

		void OnEnable()
		{
			ChangeClientState(State.Active);
		}

		void OnDisable()
		{
			ChangeClientState(State.Inactive);
		}

		#region state-change
		/// <summary>
		/// Will apply a new state to matching ClusterObjects on cluster's clients.
		/// </summary>
		/// <param name="newState"></param>
		void ChangeClientState(State newState)
		{
			if(Cluster.isMaster && updateStateOnClients)
			{
				Cluster.ChangeClusterObjectState(clusterID, newState);
			}
		}

		/// <summary>
		/// On a client, update a ClusterObject's state to match that of the Master.
		/// </summary>
		/// <param name="newState"></param>
		public void SetClientState(int newState)
		{
			if (!updateStateOnClients || (clusterID == -1) )
				return;

			switch((State)newState)
			{
				case State.Active:
					gameObject.SetActive(true);
					break;

				case State.Inactive:
					gameObject.SetActive(false);
					break;

				case State.Destroyed:
					Destroy(gameObject);
					break;
			}
		}
		#endregion

		#region cluster-transform
        /// <summary>
        /// Check if the GameObject's parent has changed since the last time we checked.
        /// </summary>
		public bool parentChanged
		{
			get
			{
				// is it the same parent?
				if (oldParentID != parentID)
				{
					// new parent!
					oldParentID = parentID;
					return true;
				}
				else // hasn't changed
					return false;
			}
		}
		#endregion
	}
}