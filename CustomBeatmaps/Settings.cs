using System;
using System.IO;
using UnityEngine;

namespace CustomBeatmaps
{
    public class Settings
    {
        public string OsuSongPathOverride = "";
        public string Path { get; private set; }

        public Settings(string path)
        {
            Path = path;
        }
        
        public static Settings Load(string path)
        {
            Settings result;
            if (!File.Exists(path))
            {
                result = new Settings(path);
                File.WriteAllText(path, JsonUtility.ToJson(result, true));
            }
            else
            {
                result = JsonUtility.FromJson<Settings>(File.ReadAllText(path));
                result.Path = path;
            }

            return result;
        }
    }
}
