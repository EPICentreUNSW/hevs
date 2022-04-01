using UnityEngine;
using System.Collections.Generic;
using HEVS.SimpleJSON;
using System.IO;
using System;
using System.Threading;
using System.Collections;

namespace HEVS.IO
{
    /// <summary>
    /// A class for creating a GameObject Mesh heirarchy for a POTree point-cloud.
    /// </summary>
    public class PotreeMesh
    {
        class PotreeNode
        {
            public string nodePosition;
            public int numPoints;
            public bool hasColour;
            public Vector3[] points;
            public Color[] colours;
            public Bounds bounds;

            public GameObject gameObject;

            public PotreeNode parent = null;
            public List<PotreeNode> children = new List<PotreeNode>();

            public PotreeRenderMaterial renderMaterial;

            public PotreeNode(string nodePosition)
            {
                this.nodePosition = nodePosition;
            }
        }

        class PotreeRenderMaterial
        {
            public Material mat;
            public int numPoints;
            public ComputeBuffer pointsBuffer;
            public ComputeBuffer colourBuffer;
        }

        string filename;
        JSONNode cloudJson;
        string dataDir;
        byte[] hrc;
        byte[] pointData;
        uint basePoints;
        float scale;
        string rootDir;
        int hierarchyStepSize;
        Vector3 boundingBoxMin;
        Vector3 boundingBoxMax;

        /// <summary>
        /// The bounding box for the POTree.
        /// </summary>
        public Bounds boundingBox;

        Dictionary<char, Vector3> childOffsets = new Dictionary<char, Vector3>();

        PotreeNode rootNode;
        GameObject rootGameObject; 

        // used for threaded loading
        Queue<PotreeNode> nodesToLoad = new Queue<PotreeNode>();

        uint numRootPoints;
    //    bool hasColour;
    //    bool hasNormals;
        int dataStride;

        Thread _thread;
        bool _threadRunning;
        bool _isLoading = false;

        object bufferlock = new object();

        Queue<PotreeNode> readyForBufferCreation = new Queue<PotreeNode>();

        Dictionary<PotreeNode, GameObject> objMap = new Dictionary<PotreeNode, GameObject>();

        /// <summary>
        /// Creates a POTree from a specified point-cloud file.
        /// </summary>
        /// <param name="filename"></param>
        public PotreeMesh(string filename)
        {
            int byteIndex = 0;

            Debug.Log("HEVS: Loading Potree point cloud from " + filename);

            this.filename = filename;
            cloudJson = JSON.Parse(File.ReadAllText(filename));
            scale = cloudJson["scale"].AsFloat;
            //Debug.Log(scale);
            dataDir = Path.Combine(Path.GetDirectoryName(filename), cloudJson["octreeDir"]);
            hierarchyStepSize = cloudJson["hierarchyStepSize"].AsInt;
            //Debug.Log(dataDir);

            boundingBoxMin = new Vector3();
            boundingBoxMax = new Vector3();

            boundingBoxMin.x = cloudJson["boundingBox"]["lx"].AsFloat;
            boundingBoxMin.z = cloudJson["boundingBox"]["ly"].AsFloat;
            boundingBoxMin.y = cloudJson["boundingBox"]["lz"].AsFloat;
            boundingBoxMax.x = cloudJson["boundingBox"]["ux"].AsFloat;
            boundingBoxMax.z = cloudJson["boundingBox"]["uy"].AsFloat;
            boundingBoxMax.y = cloudJson["boundingBox"]["uz"].AsFloat;

            boundingBox = new Bounds();
            boundingBox.SetMinMax(boundingBoxMin, boundingBoxMax);

            dataStride = 0;
            foreach (JSONNode attr in cloudJson["pointAttributes"].AsArray)
            {
                switch (attr.Value)
                {
                    case "POSITION_CARTESIAN":
                        dataStride += 12;
                        break;
                    case "COLOR_PACKED":
                        dataStride += 4;
                        break;
                    case "NORMAL_SPHEREMAPPED":
                        dataStride += 2;
                        break;
                }
            }

            /* FIXME : build proper hierarchy table here */
            if (cloudJson.Keys.Contains("hierarchy"))
            {
                numRootPoints = (uint)cloudJson["hierarchy"].AsArray[0].AsArray[1].AsInt;
                Debug.Log("HEVS: base Points : " + numRootPoints);
                rootDir = dataDir;
            }
            else
            {
                hrc = File.ReadAllBytes(Path.Combine(Path.Combine(dataDir, "r"), "r.hrc"));
                //Debug.Log(hrc.Length);

                numRootPoints = System.BitConverter.ToUInt32(hrc, byteIndex + 1);
                while ((byteIndex + 5) < hrc.Length)
                {
                    byte mask = (byte)System.BitConverter.ToChar(hrc, byteIndex);
                    uint numPoints = System.BitConverter.ToUInt32(hrc, byteIndex + 1);
                    //Debug.Log(Convert.ToString(mask, 2) + ":" + numPoints);
                    byteIndex += 5;
                }
                rootDir = Path.Combine(dataDir, "r");
            }

            // this maps the numbers 0..7 to the 8 octants
            childOffsets.Add('0', new Vector3(0, 0, 0));
            childOffsets.Add('1', new Vector3(0, 1, 0));
            childOffsets.Add('2', new Vector3(0, 0, 1));
            childOffsets.Add('3', new Vector3(0, 1, 1));
            childOffsets.Add('4', new Vector3(1, 0, 0));
            childOffsets.Add('5', new Vector3(1, 1, 0));
            childOffsets.Add('6', new Vector3(1, 0, 1));
            childOffsets.Add('7', new Vector3(1, 1, 1));
        }

        /// <summary>
        /// Position of the POTree's bounding box, i.e. its centre.
        /// </summary>
        public Vector3 boundingBoxPosition
        {
            get
            {
                return new Vector3((boundingBoxMin.x + boundingBoxMax.x) / 2.0f,
                    (boundingBoxMin.y + boundingBoxMax.y) / 2.0f,
                    (boundingBoxMin.z + boundingBoxMax.z) / 2.0f);
            }
        }

        /// <summary>
        /// Size of the POTree's bounding box.
        /// </summary>
        public Vector3 boundingBoxScale
        {
            get
            {
                return new Vector3((boundingBoxMax.x - boundingBoxMin.x),
                    (boundingBoxMax.y - boundingBoxMin.y),
                    (boundingBoxMax.z - boundingBoxMin.z));
            }
        }

        Vector3 nodeOffset(string nodePosition)
        {
            Vector3 v = Vector3.zero;
            Vector3 size;

            size = boundingBox.size;

            foreach (char c in nodePosition)
            {
                size = size / 2;
                v += Vector3.Scale(size, childOffsets[c]);
            }


            return v;
        }

        // if the depth is greater than the hierarchyStepSize, the data file will be in a subfolder. This figures out
        // the actual filename of the datafile.
        // eg if hierarchyStepSize if 4, "0123456" -> "0123/r0123456.bin", "0123456789" -> "0123/4567/r0123456789.bin"
        string nodePosToFilename(string nodePos, int hierarchyStepSize)
        {
            string filename = "r" + nodePos + ".bin";
            string path = "";
            string fullPath;
            string origNodePos = nodePos;

            while (nodePos.Length >= hierarchyStepSize)
            {
                path = Path.Combine(path, nodePos.Substring(0, hierarchyStepSize));
                nodePos = nodePos.Substring(hierarchyStepSize);
            }
            fullPath = Path.Combine(path, filename);

            return fullPath;
        }

        bool readNode(ref PotreeNode nodeData)
        {
            int byteIndex;

            float nodeScale;
            Vector3 offset;
            Vector3 min = new Vector3(1000, 1000, 1000);
            Vector3 max = new Vector3(-1000, -1000, -1000);

            string filename = Path.Combine(rootDir, nodePosToFilename(nodeData.nodePosition, hierarchyStepSize));

            try
            {
                pointData = File.ReadAllBytes(filename);
            }
            catch (FileNotFoundException)
            {
                return false;
            }
            catch (DirectoryNotFoundException)
            {
                return false;
            }
            catch (Exception e)
            {
                Debug.Log("HEVS: Exception in file loading : " + e);
                return false;
            }
            //Debug.Log(pointData.Length);
            nodeScale = scale / Mathf.Pow(2, nodeData.nodePosition.Length);

            /* FIXME : should probably get this number from the hierarchy file instead ... */
            nodeData.numPoints = pointData.Length / dataStride;
            nodeData.points = new Vector3[nodeData.numPoints];
            nodeData.hasColour = true;
            nodeData.colours = new Color[nodeData.numPoints];
            byteIndex = 0;
            offset = boundingBoxMin + nodeOffset(nodeData.nodePosition);
            for (int i = 0; i < nodeData.numPoints; i++)
            {
                int x, y, z;
                byte r, g, b, a;

                x = System.BitConverter.ToInt32(pointData, byteIndex);
                z = System.BitConverter.ToInt32(pointData, byteIndex + 4);
                y = System.BitConverter.ToInt32(pointData, byteIndex + 8);
                r = pointData[byteIndex + 12];
                g = pointData[byteIndex + 13];
                b = pointData[byteIndex + 14];
                a = pointData[byteIndex + 15];

                nodeData.points[i] = new Vector3(x * scale, y * scale, z * scale) + offset;

                if (nodeData.points[i].x < min.x)
                    min.x = nodeData.points[i].x;
                if (nodeData.points[i].x > max.x)
                    max.x = nodeData.points[i].x;

                if (nodeData.points[i].y < min.y)
                    min.y = nodeData.points[i].y;
                if (nodeData.points[i].y > max.y)
                    max.y = nodeData.points[i].y;

                if (nodeData.points[i].z < min.z)
                    min.z = nodeData.points[i].z;
                if (nodeData.points[i].z > max.z)
                    max.z = nodeData.points[i].z;

                nodeData.colours[i] = new Color(r / 255.0f, g / 255.0f, b / 255.0f);
                byteIndex += dataStride;

            }

            nodeData.bounds = new Bounds();
            nodeData.bounds.SetMinMax(min, max);
            return true;

        }

        /// <summary>
        /// Loads the POTree in the background;
        /// </summary>
        /// <returns>Returns the root GameObject that will be populated by the load.</returns>
        public GameObject loadInBackground()
        {
            if (_isLoading)
            {
                Debug.LogWarning("HEVS: Already loading, will not load " + filename);
                return null;
            }

            _isLoading = true;

            rootGameObject = new GameObject(); 
            _thread = new Thread(_loadingThread);
            _thread.Start();

            return rootGameObject;
        }

        void createMesh(PotreeNode node, Material material)
        {
            MeshFilter mf = node.gameObject.AddComponent<MeshFilter>();
            MeshRenderer mr = node.gameObject.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh();

            int[] indices = new int[node.points.Length];

            for (int i = 0; i < node.points.Length; i++)
            {
                indices[i] = i;
            }

            mesh.vertices = node.points;
            mesh.colors = node.colours;
            mesh.SetIndices(indices, MeshTopology.Points, 0, true);

            mf.mesh = mesh;

            mr.material = material;
        }

        /// <summary>
        /// If there are any nodes that have been loaded form disk but haven't yet had buffers created we'll create them here. Only does
        /// max of 1 per frame - the idea is you call this function every frame until it's finished loading.
        /// This needs to happen in the main Unity thread, so it can't happen in the loadingThread (hence the need for the
        /// readyForBufferCreation queue).
        /// </summary>
        /// <param name="cloudMaterial">A material to apply to all loaded point-cloud chunks.</param>
        public void updateMeshes(Material cloudMaterial)
        {
            PotreeNode node;

            lock (bufferlock)
            {
                if (readyForBufferCreation.Count > 0)
                {
                    node = readyForBufferCreation.Dequeue();

                    // create the new game object

                    if (node.parent == null)
                    {
                        node.gameObject = rootGameObject; 
                        node.gameObject.name = "Potree Object (" + filename + ")";
                    }
                    else
                    {
                        node.gameObject = new GameObject();
                        node.gameObject.transform.position = rootGameObject.transform.position;
                        node.gameObject.transform.rotation = rootGameObject.transform.rotation;
                        node.gameObject.name = node.nodePosition;
                        node.gameObject.transform.SetParent(node.parent.gameObject.transform);
                    }

                    // create the mesh on this object
                    createMesh(node, cloudMaterial);
                }
            }
        }

        void _loadingThread()
        {
            bool nodeExists;

            _threadRunning = true;

            int totalLoaded = 0;

            nodesToLoad = new Queue<PotreeNode>();

            // start by loading the root node
            rootNode = new PotreeNode("");
            nodesToLoad.Enqueue(rootNode);

            string[] children = { "0", "1", "2", "3", "4", "5", "6", "7" };

            while ((nodesToLoad.Count > 0) && (_threadRunning))
            {
                //Debug.Log("load count = " + nodesToLoad.Count);
                PotreeNode parentNode = nodesToLoad.Dequeue();

                nodeExists = readNode(ref parentNode);
                if (nodeExists)
                {
                    totalLoaded += parentNode.numPoints;

                    lock (bufferlock)
                    {
                        readyForBufferCreation.Enqueue(parentNode);
                    }

                    // add all the children nodes of this node
                    foreach (string child in children)
                    {
                        PotreeNode childNode = new PotreeNode(parentNode.nodePosition + child);
                        childNode.parent = parentNode;
                        parentNode.children.Add(childNode);
                        nodesToLoad.Enqueue(childNode);
                    }
                }
            }

            _threadRunning = false;
            Debug.Log("HEVS: Finished loading " + filename + ". Total of " + totalLoaded + " points.");
        }

        /// <summary>
        /// Cleanup the potree on deletion.
        /// </summary>
        public void cleanup()
        {
            // If the thread is still running, we should shut it down,
            // otherwise it can prevent the game from exiting correctly.
            if (_threadRunning)
            {
                // This forces the while loop in the ThreadedWork function to abort.
                _threadRunning = false;

                // This waits until the thread exits,
                // ensuring any cleanup we do after this is safe. 
                _thread.Join();
            }
        }
    }
}