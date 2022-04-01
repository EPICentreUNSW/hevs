using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace HEVS
{
    /// <summary>
    /// The HEVS Config object that holds loaded JSON data defining platforms and their structure.
    /// </summary>
    public partial class Config
    {
        /// <summary>
        /// Interface for HEVS objects parsed from SimpleJSON JSON data.
        /// </summary>
        public interface IConfigObject
        {
            /// <summary>
            /// Parse a SimpleJSON JSON node to set up this object.
            /// </summary>
            /// <param name="json">The SimpleJSON JSON data to parse.</param>
            /// <returns>Returns true if successfully parsed, and false if it fails.</returns>
            bool Parse(SimpleJSON.JSONNode json);
        }

        /// <summary>
        /// Access to the JSON used to define the config.
        /// </summary>
        public SimpleJSON.JSONNode json { get; private set; }

        /// <summary>
        /// Access to the path of the JSON file used to define the config.
        /// </summary>
        public string jsonPath { get; private set; } = "";

        /// <summary>
        /// Access to all currently loaded platforms.
        /// </summary>
        public Dictionary<string, Platform> platforms { get; private set; } = new Dictionary<string, Platform>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Access to the globals, parsed from the JSON config file.
        /// </summary>
        public Dictionary<string, object> globals { get; private set; } = new Dictionary<string, object>();

        /// <summary>
        /// Loads and parses a specified JSON file containing HEVS configuration data.
        /// </summary>
        /// <param name="path">Path to the JSON file to load and parse.</param>
        /// <returns>Returns true if the specified JSON file was successfully parsed.</returns>
        public bool ParseConfig(string path)
        {
            if (path.StartsWith("StreamingAssets"))
                path = path.Replace("StreamingAssets", UnityEngine.Application.streamingAssetsPath);

            // open file
            if (File.Exists(path))
            {
                string jsonText = File.ReadAllText(path);
                if (!string.IsNullOrWhiteSpace(jsonText))
                { 
                    json = SimpleJSON.JSON.Parse(jsonText);
                    if (json != null)
                    {
                        // clear old data
                        platforms.Clear();
                        globals.Clear();

                        jsonPath = path;

                        // add default platform
                        Platform defaultPlatform = new Platform("Default");
                        defaultPlatform.Parse("", null);
                        platforms.Clear();
                        platforms.Add("Default", defaultPlatform);

                        // parse the json
                        if (json.Keys.Contains("globals"))
                            globals = Utils.JSONToDictionary(json["globals"]);

                        if (json.Keys.Contains("platforms"))
                        {
                            // load all platforms
                            foreach (string platform_id in json["platforms"].Keys)
                            {
                                Platform platform = new Platform(platform_id);

                                if (platform.Parse(json["platforms"][platform_id], platforms))
                                    platforms.Add(platform_id, platform);
                                else
                                    Debug.LogError("HEVS: Error parsing JSON for platform [" + platform_id + "]! Platform ignored.");
                            }
                        }
                        return true;
                    }
                    else
                    {
                        Debug.LogError("HEVS: Specified configuration file is not a valid JSON format!");
                    }
                }
            }

            if (!string.IsNullOrEmpty(path))
                Debug.LogWarning("HEVS: Unable to parse HEVS config [" + path + "]. File does not exist.");

            if (platforms.Count == 0)
            {
                // add default platform
                Platform defaultPlatform = new Platform("Default");
                defaultPlatform.Parse("", null);
                platforms.Clear();
                platforms.Add("Default", defaultPlatform);
            }

            return false;
        }

        /// <summary>
        /// Searches this config object for the specified platform and host/node combination.
        /// If no matching combination is found then the platform and node parameters are set to default if useDefaultIfNotFound is set to true.
        /// </summary>
        /// <param name="platformName">Platform to search for.</param>
        /// <param name="hostName">Host/Node to search for within the specified platform.</param>
        /// <param name="platform">The output platform config object if found.</param>
        /// <param name="node">The output node config object if found.</param>
        /// <param name="useDefaultIfNotFound">Used to specify if the method should succeed if a matching combination isn't found, setting platform and node to default values.</param>
        /// <returns>Returns true if a matchin combination was found, or if useDefaultIfNotFound was set.</returns>
        public bool FindPlatformAndNode(string platformName, string hostName, out Platform platform, out Node node, bool useDefaultIfNotFound = true)
        {
            if (platforms.TryGetValue(platformName, out platform))
                if (platform.nodes.TryGetValue(hostName, out node))
                    return true;

            if (useDefaultIfNotFound)
            {
                platform = platforms["Default"];
                node = platform.nodes.First().Value;
            }
            else
            {
                platform = null;
                node = null;
            }

            return false;
        }
    }
}
