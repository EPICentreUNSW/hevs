using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace HEVS
{
    /// <summary>
    /// A Utility class containing various utility sub-classes and methods.
    /// </summary>
    public partial class Utils
    {
        /// <summary>
        /// Convert a JSONNode to an object.
        /// </summary>
        /// <param name="node">The node to convert.</param>
        /// <returns>Returns the converted object.</returns>
        static public object JSONToValue(SimpleJSON.JSONNode node)
        {
            switch (node.Tag)
            {
                case SimpleJSON.JSONNodeType.Boolean: return node.AsBool;
                case SimpleJSON.JSONNodeType.Number: return node.AsDouble;
                case SimpleJSON.JSONNodeType.String: return node.Value;
                case SimpleJSON.JSONNodeType.Object: return JSONToDictionary(node);
                case SimpleJSON.JSONNodeType.Array:
                case 0: // also array for some shit
                    SimpleJSON.JSONArray arr = node.AsArray;
                    if (arr != null)
                    {
                        object[] objects = new object[arr.Count];
                        int i = 0;
                        foreach (var obj in arr)
                        {
                            objects[i++] = JSONToValue(obj.Value);
                        }
                        return objects;
                    }
                    else
                    {
                        // has children?
                        if (node.Count > 0)
                            return JSONToDictionary(node);
                    }
                    break;
            }
            return null;
        }

        /// <summary>
        /// Convert a JSONNode to a Dictionary.
        /// </summary>
        /// <param name="node">The node to convert.</param>
        /// <returns>Returns the converted node as a dictionary.</returns>
        static public Dictionary<string, object> JSONToDictionary(SimpleJSON.JSONNode node)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            foreach (var key in node)
                dictionary.Add(key.Key, JSONToValue(key.Value));
            return dictionary;
        }

        /// <summary>
        /// Recursively create a folder structure. 
        /// If parent folder doesn't exist, create as well.
        /// </summary>
        /// <param name="folder">Folder path to create.</param>
        static public void CreateFolder(string folder)
        {
            try
            {
                // If folder exists, nothing to do
                if (Directory.Exists(folder))
                    return;

                // If folder is root, done
                DirectoryInfo di = new DirectoryInfo(folder);
                if (di.FullName == di.Root.FullName)
                    return;

                // Create parent folder of the specified folder
                CreateFolder(di.Parent.FullName);

                // Create the specified folder
                Directory.CreateDirectory(di.FullName);
            }
            catch (Exception e)
            {
                Debug.Log("HEVS: Error creating folder \"" + folder + "\".\n\n" + e.Message + "\n\n" + e.StackTrace);
            }
        }

        /// <summary>
        /// Delete all files from a folder, and optionally delete sub-folders.
        /// If the folder is empty after its contents have been deleted then it is also deleted.
        /// </summary>
        /// <param name="folder">Folder path to delete.</param>
        /// <param name="recursive">Should sub-folders be recursively deleted.</param>
        static public void DeleteFolder(string folder, bool recursive = false)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(folder);
                foreach (FileInfo f in di.GetFiles("*.*", SearchOption.TopDirectoryOnly))
                    f.Delete();

                if (recursive)
                    foreach (DirectoryInfo d in di.GetDirectories("*.*", SearchOption.TopDirectoryOnly))
                        DeleteFolder(d.FullName, recursive);

                di = new DirectoryInfo(folder);
                if (di.GetFiles().Length == 0 && di.GetDirectories().Length == 0)
                    Directory.Delete(folder);
            }
            catch (Exception e)
            {
                Debug.Log("HEVS: " + e.Message + "\n" + e.StackTrace);
            }
        }

        /// <summary>
        /// Copy contents from one directory to another.
        /// </summary>
        /// <param name="src">Source path to copy.</param>
        /// <param name="dst">Destination path to copy to.</param>
        static void CopyAssets(string src, string dst)
        {
            // if dest doesn't exist as a directory then try as a file
            if (Directory.Exists(src))
            {
                if (!Directory.Exists(dst))
                    Directory.CreateDirectory(dst);

                // copy over the data
                foreach (string directory in Directory.GetDirectories(src, "*.*", SearchOption.AllDirectories))
                    Directory.CreateDirectory(directory.Replace(src, dst));
                foreach (string file in Directory.GetFiles(src, "*.*", SearchOption.AllDirectories))
                    File.Copy(file, file.Replace(src, dst), true);
            }
            else if (File.Exists(src))
            {
                File.Copy(src, dst, true);
            }
        }

        /// <summary>
        /// Clean filename and convert to WWW format (starting with file:// and using only forward slashes).
        /// </summary>
        /// <param name="originalFileName">Filename to clean and convert</param>
        /// <returns>Filename cleaned and converted</returns>
        static public string GetCleanFileName(string originalFileName)
        {
            string fileToLoad = originalFileName.Replace('\\', '/');

            if (fileToLoad.StartsWith("http") == false)
            {
                fileToLoad = string.Format("file://{0}", fileToLoad);
            }

            return fileToLoad;
        }

        /// <summary>
        /// Load a texture from disc, reading the contents to memory then creates a UnityEngine.Texture2D.
        /// </summary>
        /// <param name="filePath">Path to the texture to load.</param>
        /// <returns>Returns the loaded Texture2D, or null if the path did not exist. If it did exist but was not a valid image then the returned texture will have a resolution of 1-by-1.</returns>
        public static Texture2D LoadTexture(string filePath)
        {
            Texture2D tex = null;
            byte[] fileData;

            if (File.Exists(filePath))
            {
                fileData = File.ReadAllBytes(filePath);
                tex = new Texture2D(1, 1);
                tex.LoadImage(fileData);
            }
            return tex;
        }

        /// <summary>
        /// Convert an index array to a comma separated values string.
        /// </summary>
        /// <param name="array">Array to convert</param>
        /// <returns>Returns the list of values as a string</returns>
        static public string IndexArrayToString(uint[] array)
        {
            StringBuilder builder = new StringBuilder();
            foreach (uint i in array)
            {
                if (builder.Length > 0) builder.Append(",");
                builder.Append(i.ToString());
            }

            return builder.ToString();
        }

        /// <summary>
        /// Convert a UnityEngine.Color to a 32bit unsigned int.
        /// </summary>
        /// <param name="color">The Color to convert.</param>
        /// <returns>Returns the converted color as a 32bit unsigned int.</returns>
        static public uint DecodeIntRGBA(UnityEngine.Color color)
        {
            return (uint)Mathf.RoundToInt(
                  (Mathf.RoundToInt(color.a * 255) |
                  (Mathf.RoundToInt(color.b * 255) << 8) |
                  (Mathf.RoundToInt(color.g * 255) << 16) |
                  (Mathf.RoundToInt(color.r * 255) << 24)));
        }
    }
}