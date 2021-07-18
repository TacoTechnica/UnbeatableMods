using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace CustomBeatmaps.OsuReading
{
    public class OsuReader
    {

        public bool SongsFolderFound => Directory.Exists(SongsFolder);
        public string SongsFolder => string.IsNullOrEmpty(Mod.Instance.Settings.OsuSongPathOverride) ? Path.GetFullPath(Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData).Replace('\\', '/'), "../Local/osu!/Songs")) : Mod.Instance.Settings.OsuSongPathOverride;

        public string[] GetBeatmapList()
        {
            if (SongsFolderFound)
            {
                List<string> beatmaps = new List<string>();
                foreach (string osuProjectDir in Directory.EnumerateDirectories(SongsFolder))
                {
                    foreach (string file in Directory.EnumerateFiles(osuProjectDir))
                    {
                        if (file.EndsWith(".osu"))
                        {
                            beatmaps.Add(file);
                        }
                    }
                }

                double TimeSinceLastWrite(string filename)
                {
                    return (File.GetLastWriteTime(filename) - DateTime.Now).TotalSeconds;
                }

                // Sort by newest access
                beatmaps.Sort((left, right) => -1 * Math.Sign(TimeSinceLastWrite(left) - TimeSinceLastWrite(right)));

                return beatmaps.ToArray();
            }
            return new string[0];
        }
    }
}
