using UnityEngine;
using UnityEngine.Audio;

namespace GameJam
{
    public class AudioManager : SingletonMonoBehaviour<AudioManager>
    {
        static readonly string MIXER_MASTER_VOLUME = "MasterVolume";
        static readonly string MIXER_BACKGROUND_VOLUME = "BackgroundVolume";

        static readonly string PREF_MASTER_VOLUME = "MasterVolume";

        [Header("Mixers")]
        [SerializeField] private AudioMixer mainMixer;
        [SerializeField] private AudioMixer backgroundMixer;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource ambientSource;

        [Header("References")]
        [SerializeField] private AudioSource worldSfxPrefab;

        [Header("Settings")]
        [SerializeField][Range(0f, 1f)] private float defaultMasterVolume = 1f;
        [Tooltip("In seconds")]
        [SerializeField] private float musicFadeTime = 2f;

        [Header("Spawning")]
        [SerializeField] private Transform audioParent;

        private float masterVolumeSetting;
        private Coroutine backgroundRoutine;

        protected override void Awake()
        {
            base.Awake();

            LoadSavedVolumeSettings();
        }

        // sfx =========================================================
        /// <summary>
        /// Play a 3D sound effect at the specified position
        /// </summary>
        public AudioSource PlayWorldSfx(AudioClip clip, Vector3 position)
        {
            // /!\ sfx prefab should have an auto destroy component
            AudioSource soundInstance = Instantiate(worldSfxPrefab, position, Quaternion.identity);

            soundInstance.clip = clip;
            soundInstance.Play();
            soundInstance.GetComponent<AutoDestroy>().SetTimer(clip.length);

            return soundInstance;
        }

        /// <summary>
        /// Play a random 3D sound effect from the specified audioSource
        /// </summary>
        public AudioClip PlayRandomSfxFromSource(AudioClip[] clips, AudioSource audioSource)
        {
            AudioClip randomClip = Utils.GetRandomClip(clips);

            audioSource.PlayOneShot(randomClip);
            return randomClip;
        }

        /// <summary>
        /// Play a 2D sound effect
        /// </summary>
        public void PlaySfx(AudioClip clip) => sfxSource.PlayOneShot(clip);

        // music & ambient =============================================
        public void PlayBackgroundAudio(AudioClip musicClip, AudioClip ambientClip)
        {
            PlayMusic(musicClip);
            PlayAmbient(ambientClip);

            // fade in background volume
            float targetVolume = 1f;
            if (backgroundRoutine != null)
            {
                StopCoroutine(backgroundRoutine);
            }
            MixerHelper.SetVolume(backgroundMixer, MIXER_BACKGROUND_VOLUME, 0f);
            backgroundRoutine = StartCoroutine(MixerHelper.StartFade(backgroundMixer, MIXER_BACKGROUND_VOLUME, targetVolume, musicFadeTime));
        }
        public void StopBackgroundAudio()
        {
            StopMusic();
            StopAmbient();
        }

        public void PlayMusic(AudioClip musicClip)
        {
            musicSource.Stop();

            if (musicClip == null) { return; }

            musicSource.clip = musicClip;
            musicSource.loop = true;
            musicSource.Play();
            
        }
        public void PlayAmbient(AudioClip ambientClip)
        {
            ambientSource.Stop();

            if (ambientClip == null) { return; }

            ambientSource.clip = ambientClip;
            ambientSource.loop = true;
            ambientSource.Play();
        }
        public void StopMusic()
        {
            musicSource.Stop();
        }
        public void StopAmbient()
        {
            ambientSource.Stop();
        }

        // settings - Volume ===========================================
        public float GetSettingMasterVolume() => masterVolumeSetting;

        public void SetMasterVolume(float volume)
        {
            float targetValue = Mathf.Clamp(volume, 0.0001f, 1f);

            MixerHelper.SetVolume(mainMixer, MIXER_MASTER_VOLUME, targetValue);

            masterVolumeSetting = volume;
            SaveMasterVolume(volume);
        }

        #region saving / loading audio settings
        public void SaveMasterVolume(float volume) => PlayerPrefs.SetFloat(PREF_MASTER_VOLUME, volume);

        public void LoadSavedVolumeSettings()
        {
            float masterVolume = PlayerPrefs.HasKey(PREF_MASTER_VOLUME)
                ? PlayerPrefs.GetFloat(PREF_MASTER_VOLUME)
                : defaultMasterVolume;

            this.masterVolumeSetting = masterVolume;
        }
        #endregion
    }
}