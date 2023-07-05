using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoundManagement {
    public static class AudioSourceExternal {
        public static IEnumerator PlayWithFadeIn(this AudioSource source, AudioClip clip, float fadeTime = 0.1f, float volume = 1f) {
            source.clip = clip;
            source.Play();
            source.volume = 0f;
            float t = 0;
            while (t < 1) {
                t += Time.deltaTime / fadeTime;
                source.volume = Mathf.Min(volume * t, volume);
                yield return null;
            }
        }
        public static IEnumerator StopWithFadeOut(this AudioSource source, float fadeTime = 0.1f) {
            float volume = source.volume;
            float t = 0;
            while (t < 1) {
                t += Time.deltaTime / fadeTime;
                source.volume = Mathf.Min(volume * (1 - t), volume);
                yield return null;
            }
        }
    }
}