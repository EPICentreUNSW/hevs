using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

using HEVS.Collections;
using System.Linq;

namespace HEVS
{
	/// <summary>
	/// HEVS.RPC attribute is needed to identify functions which can be called as remote procedures.
	/// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RPCAttribute : Attribute { }

    /// <summary>
    /// Interface used for registering classes, which aren't MonoBehaviours attached to GameObjects with 
    /// a ClusterObject components, as containing static RPC methods.
    /// </summary>
    public interface IRPCInterface { }

    /// <summary>
    /// HEVS RPC manager class used to execute RPC within a cluster.
    /// </summary>
    public class RPC
    {
        struct RPCCall
        {
            public RPCCall(Delegate method, object[] args)
            {
                if (method.Target != null)
                {
                    ClusterObject obj = (method.Target as MonoBehaviour).GetComponent<ClusterObject>();
                    clusterID = obj.clusterID;
                }
                else
                    clusterID = -1; 
                this.method = method.Method.Name; 
                arguments = args; 
            }
            public RPCCall(int id, string mthd, object[] args) { clusterID = id; method = mthd; arguments = args; }
            public int clusterID;
            public string method;
            public object[] arguments;
        }

		/// <summary>
		/// List of calls we want to invoke on each client.
		/// On the master this list is stored for serializing and sending to the clients.
		/// On each clients the calls are deserialized and stored to be invoked. 
		/// </summary>
		static List<RPCCall> allCallList = new List<RPCCall>();
		/// <summary>
		/// List of calls we want to invoke on the master.
		/// Clients store their calls here before serializing them and sending to the master.
		/// The master stores calls here as they are deserialized and then invokes them.
		/// </summary>
		static List<RPCCall> masterCallList = new List<RPCCall>(); 

        /// <summary>
        /// The number of calls made to the master this frame from this client node.
        /// </summary>
		public static int numberOfMasterCalls { get { return masterCallList.Count; } }

        /// <summary>
        /// The number of calls made from the master to the clients this frame.
        /// </summary>
		public static int numberOfCallsForAll {  get { return allCallList.Count; } }

        /// <summary>
        /// The number of calls made to the master last frame from clients.
        /// If queried on a client node the number is how many calls this client made.
        /// </summary>
		public static int numberOfMasterCallsLastFrame { get; private set; } = 0;

        /// <summary>
        /// The number of calls made from the master to the clients last frame.
        /// </summary>
		public static int numberOfCallsLastFrame { get; private set; } = 0;

        /// <summary>
        /// Dictionary of all Cluster Objects and ther monobehaviours containing methods with the RPC attribute.
        /// When RPCs are made, we loop through this dictionary to find what they are invoked on.
        /// Cluster Objects are added and removed to this with RegisterObject() and DeregisterObject() 
        /// </summary>
        static Dictionary<int, List<MonoBehaviour>> registeredObjectsDictionary = new Dictionary<int, List<MonoBehaviour>>();

        static List<MethodInfo> registeredStaticMethods = new List<MethodInfo>();

        // the following are safety catches for calling RPC within an RPC
        static bool isInvokingCalls = false;
        static List<RPCCall> nextFrameMasterCallList = new List<RPCCall>();
        static List<RPCCall> nextFrameCallOnAllList = new List<RPCCall>();

        /// <summary>
        /// Finds and registers all static RPCAttribute methods on types that inherit IRPCInterface.
        /// </summary>
        internal static void RegisterStaticRPCalls()
        {
            // find all classes that use the IRPCInterface
            var typeList = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                            .Where(x => typeof(IRPCInterface).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract).ToList();

            // find all static methods that have the RPCAttribute on the found types
            foreach (var type in typeList)
            {
                MethodInfo[] methodArray = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                foreach (MethodInfo method in methodArray)
                {
                    object[] attr = method.GetCustomAttributes(typeof(RPCAttribute), true);

                    if (attr.Length > 0)
                    {
                        // use the registered RPCAttribute's name, or the method name
                        var rpcAttr = attr[0] as RPCAttribute;

                        // register methods by their name, printing error for duplicates
                        if (registeredStaticMethods.Contains(method))
                        {
                            Debug.LogError("HEVS: A Static RPC named [" + method.Name + "] has already been registered! Duplicates will be ignored!");
                        }
                        else
                        {
                            registeredStaticMethods.Add(method);
                        }
                    }
                }
            }
        }

		/// <summary>
		/// Register a Cluster Object with our dictionary so we can invoke methods on it later using RPC. 
		/// </summary>
		/// <param name="obj">The object to be registered</param>
		internal static void RegisterObject(ClusterObject obj)
		{
			registeredObjectsDictionary.Add(obj.clusterID, new List<MonoBehaviour>());

			MonoBehaviour[] monoList = obj.gameObject.GetComponents<MonoBehaviour>();
			foreach (MonoBehaviour mono in monoList)
			{
                if (mono == null)
                {
                    Debug.LogError("HEVS: RPCManager tried to check an invalid component for RPC calls on object [" + obj.name + "].");
                    continue;
                }

				Type monoType = mono.GetType();

                // bind member methods that require an instance
				MethodInfo[] methodArray = monoType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
				foreach (MethodInfo method in methodArray)
				{
					if (method.GetCustomAttributes(typeof(RPCAttribute), true).Length > 0)
					{
						registeredObjectsDictionary[obj.clusterID].Add(mono);
						break;
					}
				}

                // potentially register any static RPC
                methodArray = monoType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                foreach (MethodInfo method in methodArray)
                {
                    if (method.GetCustomAttributes(typeof(RPCAttribute), true).Length > 0)
                    {
                        if (!registeredStaticMethods.Contains(method))
                            registeredStaticMethods.Add(method);
                    }
                }
            }
		}

        /// <summary>
        /// Deregister a Cluster Object, so that we don't search through it when making an RPC.
        /// Typically because it has been destroyed. 
        /// </summary>
        /// <param name="obj">The object to deregister</param>
        internal static void DeregisterObject(ClusterObject obj)
		{
			registeredObjectsDictionary.Remove(obj.clusterID);
		}

        /// <summary>
        /// Loop through all the calls for a Master and invoke them.
        /// Should only be called on the master.
        /// </summary>
        internal static void InvokeCallsOnMaster()
        {
            isInvokingCalls = true;

            InvokeCalls(masterCallList);

            numberOfMasterCallsLastFrame = masterCallList.Count;

			masterCallList.Clear();

            isInvokingCalls = false;

            // add new calls to the call list for next frame
            masterCallList.AddRange(nextFrameMasterCallList);
            nextFrameMasterCallList.Clear();
        }

        /// <summary>
        /// Loop through all the calls for a client and invoke them.
        /// Should only be called on a client. 
        /// </summary>
        internal static void InvokeCallsOnAll()
		{
            isInvokingCalls = true;

            InvokeCalls(allCallList);

            numberOfCallsLastFrame = allCallList.Count;

            allCallList.Clear();

            isInvokingCalls = false;

            // add new calls to the call list for next frame
            allCallList.AddRange(nextFrameCallOnAllList);
            nextFrameCallOnAllList.Clear();
        }

		/// <summary>
		/// Invokes a provided list of calls on the current machine.
		/// </summary>
		/// <param name="callList">The RPCs to invoke.</param>
        static void InvokeCalls(List<RPCCall> callList)
        {
			foreach (RPCCall call in callList)
			{
                // is it a global call?
                if (call.clusterID == -1)
                {
                    var method = registeredStaticMethods.Find(m => m.Name == call.method);
                    if (method != null)//registeredStaticMethods.Exists(m => m.Name == call.method))
                        method.Invoke(null, call.arguments);
                    else
                        Debug.LogError("HEVS: Could not find registered Global RPC call [" + call.method + "]!");
                }
				// find the correct objects to make the call on
				else if (registeredObjectsDictionary.ContainsKey(call.clusterID))
				{
					foreach (var obj in registeredObjectsDictionary[call.clusterID])
					{
						// does the behaviour have a method with the correct name?
						MethodInfo method = obj.GetType().GetMethod(call.method, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
						if (method != null)
							method.Invoke(obj, call.arguments);
					}
				}
				else
				{
					Debug.LogError("HEVS: Could not find registered cluster object to make RPC call [" + call.method + "] on!");
				}			
			}
		}

        /// <summary>
        /// Call an RPC from the master to all nodes within a cluster.
        /// Argument type and counts must match the method being used.
        /// </summary>
        /// <param name="method">Method to call.</param>
        public static void Call(Action method)
        {
            Call_Internal(method);
        }
        /// <summary>
        /// Call an RPC from the master to all nodes within a cluster.
        /// Argument type and counts must match the method being used.
        /// </summary>
        /// <param name="method">Method to call.</param>
        public static void Call<T1>(Action<T1> method, T1 arg1)
        {
            Call_Internal(method, arg1);
        }
        /// <summary>
        /// Call an RPC from the master to all nodes within a cluster.
        /// Argument type and counts must match the method being used.
        /// </summary>
        /// <param name="method">Method to call.</param>
        public static void Call<T1, T2>(Action<T1, T2> method, T1 arg1, T2 arg2)
        {
            Call_Internal(method, arg1, arg2);
        }
        /// <summary>
        /// Call an RPC from the master to all nodes within a cluster.
        /// Argument type and counts must match the method being used.
        /// </summary>
        /// <param name="method">Method to call.</param>
        public static void Call<T1, T2, T3>(Action<T1, T2, T3> method,
            T1 arg1, T2 arg2, T3 arg3)
        {
            Call_Internal(method, arg1, arg2, arg3);
        }
        /// <summary>
        /// Call an RPC from the master to all nodes within a cluster.
        /// Argument type and counts must match the method being used.
        /// </summary>
        /// <param name="method">Method to call.</param>
        public static void Call<T1, T2, T3, T4>(Action<T1, T2, T3, T4> method,
            T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            Call_Internal(method, arg1, arg2, arg3, arg4);
        }
        /// <summary>
        /// Call an RPC from the master to all nodes within a cluster.
        /// Argument type and counts must match the method being used.
        /// </summary>
        /// <param name="method">Method to call.</param>
        public static void Call<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> method,
            T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            Call_Internal(method, arg1, arg2, arg3, arg4, arg5);
        }
        /// <summary>
        /// Call an RPC from the master to all nodes within a cluster.
        /// Argument type and counts must match the method being used.
        /// </summary>
        /// <param name="method">Method to call.</param>
        public static void Call<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> method,
            T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            Call_Internal(method, arg1, arg2, arg3, arg4, arg5, arg6);
        }
        /// <summary>
        /// Call an RPC from the master to all nodes within a cluster.
        /// Argument type and counts must match the method being used.
        /// </summary>
        /// <param name="method">Method to call.</param>
        public static void Call<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> method,
            T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            Call_Internal(method, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }
        /// <summary>
        /// Call an RPC from the master to all nodes within a cluster.
        /// Argument type and counts must match the method being used.
        /// </summary>
        /// <param name="method">Method to call.</param>
        public static void Call<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> method,
            T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            Call_Internal(method, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }
        /// <summary>
        /// Call an RPC from the master to all nodes within a cluster.
        /// Argument type and counts must match the method being used.
        /// </summary>
        /// <param name="method">Method to call.</param>
        public static void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> method,
            T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            Call_Internal(method, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        }
        /// <summary>
        /// Call an RPC from the master to all nodes within a cluster.
        /// Argument type and counts must match the method being used.
        /// </summary>
        /// <param name="method">Method to call.</param>
        public static void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> method,
            T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
        {
            Call_Internal(method, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        }
        /// <summary>
        /// Call an RPC from the master to all nodes within a cluster.
        /// Argument type and counts must match the method being used.
        /// </summary>
        /// <param name="method">Method to call.</param>
        public static void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> method,
            T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
        {
            Call_Internal(method, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
        }
        /// <summary>
        /// Call an RPC from the master to all nodes within a cluster.
        /// Argument type and counts must match the method being used.
        /// </summary>
        /// <param name="method">Method to call.</param>
        public static void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> method,
            T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
        {
            Call_Internal(method, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
        }
        /// <summary>
        /// Call an RPC from the master to all nodes within a cluster.
        /// Argument type and counts must match the method being used.
        /// </summary>
        /// <param name="method">Method to call.</param>
        public static void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> method,
            T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
        {
            Call_Internal(method, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
        }
        /// <summary>
        /// Call an RPC from the master to all nodes within a cluster.
        /// Argument type and counts must match the method being used.
        /// </summary>
        /// <param name="method">Method to call.</param>
        public static void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> method,
            T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
        {
            Call_Internal(method, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
        }
        /// <summary>
        /// Call an RPC from the master to all nodes within a cluster.
        /// Argument type and counts must match the method being used.
        /// </summary>
        /// <param name="method">Method to call.</param>
        public static void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> method,
            T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
        {
            Call_Internal(method, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
        }
        /// <summary>
        /// Call an RPC from the master to all nodes within a cluster.
        /// Argument type and counts must match the method being used.
        /// </summary>
        /// <param name="method">Method to call.</param>
        public static void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> method,
            T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16)
        {
            Call_Internal(method, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
        }

        /// <summary>
        /// Call an RPC from a client to the master.
        /// Argument type and counts must match the method being used.
        /// </summary>
        /// <param name="method">Method to call.</param>
        public static void CallOnMaster(Action method)
        {
            CallOnMaster_Internal(method);
        }
        /// <summary>
        /// Call an RPC from a client to the master.
        /// Argument type and counts must match the method being used.
        /// </summary>
        /// <param name="method">Method to call.</param>
        public static void CallOnMaster<T1>(Action<T1> method, T1 arg1)
        {
            CallOnMaster_Internal(method, arg1);
        }
        /// <summary>
        /// Call an RPC from a client to the master.
        /// Argument type and counts must match the method being used.
        /// </summary>
        /// <param name="method">Method to call.</param>
        public static void CallOnMaster<T1, T2>(Action<T1, T2> method, T1 arg1, T2 arg2)
        {
            CallOnMaster_Internal(method, arg1, arg2);
        }
        /// <summary>
        /// Call an RPC from a client to the master.
        /// Argument type and counts must match the method being used.
        /// </summary>
        /// <param name="method">Method to call.</param>
        public static void CallOnMaster<T1, T2, T3>(Action<T1, T2, T3> method,
            T1 arg1, T2 arg2, T3 arg3)
        {
            CallOnMaster_Internal(method, arg1, arg2, arg3);
        }
        /// <summary>
        /// Call an RPC from a client to the master.
        /// Argument type and counts must match the method being used.
        /// </summary>
        /// <param name="method">Method to call.</param>
        public static void CallOnMaster<T1, T2, T3, T4>(Action<T1, T2, T3, T4> method,
            T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            CallOnMaster_Internal(method, arg1, arg2, arg3, arg4);
        }
        /// <summary>
        /// Call an RPC from a client to the master.
        /// Argument type and counts must match the method being used.
        /// </summary>
        /// <param name="method">Method to call.</param>
        public static void CallOnMaster<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> method,
            T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            CallOnMaster_Internal(method, arg1, arg2, arg3, arg4, arg5);
        }
        /// <summary>
        /// Call an RPC from a client to the master.
        /// Argument type and counts must match the method being used.
        /// </summary>
        /// <param name="method">Method to call.</param>
        public static void CallOnMaster<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> method,
            T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            CallOnMaster_Internal(method, arg1, arg2, arg3, arg4, arg5, arg6);
        }
        /// <summary>
        /// Call an RPC from a client to the master.
        /// Argument type and counts must match the method being used.
        /// </summary>
        /// <param name="method">Method to call.</param>
        public static void CallOnMaster<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> method,
            T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            CallOnMaster_Internal(method, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }
        /// <summary>
        /// Call an RPC from a client to the master.
        /// Argument type and counts must match the method being used.
        /// </summary>
        /// <param name="method">Method to call.</param>
        public static void CallOnMaster<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> method,
            T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            CallOnMaster_Internal(method, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }
        /// <summary>
        /// Call an RPC from a client to the master.
        /// Argument type and counts must match the method being used.
        /// </summary>
        /// <param name="method">Method to call.</param>
        public static void CallOnMaster<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> method,
            T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            CallOnMaster_Internal(method, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        }
        /// <summary>
        /// Call an RPC from a client to the master.
        /// Argument type and counts must match the method being used.
        /// </summary>
        /// <param name="method">Method to call.</param>
        public static void CallOnMaster<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> method,
            T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
        {
            CallOnMaster_Internal(method, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        }
        /// <summary>
        /// Call an RPC from a client to the master.
        /// Argument type and counts must match the method being used.
        /// </summary>
        /// <param name="method">Method to call.</param>
        public static void CallOnMaster<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> method,
            T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
        {
            CallOnMaster_Internal(method, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
        }
        /// <summary>
        /// Call an RPC from a client to the master.
        /// Argument type and counts must match the method being used.
        /// </summary>
        /// <param name="method">Method to call.</param>
        public static void CallOnMaster<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> method,
            T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
        {
            CallOnMaster_Internal(method, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
        }
        /// <summary>
        /// Call an RPC from a client to the master.
        /// Argument type and counts must match the method being used.
        /// </summary>
        /// <param name="method">Method to call.</param>
        public static void CallOnMaster<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> method,
            T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
        {
            CallOnMaster_Internal(method, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
        }
        /// <summary>
        /// Call an RPC from a client to the master.
        /// Argument type and counts must match the method being used.
        /// </summary>
        /// <param name="method">Method to call.</param>
        public static void CallOnMaster<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> method,
            T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
        {
            CallOnMaster_Internal(method, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
        }
        /// <summary>
        /// Call an RPC from a client to the master.
        /// Argument type and counts must match the method being used.
        /// </summary>
        /// <param name="method">Method to call.</param>
        public static void CallOnMaster<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> method,
            T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
        {
            CallOnMaster_Internal(method, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
        }
        /// <summary>
        /// Call an RPC from a client to the master.
        /// Argument type and counts must match the method being used.
        /// </summary>
        /// <param name="method">Method to call.</param>
        public static void CallOnMaster<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> method,
            T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16)
        {
            CallOnMaster_Internal(method, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
        }

        internal static void Call_Internal(Delegate method, params object[] arguments)
		{
            if (!Cluster.isMaster)
			{
				Debug.LogError("HEVS: Cluster client tried to make an RPC call to other clients! Method: [" + method.Method.Name + "]");
				return;
            }

            if (method.Target != null)
            {
                var mb = method.Target as MonoBehaviour;
                if (mb == null)
                {
                    Debug.LogError("HEVS: Tried to call a non-static RPC method on a non-GameObject! Method: [" + method.Method.Name + "] ");
                    return;
                }

                if (mb.GetComponent<ClusterObject>() == null)
                {
                    Debug.LogError("HEVS: Tried to call a non-static RPC method on a GameObject that doesn't contain a ClusterObject! GameObject: [" + mb.name + "] Method: [" + method.Method.Name + "] ");
                    return;
                }
            }

            if (!CheckArgumentsValid(arguments))
			{
				Debug.LogError("HEVS: Invalid argument type for RPC call! [" + method.Method.Name + "] ");
				return;
			}

            if (isInvokingCalls)
                nextFrameCallOnAllList.Add(new RPCCall(method, arguments));
            else
                allCallList.Add(new RPCCall(method, arguments));
		}

        internal static void CallOnMaster_Internal(Delegate method, params object[] arguments)
		{
			if (Cluster.isMaster)
			{
				Debug.Log("HEVS: The master tried to call an RPC on itself! Method: [" + method.Method.Name + "]");
				return;
			}

            if (method.Target != null)
            {
                var mb = method.Target as MonoBehaviour;
                if (mb == null)
                {
                    Debug.LogError("HEVS: Tried to call a non-static RPC method on a non-GameObject! Method: [" + method.Method.Name + "] ");
                    return;
                }

                if (mb.GetComponent<ClusterObject>() == null)
                {
                    Debug.LogError("HEVS: Tried to call a non-static RPC method on a GameObject that doesn't contain a ClusterObject! GameObject: [" + mb.name + "] Method: [" + method.Method.Name + "] ");
                    return;
                }
            }

			if (!CheckArgumentsValid(arguments))
			{
				Debug.LogError("HEVS: Invalid argument type for RPC call! Method: [" + method.Method.Name + "] ");
				return;
            }

            if (isInvokingCalls)
                nextFrameMasterCallList.Add(new RPCCall(method, arguments));
            else
                masterCallList.Add(new RPCCall(method, arguments));
		}

        static bool IsValidType(TypeCode typeCode, Type type)
        {
            return (typeCode == TypeCode.Empty ||
                   typeCode == TypeCode.Boolean ||
                   typeCode == TypeCode.Char ||
                   typeCode == TypeCode.SByte ||
                   typeCode == TypeCode.Byte ||
                   typeCode == TypeCode.Int16 ||
                   typeCode == TypeCode.UInt16 ||
                   typeCode == TypeCode.Int32 ||
                   typeCode == TypeCode.UInt32 ||
                   typeCode == TypeCode.Int64 ||
                   typeCode == TypeCode.UInt64 ||
                   typeCode == TypeCode.Single ||
                   typeCode == TypeCode.Double ||
                   typeCode == TypeCode.String ||
                   type == typeof(Vector2) ||
                   type == typeof(Vector3) ||
                   type == typeof(Vector4) ||
                   type == typeof(Color) ||
                   type == typeof(Color32) ||
                   type == typeof(Quaternion));
        }

        static bool AreAllSameType(object[] items)
        {
            Type type = items[0].GetType();
            foreach (var o in items)
                if (o.GetType() != type)
                    return false;
            return true;
        }

        static bool CheckArgumentValid(object arg)
        {
            Type argType = arg.GetType();
            TypeCode argTypeCode = Type.GetTypeCode(argType);

            // is it an array?
            if (argType.IsArray)
            {
                var ar = arg as object[];

                if (AreAllSameType(ar))
                {
                    // get correct type code
                    argType = ar[0].GetType();
                    if (argType.IsArray)
                    {
                        Debug.LogError("HEVS: RPC parameters can not contain nested arrays!");
                        return false;
                    }
               //     Type elementType = argType.GetElementType();
                    argTypeCode = Type.GetTypeCode(argType);
                }
                else
                {
                    // check each element
                    foreach (object a in ar)
                    {
                        // get correct type code
                        argType = a.GetType();
                        if (argType.IsArray)
                        {
                            Debug.LogError("HEVS: RPC parameters can not contain nested arrays!");
                            return false;
                        }
                     //   Type elementType = argType.GetElementType();
                        argTypeCode = Type.GetTypeCode(argType);
                        if (!IsValidType(argTypeCode, argType))
                            return false;
                    }

                    // each element is valid
                    return true;
                }
            }

            return IsValidType(argTypeCode, argType);
        }

        static bool CheckArgumentsValid(object[] arguments)
		{
			foreach (object arg in arguments)
			{
                if (!CheckArgumentValid(arg))
                    return false;
			}
			return true;
		}

        internal static void SerializeCallsOnAll(ByteBufferWriter writer)
		{
			Serialize(writer, allCallList);
		}

        internal static void SerializeCallsOnMaster(ByteBufferWriter writer)
		{
			Serialize(writer, masterCallList);
			masterCallList.Clear();
		}

        static bool WriteTypeCode(ByteBufferWriter writer, Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean: writer.Write((byte)1); break;
                case TypeCode.Char: writer.Write((byte)2); break;
                case TypeCode.SByte: writer.Write((byte)3); break;
                case TypeCode.Byte: writer.Write((byte)4); break;
                case TypeCode.Int16: writer.Write((byte)5); break;
                case TypeCode.UInt16: writer.Write((byte)6); break;
                case TypeCode.Int32: writer.Write((byte)7); break;
                case TypeCode.UInt32: writer.Write((byte)8); break;
                case TypeCode.Int64: writer.Write((byte)9); break;
                case TypeCode.UInt64: writer.Write((byte)10); break;
                case TypeCode.Single: writer.Write((byte)11); break;
                case TypeCode.Double: writer.Write((byte)12); break;
                case TypeCode.String: writer.Write((byte)14); break;
                default:
                    {
                        // vector2
                        if (type == typeof(Vector2))
                            writer.Write((byte)15);
                        // vector3
                        else if (type == typeof(Vector3))
                            writer.Write((byte)16);
                        // vector4
                        else if (type == typeof(Vector4))
                            writer.Write((byte)17);
                        // color
                        else if (type == typeof(Color))
                            writer.Write((byte)18);
                        // color32
                        else if (type == typeof(Color32))
                            writer.Write((byte)19);
                        // quaternion
                        else if (type == typeof(Quaternion))
                            writer.Write((byte)20);
                        else
                        {
                            // invalid type!!!!!
                            return false;
                        }
                    }
                    break;
            }
            return true;
        }


        static bool WriteTypeCodeAndData(ByteBufferWriter writer, Type type, object arg)
        {
            // write the data for the single element
            switch (Type.GetTypeCode(type))
            {
                //case TypeCode.Empty: writer.Write((byte)0); break; // null
                case TypeCode.Boolean: writer.Write((byte)1); writer.Write((bool)arg); break;
                case TypeCode.Char: writer.Write((byte)2); writer.Write((char)arg); break;
                case TypeCode.SByte: writer.Write((byte)3); writer.Write((sbyte)arg); break;
                case TypeCode.Byte: writer.Write((byte)4); writer.Write((byte)arg); break;
                case TypeCode.Int16: writer.Write((byte)5); writer.Write((short)arg); break;
                case TypeCode.UInt16: writer.Write((byte)6); writer.Write((ushort)arg); break;
                case TypeCode.Int32: writer.Write((byte)7); writer.Write((int)arg); break;
                case TypeCode.UInt32: writer.Write((byte)8); writer.Write((uint)arg); break;
                case TypeCode.Int64: writer.Write((byte)9); writer.Write((long)arg); break;
                case TypeCode.UInt64: writer.Write((byte)10); writer.Write((ulong)arg); break;
                case TypeCode.Single: writer.Write((byte)11); writer.Write((float)arg); break;
                case TypeCode.Double: writer.Write((byte)12); writer.Write((double)arg); break;
                case TypeCode.String: writer.Write((byte)14); writer.Write((string)arg); break;
                default:
                    {
                        // vector2
                        if (type == typeof(Vector2))
                        {
                            writer.Write((byte)15);
                            writer.Write((Vector2)arg);
                        }
                        // vector3
                        else if (type == typeof(Vector3))
                        {
                            writer.Write((byte)16);
                            writer.Write((Vector3)arg);
                        }
                        // vector4
                        else if (type == typeof(Vector4))
                        {
                            writer.Write((byte)17);
                            writer.Write((Vector4)arg);
                        }
                        // color
                        else if (type == typeof(Color))
                        {
                            writer.Write((byte)18);
                            writer.Write((Color)arg);
                        }
                        // color32
                        else if (type == typeof(Color32))
                        {
                            writer.Write((byte)19);
                            writer.Write((Color32)arg);
                        }
                        // quaternion
                        else if (type == typeof(Quaternion))
                        {
                            writer.Write((byte)20);
                            writer.Write((Quaternion)arg);
                        }
                        else
                        {
                            // invalid type!!!!!
                            return false;
                        }
                    }
                    break;
            }
            return true;
        }

        static void Serialize(ByteBufferWriter writer, List<RPCCall> callList)
        {
            writer.Write(callList.Count);

            foreach (RPCCall call in callList)
            {
                // write ID of object to make the call on
                writer.Write(call.clusterID);

                // write method to call
                writer.Write(call.method);

                // how many arguments were passed in?
                writer.Write(call.arguments.Length);
                foreach (object arg in call.arguments)
                {
                    // what is the type of each argument?
                    Type argType = arg.GetType();

                    // is it an array?
                    if (argType.IsArray)
                    {
                        // write how many elements there are
                        object[] args = arg as object[];
                        writer.Write((int)args.Length);

                        // all the same?
                        if (args[0].GetType() != typeof(string) &&
                            AreAllSameType(args))
                        {
                            // yes, all the same
                            writer.Write((byte)1);

                            // write type
                            WriteTypeCode(writer, args[0].GetType());

                            // write all data
                            byte[] result = new byte[args.Length * System.Runtime.InteropServices.Marshal.SizeOf(args[0])];
                            Buffer.BlockCopy(args, 0, result, 0, result.Length);
                            writer.Write(result);
                        }
                        else
                        {
                            // no, not the same
                            writer.Write((byte)0);

                            // write each element individually
                            foreach (var a in args)
                                WriteTypeCodeAndData(writer, a.GetType(), a);
                        }
                    }
                    else
                    {
                        // if not an array, is it a null?
                        if (Type.GetTypeCode(argType) == TypeCode.Empty)
                        {
                            // write 0 if null
                            writer.Write((int)0);
                        }
                        else
                        {
                            // write that it is a single element
                            writer.Write((int)1);

                            // write the type and data
                            WriteTypeCodeAndData(writer, argType, arg);
                        }
                    }
                }
            }
        }

        internal static void DeserializeCallsOnMaster(ByteBufferReader reader)
		{
            masterCallList.AddRange(Deserialize(reader));
		}

        internal static void DeserializeCallsOnAll(ByteBufferReader reader)
		{
			allCallList.Clear();
			allCallList = Deserialize(reader);
		}

		static List<RPCCall> Deserialize(ByteBufferReader reader)
        {
			List<RPCCall> callList = new List<RPCCall>();
            int count = reader.ReadInt();

            for (int i = 0; i < count; ++i)
            {
                // object to call the rpc on
                int clusterID = reader.ReadInt();

                // what method to call
                string method = reader.ReadString();

                // how many arguments total?
                int argCount = reader.ReadInt();

                object[] arguments = new object[argCount];
                bool validArguments = true;

                for (int arg = 0; arg < argCount; ++arg)
                {
                    // read how many elements in the argument
                    int arrayCount = reader.ReadInt();

                    // if none, then it is a null argument
                    if (arrayCount == 0)
                    {
                        arguments[arg] = null;
                    }
                    else
                    {
                        // if one element, simply read it
                        if (arrayCount == 1)
                        {
                            // what is the element type?
                            switch (reader.ReadByte())
                            {
                                case 1: arguments[arg] = reader.ReadBoolean(); break;
                                case 2: arguments[arg] = reader.ReadChar(); break;
                                case 3: arguments[arg] = reader.ReadSByte(); break;
                                case 4: arguments[arg] = reader.ReadByte(); break;
                                case 5: arguments[arg] = reader.ReadShort(); break;
                                case 6: arguments[arg] = reader.ReadUShort(); break;
                                case 7: arguments[arg] = reader.ReadInt(); break;
                                case 8: arguments[arg] = reader.ReadUInt(); break;
                                case 9: arguments[arg] = reader.ReadLong(); break;
                                case 10: arguments[arg] = reader.ReadULong(); break;
                                case 11: arguments[arg] = reader.ReadFloat(); break;
                                case 12: arguments[arg] = reader.ReadDouble(); break;
                                case 14: arguments[arg] = reader.ReadString(); break;
                                case 15: arguments[arg] = reader.ReadVector2(); break;
                                case 16: arguments[arg] = reader.ReadVector3(); break;
                                case 17: arguments[arg] = reader.ReadVector4(); break;
                                case 18: arguments[arg] = reader.ReadColor(); break;
                                case 19: arguments[arg] = reader.ReadColor32(); break;
                                case 20: arguments[arg] = reader.ReadQuaternion(); break;
                                default: validArguments = false; break;
                            }
                        }
                        else
                        {
                            // are all the elements the same?
                            byte allSame = reader.ReadByte();

                            if (allSame == 0)
                            {
                                // all different
                                object[] data = new object[arrayCount];
                                for (int j = 0; j < arrayCount; ++j)
                                {
                                    switch (reader.ReadByte())
                                    {
                                        case 1: data[j] = reader.ReadBoolean(); break;
                                        case 2: data[j] = reader.ReadChar(); break;
                                        case 3: data[j] = reader.ReadSByte(); break;
                                        case 4: data[j] = reader.ReadByte(); break;
                                        case 5: data[j] = reader.ReadShort(); break;
                                        case 6: data[j] = reader.ReadUShort(); break;
                                        case 7: data[j] = reader.ReadInt(); break;
                                        case 8: data[j] = reader.ReadUInt(); break;
                                        case 9: data[j] = reader.ReadLong(); break;
                                        case 10: data[j] = reader.ReadULong(); break;
                                        case 11: data[j] = reader.ReadFloat(); break;
                                        case 12: data[j] = reader.ReadDouble(); break;
                                        case 14: data[j] = reader.ReadString(); break;
                                        case 15: data[j] = reader.ReadVector2(); break;
                                        case 16: data[j] = reader.ReadVector3(); break;
                                        case 17: data[j] = reader.ReadVector4(); break;
                                        case 18: data[j] = reader.ReadColor(); break;
                                        case 19: data[j] = reader.ReadColor32(); break;
                                        case 20: data[j] = reader.ReadQuaternion(); break;
                                        default: validArguments = false; break;
                                    }
                                }
                                arguments[arg] = data;
                            }
                            else
                            {
                                // all the same

                                // read type
                                byte argType = reader.ReadByte();

                                // if it is an array, read the bytes then convert to the correct type
                                byte[] bytes = reader.ReadByteArray();

                                switch (argType)
                                {
                                    case 1: arguments[arg] = new bool[arrayCount]; Buffer.BlockCopy(bytes, 0, (arguments[arg] as bool[]), 0, (arguments[arg] as bool[]).Length); break;
                                    case 2: arguments[arg] = new char[arrayCount]; Buffer.BlockCopy(bytes, 0, (arguments[arg] as char[]), 0, (arguments[arg] as char[]).Length); break;
                                    case 3: arguments[arg] = new sbyte[arrayCount]; Buffer.BlockCopy(bytes, 0, (arguments[arg] as sbyte[]), 0, (arguments[arg] as sbyte[]).Length); break;
                                    case 4: arguments[arg] = new byte[arrayCount]; Buffer.BlockCopy(bytes, 0, (arguments[arg] as byte[]), 0, (arguments[arg] as byte[]).Length); break;
                                    case 5: arguments[arg] = new short[arrayCount]; Buffer.BlockCopy(bytes, 0, (arguments[arg] as short[]), 0, (arguments[arg] as short[]).Length); break;
                                    case 6: arguments[arg] = new ushort[arrayCount]; Buffer.BlockCopy(bytes, 0, (arguments[arg] as ushort[]), 0, (arguments[arg] as ushort[]).Length); break;
                                    case 7: arguments[arg] = new int[arrayCount]; Buffer.BlockCopy(bytes, 0, (arguments[arg] as int[]), 0, (arguments[arg] as int[]).Length); break;
                                    case 8: arguments[arg] = new uint[arrayCount]; Buffer.BlockCopy(bytes, 0, (arguments[arg] as uint[]), 0, (arguments[arg] as uint[]).Length); break;
                                    case 9: arguments[arg] = new long[arrayCount]; Buffer.BlockCopy(bytes, 0, (arguments[arg] as long[]), 0, (arguments[arg] as long[]).Length); break;
                                    case 10: arguments[arg] = new ulong[arrayCount]; Buffer.BlockCopy(bytes, 0, (arguments[arg] as ulong[]), 0, (arguments[arg] as ulong[]).Length); break;
                                    case 11: arguments[arg] = new float[arrayCount]; Buffer.BlockCopy(bytes, 0, (arguments[arg] as float[]), 0, (arguments[arg] as float[]).Length); break;
                                    case 12: arguments[arg] = new double[arrayCount]; Buffer.BlockCopy(bytes, 0, (arguments[arg] as double[]), 0, (arguments[arg] as double[]).Length); break;
                                    case 14: arguments[arg] = new string[arrayCount]; Buffer.BlockCopy(bytes, 0, (arguments[arg] as string[]), 0, (arguments[arg] as string[]).Length); break;
                                    case 15: arguments[arg] = new Vector2[arrayCount]; Buffer.BlockCopy(bytes, 0, (arguments[arg] as Vector2[]), 0, (arguments[arg] as Vector2[]).Length); break;
                                    case 16: arguments[arg] = new Vector3[arrayCount]; Buffer.BlockCopy(bytes, 0, (arguments[arg] as Vector3[]), 0, (arguments[arg] as Vector3[]).Length); break;
                                    case 17: arguments[arg] = new Vector4[arrayCount]; Buffer.BlockCopy(bytes, 0, (arguments[arg] as Vector4[]), 0, (arguments[arg] as Vector4[]).Length); break;
                                    case 18: arguments[arg] = new Color[arrayCount]; Buffer.BlockCopy(bytes, 0, (arguments[arg] as Color[]), 0, (arguments[arg] as Color[]).Length); break;
                                    case 19: arguments[arg] = new Color32[arrayCount]; Buffer.BlockCopy(bytes, 0, (arguments[arg] as Color32[]), 0, (arguments[arg] as Color32[]).Length); break;
                                    case 20: arguments[arg] = new Quaternion[arrayCount]; Buffer.BlockCopy(bytes, 0, (arguments[arg] as Quaternion[]), 0, (arguments[arg] as Quaternion[]).Length); break;
                                    default: validArguments = false; break;
                                }
                            }
                        }
                    }
                }

                if (validArguments)
                {
					callList.Add(new RPCCall(clusterID, method, arguments));
                }
                else
                {
                    Debug.LogError("HEVS: Invalid RPC arguments deserialized!");
                }
            }

			return callList; 
		}

        internal static void ResetState()
		{
			registeredObjectsDictionary.Clear();
			allCallList.Clear();
			masterCallList.Clear(); 
		}
    }
}