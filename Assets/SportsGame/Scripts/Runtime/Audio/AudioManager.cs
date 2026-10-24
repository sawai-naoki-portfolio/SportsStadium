using System.Collections.Generic;
using UnityEngine;

namespace SportsGame
{
    /// <summary>効果音・環境音・BGM の再生。AppBootstrap が常駐させる。</summary>
    public class AudioManager : MonoBehaviour
    {
        const int VoiceCount = 14;
        const float AmbienceFadeSpeed = 0.5f;
        const float MusicFadeSpeed = 0.8f;
        const float MusicGain = 0.7f;

        static AudioManager instance;

        readonly Dictionary<Sfx, AudioClip> clips = new Dictionary<Sfx, AudioClip>();
        readonly List<AudioSource> voices = new List<AudioSource>();
        AudioSource ambience, music;
        AudioClip crowdLoop, titleMusic;
        float ambienceTarget, musicTarget;
        int nextVoice;

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                return;
            }
            instance = this;
            for (int i = 0; i < VoiceCount; i++) voices.Add(CreateSource(false));
            ambience = CreateSource(true);
            music = CreateSource(true);
        }

        AudioSource CreateSource(bool loop)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 0f;
            src.loop = loop;
            src.volume = loop ? 0f : 1f;
            return src;
        }

        AudioClip Clip(Sfx s)
        {
            if (!clips.TryGetValue(s, out var c) || c == null)
            {
                c = SfxSynth.Create(s);
                clips[s] = c;
            }
            return c;
        }

        /// <summary>よく使う効果音を事前に生成しておく(初回再生時のカクつき防止)。</summary>
        public static void Warmup(params Sfx[] list)
        {
            if (instance == null) return;
            foreach (var s in list) instance.Clip(s);
        }

        public static void Play(Sfx s, float volume = 1f, float pitch = 1f)
        {
            if (instance == null) return;
            float v = Mathf.Clamp01(volume * GameSettings.SeVolume);
            if (v <= 0.001f) return;
            var src = instance.voices[instance.nextVoice];
            instance.nextVoice = (instance.nextVoice + 1) % instance.voices.Count;
            src.clip = instance.Clip(s);
            src.volume = v;
            src.pitch = Mathf.Clamp(pitch, 0.5f, 2f);
            src.Play();
        }

        public static void SetAmbience(bool on, float volume = 0.3f)
        {
            if (instance == null) return;
            if (on && instance.crowdLoop == null) instance.crowdLoop = MusicSynth.CrowdLoop();
            instance.ambienceTarget = on ? Mathf.Clamp01(volume) : 0f;
            if (on && !instance.ambience.isPlaying)
            {
                instance.ambience.clip = instance.crowdLoop;
                instance.ambience.Play();
            }
        }

        public static void SetTitleMusic(bool on)
        {
            if (instance == null) return;
            if (on)
            {
                if (instance.titleMusic == null) instance.titleMusic = MusicSynth.TitleMusic();
                if (instance.music.clip != instance.titleMusic || !instance.music.isPlaying)
                {
                    instance.music.clip = instance.titleMusic;
                    instance.music.Play();
                }
            }
            instance.musicTarget = on ? 1f : 0f;
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            Fade(ambience, ambienceTarget * GameSettings.SeVolume, AmbienceFadeSpeed * dt, ambienceTarget);
            Fade(music, musicTarget * GameSettings.BgmVolume * MusicGain, MusicFadeSpeed * dt, musicTarget);
        }

        static void Fade(AudioSource src, float target, float step, float rawTarget)
        {
            src.volume = Mathf.MoveTowards(src.volume, target, step);
            if (src.isPlaying && src.volume <= 0.001f && rawTarget <= 0f) src.Stop();
        }
    }
}
