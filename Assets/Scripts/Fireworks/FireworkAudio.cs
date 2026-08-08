using System.Collections;
using UnityEngine;

namespace Fireworks
{
    public sealed class FireworkAudio : MonoBehaviour
    {
        [SerializeField]
        private AudioClip _launchClip;

        [SerializeField]
        private AudioClip _explosionClip;

        [SerializeField, Min(0f)]
        private float _soundDelay = 0.5f;

        [SerializeField, Range(0f, 1f)]
        private float _minimumLaunchVolume = 0.2f;

        [SerializeField, Range(0f, 1f)]
        private float _maximumLaunchVolume = 0.5f;

        [SerializeField, Range(0f, 1f)]
        private float _minimumExplosionVolume = 0.45f;

        [SerializeField, Range(0f, 1f)]
        private float _maximumExplosionVolume = 1f;

        [SerializeField, Range(0f, 1f)]
        private float _launchSoundDurationRatio = 0.5f;

        [SerializeField, Min(0f)]
        private float _silenceBeforeExplosion = 0.05f;

        public void PlayLaunch(float sizeRatio, float launchDuration)
        {
            float maximumDuration = Mathf.Min(
                launchDuration * _launchSoundDurationRatio,
                launchDuration - _silenceBeforeExplosion);
            maximumDuration = Mathf.Max(0f, maximumDuration);
            if (maximumDuration <= 0f)
            {
                return;
            }

            Play(
                _launchClip,
                sizeRatio,
                _minimumLaunchVolume,
                _maximumLaunchVolume,
                maximumDuration);
        }

        public void PlayExplosion(float sizeRatio)
        {
            Play(
                _explosionClip,
                sizeRatio,
                _minimumExplosionVolume,
                _maximumExplosionVolume,
                float.MaxValue);
        }

        private void Play(
            AudioClip clip,
            float sizeRatio,
            float minimumVolume,
            float maximumVolume,
            float maximumDuration)
        {
            if (clip == null)
            {
                return;
            }

            float volume = Mathf.Lerp(minimumVolume, maximumVolume, sizeRatio);
            StartCoroutine(PlayAfterDelay(clip, _soundDelay, volume, maximumDuration));
        }

        private IEnumerator PlayAfterDelay(AudioClip clip, float delay, float volume, float maximumDuration)
        {
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            GameObject audioObject = new GameObject("FireworkAudio");
            AudioSource audioSource = audioObject.AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.spatialBlend = 0f;
            audioSource.Play();

            float playbackDuration = Mathf.Min(clip.length, maximumDuration);
            if (playbackDuration < clip.length)
            {
                float elapsedTime = 0f;
                while (elapsedTime < playbackDuration)
                {
                    elapsedTime += Time.deltaTime;
                    audioSource.volume = Mathf.Lerp(volume, 0f, elapsedTime / playbackDuration);
                    yield return null;
                }
            }
            else
            {
                yield return new WaitForSeconds(playbackDuration);
            }

            Destroy(audioObject);
        }
    }
}
