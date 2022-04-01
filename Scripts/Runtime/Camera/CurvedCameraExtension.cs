using UnityEngine;
using HEVS.Extensions;

namespace HEVS
{
    /// <summary>
    /// A HEVS camera extension behaviour that adjusts the camera's view and projection transforms to capture a slice from a curved display.
    /// </summary>
    [AddComponentMenu("")]
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class CurvedSliceCameraExtension : CameraBehaviour
    {
        /// <summary>
        /// The index of this camera slice, out of the total slices that this display should use.
        /// </summary>
        public int sliceIndex;

        void Start()
        {
            UpdateProjection();
        }

        void Update()
        {
            UpdateProjection();
        }

        void UpdateProjection()
        {
            var curvedDisplay = display as CurvedDisplay;

            // compute slice angle
            float s = Mathf.Sin(Mathf.Deg2Rad * (curvedDisplay.angleStart + sliceIndex * curvedDisplay.sliceAngle)) * curvedDisplay.radius;
            float c = Mathf.Cos(Mathf.Deg2Rad * (curvedDisplay.angleStart + sliceIndex * curvedDisplay.sliceAngle)) * curvedDisplay.radius;
            float s2 = Mathf.Sin(Mathf.Deg2Rad * (curvedDisplay.angleStart + (sliceIndex + 1) * curvedDisplay.sliceAngle)) * curvedDisplay.radius;
            float c2 = Mathf.Cos(Mathf.Deg2Rad * (curvedDisplay.angleStart + (sliceIndex + 1) * curvedDisplay.sliceAngle)) * curvedDisplay.radius;

            Vector3 ll = new Vector3(s, 0, c);
            Vector3 lr = new Vector3(s2, 0, c2);

            // orientate slice
            Vector3 offset = Vector3.up * curvedDisplay.groundOffset + SceneOrigin.position + SceneOrigin.rotation * display.config.transform.translate;
            Quaternion orientation = SceneOrigin.rotation * display.config.transform.rotate;

            Vector3 ul = ll + Vector3.up * curvedDisplay.height;

            var LR = orientation * lr + offset;
            var LL = orientation * ll + offset;
            var UL = orientation * ul + offset;

            var vr = (LR - LL).normalized;
            var vu = (UL - LL).normalized;
            var vn = Vector3.Cross(vu, vr).normalized;

            transform.rotation = Quaternion.LookRotation(Vector3.Cross(vr, orientation * Vector3.up).normalized, orientation * Vector3.up);

            // compute vectors to screen corners
            var va = LL - transform.position;
            var vb = LR - transform.position;
            var vc = UL - transform.position;

            // distance of eye to projection plane
            float dist = -Vector3.Dot(va, vn);

            var cam = GetComponent<UnityEngine.Camera>();

            // extent of perpendicular projection
            float left = Vector3.Dot(vr, va) * cam.nearClipPlane / dist;
            float right = Vector3.Dot(vr, vb) * cam.nearClipPlane / dist;
            float bottom = Vector3.Dot(vu, va) * cam.nearClipPlane / dist;
            float top = Vector3.Dot(vu, vc) * cam.nearClipPlane / dist;

            // build frustum transform
            cam.projectionMatrix = Matrix4x4.Frustum(left, right, bottom, top, cam.nearClipPlane, cam.farClipPlane);
        }
    }

    /// <summary>
    /// A HEVS camera extension used to apply black-level adjustments and post-effects to curved AVIE-like displays. 
    /// Black-level adjustment is used to reduce the colour discrepencies when projecting black with overlapping projectors.
    /// Note: currently uses a mesh to apply warping for curved displays. This will be replaced with UV offsets for post-effects.
    /// </summary>
    [AddComponentMenu("")]
    [RequireComponent(typeof(Camera))]
    public class AVIECameraExtension : CameraBehaviour
    {
        internal RenderTexture renderTexture;

        /// <summary>
        /// The Texture2D used for blending the output of this projector with others.
        /// </summary>
        public Texture2D blendTexture;
        /// <summary>
        /// The black-level texture mask used for handling overlap brightness for multiple projectors.
        /// </summary>
        public Texture2D blackLevelTexture;

        /// <summary>
        /// The black-level threshold.
        /// </summary>
        public float blackLevel;

        GameObject warpMeshObject;

        WarpMeshUtilities.AVIEProjectorMeshDescriptor projectorMeshDescriptor;
        WarpMeshUtilities.AVIEProjectorMesh projectorMesh;

        Material blackLevelMaterial;

        void Update()
        {
            warpMeshObject.GetComponent<MeshRenderer>().material.SetTexture("_BlendTex", blendTexture);
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            RenderTexture postTarget = null;

            // do post first
            bool doPost = false;
            if (doPost)
            {

            }
            else
            {
                postTarget = source;
            }

            // do blacklevel
            blackLevelMaterial.SetTexture("_BlackTex", blackLevelTexture);
            blackLevelMaterial.SetFloat("_BlackLevel", blackLevel);            

            UnityEngine.Graphics.Blit(postTarget, destination, blackLevelMaterial);
        }

        internal void SetupAVIEProjector(int projectorID)
        {
            var curveDisplay = display as CurvedDisplay;

            if (!WarpMeshUtilities.ParseAVIEWarpMesh(curveDisplay.warpMeshPath, projectorID, out projectorMesh, out projectorMeshDescriptor))
            {
                Debug.LogError("HEVS: Failed to parse warp mesh!");
                return;
            }

            // Meshes and game objects that will only be created if this projector is
            // actually being used on this computer
            warpMeshObject = new GameObject(name + "_warpmesh");
            warpMeshObject.transform.SetParent(transform, false);
            warpMeshObject.transform.localPosition = Vector3.forward;
            warpMeshObject.layer = LayerMask.NameToLayer("HEVSCameras");

            warpMeshObject.AddComponent<MeshFilter>().mesh = CreateMesh();
            MeshRenderer meshRenderer = warpMeshObject.AddComponent<MeshRenderer>();
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;

            meshRenderer.material = new Material(Shader.Find("HEVS/ProjectorBlend"));
            blackLevelMaterial = new Material(Shader.Find("HEVS/Blacklevel"));

            if (!string.IsNullOrEmpty(display.blendPath))
                blendTexture = Utils.LoadTexture(display.blendPath + "blend" + projectorMesh.id.ToString("00") + ".png");

            if (!string.IsNullOrEmpty(curveDisplay.blackLevelPath))
               blackLevelTexture = Utils.LoadTexture(curveDisplay.blackLevelPath + "black" + projectorMesh.id.ToString("00") + ".png");

            blackLevel = curveDisplay.blackLevel;

            UnityEngine.Camera camera = gameObject.GetComponent<UnityEngine.Camera>();

            // set ortho options / requirements
            camera.orthographic = true;
            camera.orthographicSize = 0.5f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1.5f;
            camera.allowHDR = false;
            camera.targetTexture = null;
            camera.backgroundColor = Color.black;
            camera.clearFlags = CameraClearFlags.Color;
            camera.cullingMask = LayerMask.GetMask("HEVSCameras");

            Rect viewport = display.viewport.screenRect;

            int renderWidth = (int)(Screen.width * viewport.width * curveDisplay.renderTargetStretchFactor);
            int renderHeight = (int)(Screen.height * viewport.height * curveDisplay.renderTargetStretchFactor);

            // Create a new render texture for the projector 
            int renderTextureDepth = 24;
            renderTexture = new RenderTexture(renderWidth, renderHeight, renderTextureDepth);
            renderTexture.filterMode = FilterMode.Point;

            meshRenderer.material.mainTexture = renderTexture;
            meshRenderer.material.mainTextureScale = new Vector2(1.0f, 1.0f);
            meshRenderer.material.mainTextureOffset = new Vector2(0, 0);

            camera.rect = new Rect(viewport.x, viewport.y, viewport.width, viewport.height);

            int isRight = projectorID % 2;

            switch (display.stereoMode)
            {
                case StereoMode.SideBySide:
                    if (display.swapEyes)
                        camera.rect = new Rect(viewport.x + (1 - isRight) * viewport.width / 2, viewport.y, viewport.width / 2, viewport.height);
                    else
                        camera.rect = new Rect(viewport.x + isRight * viewport.width / 2, viewport.y, viewport.width / 2, viewport.height);
                    break;

                case StereoMode.TopBottom:
                    camera.rect = new Rect(viewport.x, viewport.y + (1 - isRight) * viewport.height / 2, viewport.width, viewport.height / 2);
                    break;

             //   case StereoMode.BottomTop:
             //       camera.rect = new Rect(viewport.x, viewport.y + isRight * viewport.height / 2, viewport.width, viewport.height / 2); break;
            }

            camera.projectionMatrix = OrthoMatrix(-0.5f, 0.5f, -0.5f, 0.5f, 0.3f, 1000.0f);

            camera.SetAndActivateTargetDisplay(display.monitor);
        }

        static Matrix4x4 OrthoMatrix(float left, float right, float bottom, float top, float near, float far)
        {
            float x = 2.0f / (right - left);
            float y = 2.0f / (top - bottom);
            float z = -2.0f / (far - near);
            float a = -(right + left) / (right - left);
            float b = -(top + bottom) / (top - bottom);
            float c = -(far + near) / (far - near);
            float d = 1.0f;

            Matrix4x4 m = new Matrix4x4();
            m[0, 0] = x;
            m[0, 1] = 0;
            m[0, 2] = 0;
            m[0, 3] = a;
            m[1, 0] = 0;
            m[1, 1] = y;
            m[1, 2] = 0;
            m[1, 3] = b;
            m[2, 0] = 0;
            m[2, 1] = 0;
            m[2, 2] = z;
            m[2, 3] = c;
            m[3, 0] = 0;
            m[3, 1] = 0;
            m[3, 2] = 0;
            m[3, 3] = d;
            return m;
        }

        // Create one individual mesh and assign it to its correct render plane
        Mesh CreateMesh()
        {
            int numVertices = projectorMesh.vertexCount;
            int gridHeight = projectorMesh.gridHeight;
            int gridWidth = projectorMesh.gridWidth;

            Vector3[] meshVertices = new Vector3[projectorMesh.vertexCount];
            Vector2[] meshUVs = new Vector2[projectorMesh.vertexCount];
            int[] triangles = new int[3 * 2 * projectorMesh.gridWidth * projectorMesh.gridHeight];

            for (int i = 0; i < numVertices; i++)
            {
                WarpMeshUtilities.AVIEProjectorMesh.CylinderPoint point = projectorMesh.GetCylinderPoint(i);

                // cylinderX is between -0.5f and 0.5f where
                // -0.5f is projector start and 0.5f is projector end

                // projectorX should go from projectorStart to projectorEnd;
                // cylinderX gives the angle in cylindrical coordinates.
                meshVertices[i].x = point.xy.x;
                meshVertices[i].y = point.xy.y;
                meshVertices[i].z = 0;
                meshUVs[i].x = point.uv.x;
                meshUVs[i].y = 1.0f - point.uv.y;
            }

            // Create triangles
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    int tri = (y * gridWidth + x) * 6;

                    triangles[tri + 0] = y * (gridWidth + 1) + x;
                    triangles[tri + 1] = triangles[tri + 0] + 1;
                    triangles[tri + 2] = triangles[tri + 1] + gridWidth;

                    triangles[tri + 3] = triangles[tri + 2];
                    triangles[tri + 4] = triangles[tri + 1];
                    triangles[tri + 5] = triangles[tri + 2] + 1;
                }
            }

            // Create mesh from vertices and triangles
            Mesh mesh = new Mesh();
            mesh.vertices = meshVertices;
            mesh.uv = meshUVs;
            mesh.triangles = triangles;
            return mesh;
        }
    }
}
