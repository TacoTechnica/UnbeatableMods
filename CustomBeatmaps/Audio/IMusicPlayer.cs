using FMOD.Studio;

namespace CustomBeatmaps.Audio
{
    public interface IMusicPlayer
    {
        void Initialize(string filePath, float volumeMultipler = 1f);
        void Start();
        void SetPaused(bool paused);
        void Stop(FMOD.Studio.STOP_MODE mode = FMOD.Studio.STOP_MODE.IMMEDIATE);
        void Release();
        void GetPlaybackState(out PLAYBACK_STATE state);
        void SetPitch(float pitch);
        void GetTimelinePosition(out int position);
        bool IsValid();
        void Update();
        void SeekTimelinePosition(int seek);
    }
}