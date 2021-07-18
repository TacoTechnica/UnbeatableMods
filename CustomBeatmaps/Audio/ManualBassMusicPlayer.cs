using FMOD.Studio;
using ManagedBass;
using UnityEngine;

namespace CustomBeatmaps.Audio
{
    public class ManualBassMusicPlayer : IMusicPlayer
    {
        private static readonly int EmptyChannel = -1;
        private static bool _bassInitialized;

        private int _channel = EmptyChannel;
        private int _pitchShiftEffect = EmptyChannel;

        private float _volumeMultiplier;

        private static double OHNO_OFFSET_MS = 84; 

        public void Initialize(string filePath, float volumeMultipler = 1)
        {
            Release();
            if (!_bassInitialized)
            {
                _bassInitialized = Bass.Init(-1, AudioSettings.outputSampleRate);
                Bass.Configure(Configuration.TruePlayPosition, false);
            }
            _channel = Bass.CreateStream(filePath, Flags: BassFlags.Default | BassFlags.FxTempoAlgorithmCubic);
            //_pitchShiftEffect = Bass.ChannelSetFX(_channel, EffectType.PitchShift, 1000);

            _volumeMultiplier = volumeMultipler;
            Bass.ChannelSetAttribute(_channel, ChannelAttribute.Volume, volumeMultipler);
        }

        public void Start()
        {
            if (!IsValid()) return;
            Bass.ChannelPlay(_channel, true);
        }

        public void SetPaused(bool paused)
        {
            if (!IsValid()) return;
            bool realPaused = Bass.ChannelIsActive(_channel) == PlaybackState.Paused;
            if (realPaused)
            {
                if (!paused)
                {
                    Bass.ChannelPlay(_channel, false);
                }
            }
            else
            {
                if (paused)
                {
                    Bass.ChannelPause(_channel);
                }
            }
        }

        public void Stop(FMOD.Studio.STOP_MODE mode = FMOD.Studio.STOP_MODE.IMMEDIATE)
        {
            if (!IsValid()) return;
            Bass.ChannelStop(_channel);
        }

        public void Release()
        {
            if (!IsValid()) return;
            if (IsValid())
            {
                Bass.ChannelStop(_channel);
                Bass.StreamFree(_channel);
                _channel = EmptyChannel;
                _pitchShiftEffect = EmptyChannel;
            }
        }

        public void GetPlaybackState(out PLAYBACK_STATE state)
        {
            if (!IsValid())
            {
                state = PLAYBACK_STATE.STOPPED;
                return;
            }
            PlaybackState result = Bass.ChannelIsActive(_channel);
            switch (result)
            {
                case PlaybackState.Playing:
                    state = PLAYBACK_STATE.PLAYING;
                    return;
                case PlaybackState.Paused:
                case PlaybackState.Stopped:
                    state = PLAYBACK_STATE.STOPPED;
                    return;
            }
            state = PLAYBACK_STATE.STOPPED;
        }

        public void SetPitch(float pitch)
        {
            if (!IsValid()) return;

            //Bass.FXSetParameters(_pitchShiftEffect, new PitchShiftParameters() {fPitchShift = pitch});
            //Bass.ChannelSetAttribute(_channel, ChannelAttribute.Pitch, pitch);
            Bass.ChannelSetAttribute(_channel, ChannelAttribute.TempoFrequency, pitch);
        }

        public void GetTimelinePosition(out int position)
        {
            if (!IsValid())
            {
                position = 0;
                return;
            }
            position = (int) (OHNO_OFFSET_MS + 1000.0 * Bass.ChannelBytes2Seconds(_channel, Bass.ChannelGetPosition(_channel, PositionFlags.Bytes)));
        }

        public bool IsValid()
        {
            return _channel != EmptyChannel;
        }

        public void Update()
        {
            if (_bassInitialized)
            {
                // Match channel volume to music mixer volume
                float vol = JeffBezosController.musicVolume;
                //Mod.MusicMixerGroup.audioMixer.GetFloat("MusicVolume", out vol);
                Bass.ChannelSetAttribute(_channel, ChannelAttribute.Volume, _volumeMultiplier * vol);
            }
        }

        public void SeekTimelinePosition(int seek)
        {
            double seconds = ((double)seek - OHNO_OFFSET_MS) / 1000.0;
            Bass.ChannelSetPosition(_channel, Bass.ChannelSeconds2Bytes(_channel, seconds));
        }
    }
}
