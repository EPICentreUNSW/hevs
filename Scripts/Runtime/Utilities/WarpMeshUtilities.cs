using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace HEVS
{
    /// <summary>
    /// Utility class for loading warp mesh information for projector-based displays.
    /// </summary>
    internal class WarpMeshUtilities
    {
        /// <summary>
        /// Parse the warp mesh files produced by DomeProjection.com tools for a dome display.
        /// </summary>
        /// <param name="warpFile">File path to the warpmap csv file.</param>
        /// <param name="cuttingFile">File path to the cutting csv file.</param>
        /// <param name="aspect">The aspect ratio of the projector.</param>
        /// <returns>Returns a UnityEngine Mesh class containing the loaded warp mesh.</returns>
        public static Mesh ParseDomeProjectionWarpMesh(string warpFile, string cuttingFile, float aspect)
        {
            Vector2[,] uvData = ReadWarpFileUVs(warpFile, cuttingFile);
            
            int cols = uvData.GetLength(0);
            int rows = uvData.GetLength(1);

            int offset;
            float x, y;

            Vector3[] verts = new Vector3[cols * rows];
            Vector2[] uvs = new Vector2[cols * rows];
            int[] tris = new int[(cols - 1) * (rows - 1) * 6];

            for (int j = 0, k = 0, tri = 0; j < rows; j++)
            {
                for (int i = 0; i < cols; i++, k++)
                {
                    x = -0.5f * aspect + aspect * ((float)i / (cols - 1));
                    y = -0.5f + ((float)j / (rows - 1));

                    verts[k] = new Vector3(x, y, 0);
                    uvs[k] = uvData[i, j];

                    offset = (j * cols) + i;

                    if ((i < (cols - 1)) && (j < (rows - 1)))
                    {
                        tris[tri * 6 + 0] = offset;
                        tris[tri * 6 + 1] = offset + 1;
                        tris[tri * 6 + 2] = offset + cols;

                        tris[tri * 6 + 3] = offset + 1;
                        tris[tri * 6 + 4] = offset + cols + 1;
                        tris[tri * 6 + 5] = offset + cols;
                        tri++;
                    }
                }
            }

            Mesh mesh = new Mesh();
            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.triangles = tris;
            return mesh;
        }

        class CuttingInfo
        {
            public Vector2 c0, c1, c2, c3;

            public float ex()
            {
                float dx;
                float dy;

                dx = c1.x - c0.x;
                dy = c1.y - c0.y;

                return Mathf.Sqrt(dx * dx + dy * dy);
            }

            public float ey()
            {
                float dx;
                float dy;

                dx = c0.x - c3.x;
                dy = c0.y - c3.y;

                return Mathf.Sqrt(dx * dx + dy * dy);
            }

            public float cx()
            {
                return ((c0.x + c1.x + c2.x + c3.x) / 4.0f);
            }

            public float cy()
            {
                return ((c0.y + c1.y + c2.y + c3.y) / 4.0f);
            }

            public float theta()
            {
                float i0, i1, j0, j1;

                i0 = (c0.x + c3.x) / 2.0f;
                j0 = (c0.y + c3.y) / 2.0f;
                i1 = (c1.x + c2.x) / 2.0f;
                j1 = (c1.y + c2.y) / 2.0f;

                return Mathf.Atan2(j1 - j0, i1 - i0);
            }
        }

        static CuttingInfo LoadCuttingInfo(string filename)
        {
            CuttingInfo ci = null;
            using (StreamReader file = new global::System.IO.StreamReader(filename))
            {
                string line;
                string[] tokens;

                // skip over the first line in cvs
                line = file.ReadLine();

                line = file.ReadLine();
                tokens = line.Split(';');

                // read cutting info
                ci = new CuttingInfo()
                {
                    c0 = new Vector2(float.Parse(tokens[7]), float.Parse(tokens[8])),
                    c1 = new Vector2(float.Parse(tokens[9]), float.Parse(tokens[10])),
                    c2 = new Vector2(float.Parse(tokens[11]), float.Parse(tokens[12])),
                    c3 = new Vector2(float.Parse(tokens[13]), float.Parse(tokens[14]))
                };
            }

            return ci;
        }

        static Vector2[,] ReadWarpFileUVs(string filename, string cuttingFile)
        {
            // read all lines in
            string[] lines = File.ReadAllLines(filename);
            if (lines.Length <= 1)
                return null;

            // read in cutting data
            CuttingInfo cutting = LoadCuttingInfo(cuttingFile);

            // get col/row from last line
            string lastLine = lines[lines.Length - 1];
            string[] tokens = lastLine.Split(';');

            int cols = Int32.Parse(tokens[4]) + 1;
            int rows = Int32.Parse(tokens[5]) + 1;

            Vector2[,] uvData = new Vector2[cols, rows];

            // extract data from each line (we only care about UVs)
            int col, row;
            for (int i = 1; i < lines.Length; i++)
            {
                tokens = lines[i].Split(';');
                col = Int32.Parse(tokens[4]);
                row = Int32.Parse(tokens[5]);

                uvData[col, row] = ApplyCutting(new Vector2(float.Parse(tokens[2]), float.Parse(tokens[3])), cutting);
            }

            return uvData;
        }

        static Vector3 RotateZ(Vector3 p, float theta)
        {
            Vector3 q;
            q.x = p.x * Mathf.Cos(theta) + p.y * Mathf.Sin(theta);
            q.y = -p.x * Mathf.Sin(theta) + p.y * Mathf.Cos(theta);
            q.z = p.z;
            return (q);
        }

        static Vector2 ApplyCutting(Vector2 orig, CuttingInfo cutting)
        {
            Vector3 v;
            float ex, ey;
            float theta;

            v = new Vector2(orig.x, orig.y);
            v.x -= 0.5f;
            v.y -= 0.5f;
            // v *= cutting.scale; // scale is 1.0 it seems
            ex = cutting.ex();
            v.x *= ex;
            ey = cutting.ey();
            v.y *= ey;

            theta = cutting.theta();
            v = RotateZ(v, -theta);

            v.x += cutting.cx();
            v.y += cutting.cy();

            return new Vector2(v.x, 1 - v.y);
        }

        /// <summary>
        /// Advanced Visualisation and Interaction Environment (AVIE) projector mesh descriptor.
        /// </summary>
        public class AVIEProjectorMeshDescriptor
        {
            public int id { get; private set; }
            public int gridWidth { get; private set; }
            public int gridHeight { get; private set; }
            public float meshStart { get; set; }
            public float meshEnd { get; set; }

            public AVIEProjectorMeshDescriptor(int id, int gridWidth, int gridHeight, float meshStart, float meshEnd)
            {
                this.id = id;
                this.gridWidth = gridWidth;
                this.gridHeight = gridHeight;
                this.meshStart = meshStart;
                this.meshEnd = meshEnd;
            }
        }

        /// <summary>
        /// Advanced Visualisation and Interaction Environment (AVIE) projector mesh instance.
        /// </summary>
        public class AVIEProjectorMesh
        {
            public int id { get { return descriptor.id; } }
            public int gridWidth { get { return descriptor.gridWidth; } }
            public int gridHeight { get { return descriptor.gridHeight; } }
            public float meshStart { get { return descriptor.meshStart; } }
            public float meshEnd { get { return descriptor.meshEnd; } }

            public int vertexCount { get; private set; }

            public AVIEProjectorMeshDescriptor descriptor { get; private set; }

            public class CylinderPoint
            {
                // x is the position between meshStart and meshEnd and y is the height
                public Vector2 xy;
                public Vector2 uv;

                public CylinderPoint(Vector2 xy, Vector2 uv)
                {
                    this.xy = xy;
                    this.uv = uv;
                }
            }

            private CylinderPoint[] points;

            public AVIEProjectorMesh(AVIEProjectorMeshDescriptor descriptor)
            {
                this.descriptor = descriptor;
                vertexCount = (gridWidth + 1) * (gridHeight + 1);
                points = new CylinderPoint[vertexCount];
            }

            public void SetCylinderPoint(int xIndex, int yIndex, Vector2 xy, Vector2 uv)
            {
                points[yIndex * (gridWidth + 1) + xIndex] = new CylinderPoint(xy, uv);
            }

            public CylinderPoint GetCylinderPoint(int index)
            {
                return points[index];
            }
        }

        /// <summary>
        /// Advanced Visualisation and Interaction Environment (AVIE) warp mesh loader.
        /// </summary>
        /// <param name="filepath">File path for the warp mesh txt file.</param>
        /// <param name="projectorID">The ID of the projector we are parsing.
        /// Note: AVIE stores projector IDs in pairs (i.e. 0 for the first projector's left eye, and 1 for the first projector's right eye).</param>
        /// <param name="mesh">The AVIEProjectorMesh that will store the parsed data.</param>
        /// <param name="descriptor">The AVIEProjectorMeshDescriptor that will store the parsed settings.</param>
        /// <returns>Returns true if the warp mesh was successfully parsed for the specified projector.</returns>
        public static bool ParseAVIEWarpMesh(string filepath, int projectorID, out AVIEProjectorMesh mesh, out AVIEProjectorMeshDescriptor descriptor)
        {
            string[] lines = File.ReadAllLines(filepath);

            // find matching projector id
            for (int i = 0; i < lines.Length; ++i)
            {
                string[] tokens = lines[i].Trim().Split(" ,():".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length == 2 && tokens[0] == "Projector")
                {
                    if (projectorID == int.Parse(tokens[1]))
                    {
                        tokens = lines[i+1].Trim().Split(" ,():".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        int gridWidth = Int32.Parse(tokens[0]);
                        int gridHeight = Int32.Parse(tokens[1]);
                        float projectorStart = (float)Double.Parse(tokens[2]);
                        float projectorEnd = (float)Double.Parse(tokens[3]);
                        int numVertices = (gridWidth + 1) * (gridHeight + 1);

                        descriptor = new AVIEProjectorMeshDescriptor(projectorID, gridWidth, gridHeight, projectorStart, projectorEnd);
                        mesh = new AVIEProjectorMesh(descriptor);

                        // continue reading lines
                        for (int j = 0; j < numVertices; ++j)
                        {
                            tokens = lines[i + 2 + j].Trim().Split(" ,():".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            if (tokens.Length < 6)
                            {
                                throw new Exception("Could not create a mesh because there weren't enough vertices. Not enough verts in this list.");
                            }
                            int xIndex = (int)int.Parse(tokens[0]);
                            int yIndex = (int)int.Parse(tokens[1]);
                            float cylinderX = (float)Double.Parse(tokens[2]);
                            float cylinderY = (float)Double.Parse(tokens[3]);
                            float cylinderU = (float)Double.Parse(tokens[4]);
                            float cylinderV = (float)Double.Parse(tokens[5]);
                            mesh.SetCylinderPoint(xIndex, yIndex, new Vector2(cylinderX, cylinderY), new Vector2(cylinderU, cylinderV));
                        }

                        return true;
                    }
                }
            }

            mesh = null;
            descriptor = null;
            return false;
        }
    }
}