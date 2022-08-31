using AOT;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace HEVS
{
    /// <summary>
    /// HEVS low-level native graphics API.
    /// </summary>
    public class Graphics
    {
        /// <summary>
        /// The render texture being used by the output cameras when utilising quad-buffering or gen-lock (hardware frame sync).
        /// </summary>
        public static RenderTexture outputRenderTexture { get; private set; }

        [DllImport("hevs.native")]
        static extern IntPtr GetRenderEventFunc();

        [DllImport("hevs.native")]
        static extern void SetupEyeTexture(int width, int height, IntPtr texture);

        internal static void Sync()
        {
            if (nativePluginAvailable)
                SyncEyeTexture();
        }

        [DllImport("hevs.native")]
        static extern void SyncEyeTexture();

        internal static void Shutdown()
        {
            if (nativePluginAvailable)
                ShutdownPlugin();
        }

        [DllImport("hevs.native")]
        static extern void ShutdownPlugin();

        static IntPtr RenderEventFunc = IntPtr.Zero;

        static bool nativePluginAvailable = false;

        internal static void Initialise()
        {
            try
            {
                RegisterDebugCallback(new DebugCallback(DebugMethod));

                RenderEventFunc = GetRenderEventFunc();

                if (RenderEventFunc == null ||
                    RenderEventFunc == IntPtr.Zero)
                    Debug.LogWarning("hevs.native: Couldn't access low-level native render callback.");
                else
                    nativePluginAvailable = true;
            }
            catch (DllNotFoundException)
            {
                Debug.LogWarning("hevs.native: DllNotFoundException. Is the native plugin missing?");
            }
            catch
            {
                Debug.LogWarning("hevs.native: Unknown exception caught during initialisation!");
            }
}

#region Debug Logging
        private delegate void DebugCallback(string message);

        [DllImport("hevs.native")]
        private static extern void RegisterDebugCallback(DebugCallback callback);

        [MonoPInvokeCallback(typeof(DebugCallback))]
        private static void DebugMethod(string message)
        {
            Debug.Log("hevs.native: " + message);
        }
#endregion

        internal static void SetupPluginRenderTarget(UnityEngine.Camera left, UnityEngine.Camera right = null)
        {
            if (nativePluginAvailable)
            { 
                if (outputRenderTexture == null)
                {
                    outputRenderTexture = new RenderTexture(Screen.width * (right ? 2 : 1), Screen.height, 16, RenderTextureFormat.ARGB32, 4);
                    outputRenderTexture.Create();
                    SetupEyeTexture(Screen.width * (right ? 2 : 1), Screen.height, outputRenderTexture.GetNativeTexturePtr());
                }

                left.targetTexture = outputRenderTexture;
                if (right)
                    right.targetTexture = outputRenderTexture;
            }
            else
                Debug.LogWarning("hevs.native: Couldn't register eye texture, defaulting camera output to main render target. Is the native plugin missing?");
        }
    }
}