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
        private readonly List<BeatmapInfo> _userBeatmaps = new();

        private readonly Dictionary<string, string> _userSongsToFilename = new();

        public UserBeatmapList(string userBeatmapPathName)
        {
            _userBeatmapPathName = userBeatmapPathName;
        }

        private string UserBeatmapPath => Application.streamingAssetsPath + "/" + _userBeatmapPathName;

        public IEnumerable<BeatmapInfo> UserBeatmaps => _userBeatmaps;

        public IEnumerable<string> SongNames => _userSongsToFilename.Keys;

        public string GetSongPath(string songName, bool relative = false)
        {
            var relativePath = _userSongsToFilename[songName];
            return relative ? relativePath : Application.streamingAssetsPath + "/" + relativePath;
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
                var difficultyStart = fileNameNoExt.LastIndexOf('[');
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
                foreach (var fpath in Directory.EnumerateFiles(UserBeatmapPath))
                    // .beatmap probably doesn't exist...
                    if (fpath.EndsWith(".bytes") || fpath.EndsWith(".osu") || fpath.EndsWith(".beatmap"))
                    {
                        var fileName = Path.GetFileNameWithoutExtension(fpath);
                        Debug.Log($"FOUND CUSTOM BEATMAP PATH: {fileName}");
                        string songName, difficulty;
                        if (TryExtractBeatmapName(fileName, out songName, out difficulty))
                        {
                            if (isDifficultySelectable.Invoke(difficulty))
                            {
                                var text = File.ReadAllText(fpath);
                                var info = new BeatmapInfo(new TextAsset(text), songName, difficulty);
                                _userBeatmaps.Add(info);
                                if (!_userSongsToFilename.ContainsKey(songName))
                                {
                                    // We have a new song, add it.
                                    var beatmapParserEngine = new BeatmapParserEngine();
                                    var beatmap = ScriptableObject.CreateInstance<Beatmap>();
                                    beatmapParserEngine.ReadBeatmap(info.text, ref beatmap);
                                    var songFilename = beatmap.general.audioFilename;
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