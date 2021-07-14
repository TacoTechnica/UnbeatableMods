using System;
using System.Collections.Generic;
using System.IO;
using Rhythm;
using UnityEngine;

namespace CustomBeatmaps
{
    public class UserBeatmapList
    {
        private readonly string _userBeatmapPathName;
        private string UserBeatmapPath => Application.streamingAssetsPath + "/" + _userBeatmapPathName;

        private readonly Dictionary<string, string> _userSongsToFilename = new Dictionary<string, string>();
        private readonly List<BeatmapInfo> _userBeatmaps = new List<BeatmapInfo>();

        public IEnumerable<BeatmapInfo> UserBeatmaps => _userBeatmaps;

        public IEnumerable<string> SongNames => _userSongsToFilename.Keys;

        public string GetSongPath(string songName, bool relative=false)
        {
            string relativePath = _userSongsToFilename[songName]; 
            return relative? relativePath : Application.streamingAssetsPath + "/" + relativePath;
        }

        public UserBeatmapList(string userBeatmapPathName)
        {
            _userBeatmapPathName = userBeatmapPathName;
        }

        public bool IsUserSong(string songName)
        {
            return _userSongsToFilename.ContainsKey(songName);
        }

        private static bool TryExtractBeatmapName(string fileNameNoExt, out string songName, out string difficulty)
        {
            songName = "";
            difficulty = "";
            if (fileNameNoExt.EndsWith("]"))
            {
                int difficultyStart = fileNameNoExt.LastIndexOf('[');
                if (difficultyStart != -1)
                {
                    songName = fileNameNoExt.Substring(0, difficultyStart);
                    difficulty =
                        fileNameNoExt.Substring(difficultyStart + 1, fileNameNoExt.Length - difficultyStart - 2);
                    return true;
                }
            }

            return false;
        }

        public void RefreshUserBeatmaps(Predicate<string> isDifficultySelectable)
        {
            if (Directory.Exists(UserBeatmapPath))
            {
                _userBeatmaps.Clear();
                foreach (string fpath in Directory.EnumerateFiles(UserBeatmapPath))
                {
                    // .beatmap probably doesn't exist...
                    if (fpath.EndsWith(".bytes") || fpath.EndsWith(".osu") || fpath.EndsWith(".beatmap"))
                    {
                        string fileName = Path.GetFileNameWithoutExtension(fpath);
                        Debug.Log($"FOUND CUSTOM BEATMAP PATH: {fileName}");
                        string songName, difficulty;
                        if (TryExtractBeatmapName(fileName, out songName, out difficulty))
                        {
                            if (isDifficultySelectable.Invoke(difficulty))
                            {
                                string text = File.ReadAllText(fpath);
                                BeatmapInfo info = new BeatmapInfo(new TextAsset(text), songName, difficulty);
                                _userBeatmaps.Add(info);
                                if (!_userSongsToFilename.ContainsKey(songName))
                                {
                                    // We have a new song, add it.
                                    BeatmapParserEngine beatmapParserEngine = new BeatmapParserEngine();
                                    Beatmap beatmap = ScriptableObject.CreateInstance<Beatmap>();
                                    beatmapParserEngine.ReadBeatmap(info.text, ref beatmap);
                                    string songFilename = beatmap.general.audioFilename;
                                    _userSongsToFilename[songName] = songFilename;
                                }
                            }
                            else
                            {
                                Debug.LogWarning($"Invalid difficulty found: {difficulty}");
                            }
                        }
                        else
                        {
                            Debug.LogWarning(
                                $"Invalid beatmap filename: {fileName}. Format needed: \"SONG NAME[DIFFICULTY]\"");
                        }
                    }
                }
            }
        }
    }
}