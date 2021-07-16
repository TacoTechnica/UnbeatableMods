using System.Collections;
using FMOD.Studio;
using UnityEngine;

namespace CustomBeatmaps.Audio
{
    public class ManualMusicPlayer
    {
        private DummyCoroutineRunner _coroutineRunner;
        private string _filePath;

        private GameObject _gameObject;
        private Coroutine _loadPlayRoutine;
        private bool _paused;
        private AudioSource _source;

        public void Initialize(string filePath, float volumeMultipler = 1f)
        {
            Release();
            _filePath = filePath;
            _gameObject = new GameObject();
            _coroutineRunner = _gameObject.AddComponent<DummyCoroutineRunner>();
            _source = _gameObject.AddComponent<AudioSource>();
            var mixer = Object.FindObjectOfType<JeffBezosController>().masterMixer;
            /*
             * Groups: Master, Music, Environment, SFX, UI
             */
            _source.outputAudioMixerGroup = mixer.FindMatchingGroups("Music")[0];
            _source.volume = volumeMultipler;
        }

        public void Start()
        {
            if (_loadPlayRoutine != null) _coroutineRunner.StopCoroutine(_loadPlayRoutine);
            _loadPlayRoutine = _coroutineRunner.StartCoroutine(PlayBackgroundMusic(_filePath));
        }

        public void SetPaused(bool paused)
        {
            if (_source != null)
            {
                if (paused)
                {
                    _source.Pause();
                }
                else
                {
                    if (!_source.isPlaying) _source.UnPause();
                }
            }

            _paused = paused;
        }

        public void Stop(FMOD.Studio.STOP_MODE mode = FMOD.Studio.STOP_MODE.IMMEDIATE)
        {
            if (_source != null) _source.Stop();
        }

        public void Release()
        {
            if (_coroutineRunner != null) _coroutineRunner.StopAllCoroutines();
            if (_source != null)
            {
                _source.Stop();
                if (_source.clip != null) _source.clip.UnloadAudioData();
            }

            if (_gameObject != null)
            {
                Object.Destroy(_gameObject);
                _gameObject = null;
            }
        }

        public void GetPlaybackState(out PLAYBACK_STATE state)
        {
            state = _source.isPlaying ? PLAYBACK_STATE.PLAYING : PLAYBACK_STATE.STOPPED;
        }

        public void SetPitch(float pitch)
        {
            _source.pitch = pitch;
        }

        public void GetTimelinePosition(out int position)
        {
            position = (int) (_source.time * 1000);
        }

        public bool IsValid()
        {
            return _source != null;
        }

        private IEnumerator PlayBackgroundMusic(string file)
        {
#pragma warning disable 618
            var www = new WWW($"file://{file}");
#pragma warning restore 618
            yield return www;
            _source.clip = www.GetAudioClip(false, false);
            // Loading happens asynchronously, so we play only if we're not paused now.
            _source.Play();
            if (_paused) _source.Pause();
            _loadPlayRoutine = null;
        }

        private class DummyCoroutineRunner : MonoBehaviour
        {
        }
    }
}