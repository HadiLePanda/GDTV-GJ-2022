using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace GameJam
{
    public static class MixerHelper
    {
        public static void SetVolume(AudioMixer audioMixer, string exposedParam, float targetVolume)
        {
            targetVolume = Mathf.Clamp(targetVolume, 0.0001f, 1);
            float targetValue = Mathf.Log10(targetVolume) * 20;

            audioMixer.SetFloat(exposedParam, targetValue);
        }

        public static IEnumerator StartFade(AudioMixer audioMixer, string exposedParam, float targetVolume, float duration)
        {
            float currentTime = 0;
            float currentVolume;
            audioMixer.GetFloat(exposedParam, out currentVolume);
            currentVolume = Mathf.Pow(10, currentVolume / 20);
            float targetValue = Mathf.Clamp(targetVolume, 0.0001f, 1);

            while (currentTime < duration)
            {
                currentTime += Time.deltaTime;
                float newVolume = Mathf.Lerp(currentVolume, targetValue, currentTime / duration);

                audioMixer.SetFloat(exposedParam, Mathf.Log10(newVolume) * 20);

                yield return null;
            }
            yield break;
        }
    }
}