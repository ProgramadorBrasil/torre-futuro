using UnityEngine;
using System.Collections.Generic;

namespace SpaceRPG.Systems
{
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;
        public static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<AudioManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("AudioManager");
                        _instance = go.AddComponent<AudioManager>();
                    }
                }
                return _instance;
            }
        }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource ambientSource;

        [Header("Music")]
        [SerializeField] private AudioClip mainTheme;
        [SerializeField] private AudioClip shopTheme;
        [SerializeField] private AudioClip combatTheme;
        [SerializeField] private List<AudioClip> musicPlaylist = new List<AudioClip>();

        [Header("Settings")]
        [SerializeField] private float masterVolume = 1f;
        [SerializeField] private float musicVolume = 0.7f;
        [SerializeField] private float sfxVolume = 1f;
        [SerializeField] private float ambientVolume = 0.5f;

        [Header("Pooling")]
        [SerializeField] private int sfxPoolSize = 10;
        private List<AudioSource> sfxPool = new List<AudioSource>();

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }

        private void Start()
        {
            PlayMusic(mainTheme);
        }

        private void InitializeAudioSources()
        {
            if (musicSource == null)
            {
                GameObject musicGO = new GameObject("MusicSource");
                musicGO.transform.SetParent(transform);
                musicSource = musicGO.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                GameObject sfxGO = new GameObject("SFXSource");
                sfxGO.transform.SetParent(transform);
                sfxSource = sfxGO.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }

            if (ambientSource == null)
            {
                GameObject ambientGO = new GameObject("AmbientSource");
                ambientGO.transform.SetParent(transform);
                ambientSource = ambientGO.AddComponent<AudioSource>();
                ambientSource.loop = true;
                ambientSource.playOnAwake = false;
            }

            // Criar pool de AudioSources para SFX
            for (int i = 0; i < sfxPoolSize; i++)
            {
                GameObject poolGO = new GameObject($"SFXPool_{i}");
                poolGO.transform.SetParent(transform);
                AudioSource source = poolGO.AddComponent<AudioSource>();
                source.playOnAwake = false;
                sfxPool.Add(source);
            }

            UpdateVolumes();
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (clip == null || musicSource == null) return;

            if (musicSource.clip == clip && musicSource.isPlaying) return;

            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }

        public void PlaySFX(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null) return;

            AudioSource availableSource = GetAvailableSFXSource();
            if (availableSource != null)
            {
                availableSource.PlayOneShot(clip, volumeScale * sfxVolume);
            }
            else
            {
                sfxSource.PlayOneShot(clip, volumeScale * sfxVolume);
            }
        }

        public void PlayAmbient(AudioClip clip)
        {
            if (clip == null || ambientSource == null) return;

            ambientSource.clip = clip;
            ambientSource.Play();
        }

        private AudioSource GetAvailableSFXSource()
        {
            foreach (var source in sfxPool)
            {
                if (!source.isPlaying)
                    return source;
            }
            return null;
        }

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        public void SetAmbientVolume(float volume)
        {
            ambientVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        private void UpdateVolumes()
        {
            if (musicSource != null)
                musicSource.volume = masterVolume * musicVolume;

            if (ambientSource != null)
                ambientSource.volume = masterVolume * ambientVolume;
        }

        public void StopMusic() => musicSource?.Stop();
        public void PauseMusic() => musicSource?.Pause();
        public void ResumeMusic() => musicSource?.UnPause();
        public void StopAllSFX()
        {
            sfxSource?.Stop();
            foreach (var source in sfxPool)
                source.Stop();
        }

        public void StopAmbient() => ambientSource?.Stop();
        public void StopAll()
        {
            StopMusic();
            StopAllSFX();
            StopAmbient();
        }
    }
}
