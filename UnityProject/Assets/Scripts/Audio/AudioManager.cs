using UnityEngine;
using SoccerGame.Core;

namespace SoccerGame.Audio
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource crowdSource;
        [SerializeField] private AudioSource bgmSource;

        [SerializeField] private AudioClip[] kickSounds;
        [SerializeField] private AudioClip[] tackleSounds;
        [SerializeField] private AudioClip whistleShort;
        [SerializeField] private AudioClip whistleLong;
        [SerializeField] private AudioClip goalCrowd;
        [SerializeField] private AudioClip crowdAmbient;
        [SerializeField] private AudioClip menuBGM;
        [SerializeField] private AudioClip matchBGM;

        [SerializeField] private float crowdMinVolume = 0.3f;
        [SerializeField] private float crowdMaxVolume = 1f;

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
            if (crowdSource == null) crowdSource = gameObject.AddComponent<AudioSource>();
            if (bgmSource == null) bgmSource = gameObject.AddComponent<AudioSource>();

            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.spatialBlend = 0f;

            crowdSource.playOnAwake = false;
            crowdSource.loop = true;
            crowdSource.spatialBlend = 0f;
            crowdSource.clip = crowdAmbient;
            crowdSource.volume = crowdMinVolume;
            if (crowdAmbient != null)
                crowdSource.Play();

            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
            bgmSource.spatialBlend = 0f;
        }

        private void OnEnable()
        {
            GameEvents.SubscribePlayerAction(HandlePlayerAction);
            GameEvents.SubscribeGoalScored(HandleGoalScored);
            GameEvents.SubscribeKickoff(HandleKickoff);
        }

        private void OnDisable()
        {
            GameEvents.UnsubscribePlayerAction(HandlePlayerAction);
            GameEvents.UnsubscribeGoalScored(HandleGoalScored);
            GameEvents.UnsubscribeKickoff(HandleKickoff);
        }

        public void PlayKick(float power)
        {
            if (sfxSource == null || kickSounds == null || kickSounds.Length == 0) return;

            AudioClip clip = kickSounds[Random.Range(0, kickSounds.Length)];
            float volume = Mathf.Clamp01(Mathf.Lerp(0.4f, 1f, power));
            sfxSource.pitch = Random.Range(0.95f, 1.05f);
            sfxSource.PlayOneShot(clip, volume);
        }

        public void PlayTackle()
        {
            if (sfxSource == null || tackleSounds == null || tackleSounds.Length == 0) return;

            AudioClip clip = tackleSounds[Random.Range(0, tackleSounds.Length)];
            sfxSource.pitch = Random.Range(0.9f, 1.1f);
            sfxSource.PlayOneShot(clip, 0.8f);
        }

        public void PlayWhistle(bool longWhistle)
        {
            if (sfxSource == null) return;

            AudioClip clip = longWhistle ? whistleLong : whistleShort;
            if (clip != null)
            {
                sfxSource.pitch = 1f;
                sfxSource.PlayOneShot(clip, 1f);
            }
        }

        public void PlayGoalCrowd()
        {
            if (crowdSource != null && goalCrowd != null)
                crowdSource.PlayOneShot(goalCrowd, 1f);
        }

        public void SetCrowdIntensity(float intensity)
        {
            if (crowdSource == null) return;
            crowdSource.volume = Mathf.Lerp(crowdMinVolume, crowdMaxVolume, Mathf.Clamp01(intensity));
        }

        public void PlayMenuBGM()
        {
            PlayBGM(menuBGM, 0.8f);
        }

        public void PlayMatchBGM()
        {
            PlayBGM(matchBGM, 0.5f);
        }

        private void PlayBGM(AudioClip clip, float volume)
        {
            if (bgmSource == null || clip == null) return;
            if (bgmSource.clip == clip && bgmSource.isPlaying) return;

            bgmSource.Stop();
            bgmSource.clip = clip;
            bgmSource.volume = volume;
            bgmSource.Play();
        }

        private void HandleGoalScored(string team)
        {
            PlayGoalCrowd();
        }

        private void HandleKickoff()
        {
            PlayWhistle(false);
        }

        private void HandlePlayerAction(string action)
        {
            switch (action)
            {
                case "Pass":
                case "Through":
                case "Cross":
                    PlayKick(0.5f);
                    break;
                case "Shoot":
                    PlayKick(1f);
                    break;
                case "Tackle":
                    PlayTackle();
                    break;
            }
        }
    }
}
