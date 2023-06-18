using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System.Linq;
//#if UNITY_EDITOR
//using UnityEditor;
//#endif

namespace SoundSystem {
    public class SoundManager : MonoBehaviour {
        public static SoundManager Instance { get; private set; }
        public List<AudioClip> bgmClips = new List<AudioClip>();
        public List<AudioClip> seClips = new List<AudioClip>();
        [SerializeField] AudioSource bgm;
        private List<AudioSource> bgmAudioSources = new List<AudioSource>();
        [SerializeField] AudioSource seSource;
        private List<AudioSource> seAudioSources = new List<AudioSource>();
        public AudioMixerGroup bgmAMG, seAMG;
        [Range(0f, 1f)] public float bgmVolume;
        [Range(0f, 1f)] public float seVolume;
        private void Awake() {
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(this);
            } else {
                Destroy(this);
                return;
            }
            bgm = InitializeAudioSource(this.gameObject, true, bgmAMG);
            seSource = InitializeAudioSource(this.gameObject, false, seAMG);
            bgmAudioSources = InitializeAudioSources(this.gameObject, true, bgmAMG, bgmClips.Count);
            seAudioSources = InitializeAudioSources(this.gameObject, false, seAMG, seClips.Count);
        }

        private AudioSource InitializeAudioSource(GameObject parentGameObject, bool isLoop = false, AudioMixerGroup amg = null) {
            AudioSource audio = parentGameObject.AddComponent<AudioSource>();
            audio.loop = isLoop;
            audio.playOnAwake = false;
            if (amg != null) {
                audio.outputAudioMixerGroup = amg;
            }
            return audio;
        }
        private List<AudioSource> InitializeAudioSources(GameObject parentGameObject, bool isLoop = false, AudioMixerGroup amg = null, int count = 1) {
            List<AudioSource> audioSources = new List<AudioSource>();
            for (int i = 0; i < count; i++) {
                AudioSource audioSource = InitializeAudioSource(parentGameObject, isLoop, amg);
                audioSources.Add(audioSource);
            }
            return audioSources;
        }
        public void _PlaySE(string clipName) {
            AudioClip audio = seClips.FirstOrDefault(clip => clip.name == clipName);
            if (audio == null) {
                Debug.Log(clipName + "????????????????");
                return;
            } else {
                foreach (var audioSource in seAudioSources) {
                    if (!audioSource.isPlaying) {
                        audioSource.pitch = 1f;
                        audioSource.Play(audio, seVolume);
                        return;
                    }
                }
                seAudioSources[0].pitch = 1f;
                seAudioSources[0].Play(audio, seVolume);

            }
        }
        public void _PlayBGM(string clipName) {
            AudioClip audio = bgmClips.FirstOrDefault(clip => clip.name == clipName);
            if (audio == null) {
                Debug.Log(clipName + "????????????????");
                return;
            }
            bgm.Play(audio, bgmVolume);
            Debug.Log($"PlayBGM:{audio.name}");
        }
        public void _StopAllBGM() {
            foreach (var s in this.bgmAudioSources) {
                s.Stop();
                Debug.Log($"Stop:{s.name}");
            }
            bgm.Stop();
        }
        public void _StopBGM(string clipName) {
            AudioSource audioSource = bgmAudioSources.FirstOrDefault(bas => bas.clip.name == clipName);
            if (audioSource == null || audioSource.isPlaying) {
                return;
            }
            audioSource.Stop();
        }
        public void _PlayBGMWithFadeIn(string clipName, float fadeTime = 2f) {
            AudioClip audioClip = bgmClips.FirstOrDefault(clip => clip.name == clipName);
            if (audioClip == null) {
                Debug.Log(clipName + "????????????????");
                return;
            }
            foreach (var audio in bgmAudioSources) {
                if (audio.clip == null) {
                    StartCoroutine(audio.PlayWithFadeIn(audioClip, fadeTime, bgmVolume));
                    return;
                }
            }
        }
        public void _StopBGMWithFadeOut(string clipName, float fadeTime = 2f) {
            AudioSource audioSource = bgmAudioSources.FirstOrDefault(bas => bas.clip.name == clipName);
            if (audioSource == null || audioSource.isPlaying == false) {
                Debug.Log(clipName + "????????????????????");
                return;
            }
            StartCoroutine(audioSource.StopWithFadeOut(fadeTime));
        }

        public void SetVolume(float vol) {
            foreach (var s in this.bgmAudioSources) {
                s.volume = vol;
            }
            foreach (var s in this.seAudioSources) {
                s.volume = vol;
            }
            bgm.volume = vol;
            seSource.volume = vol;
            bgmVolume = vol;
            seVolume = vol;
        }

        public static void PlaySE(string clipName) => SoundManager.Instance._PlaySE(clipName);
        public static void PlayBGM(string clipName) => SoundManager.Instance._PlayBGM(clipName);
        public static void PlayBGMWithFadeIn(string clipName, float fadeTime) => SoundManager.Instance._PlayBGMWithFadeIn(clipName, fadeTime);
        public static void StopBGMWithFadeOut(string clipName, float fadeTime) => SoundManager.Instance._StopBGMWithFadeOut(clipName, fadeTime);
    }

//#if UNITY_EDITOR
//    [CustomEditor(typeof(SoundManager))]
//    public class SoundManagerEditor : Editor {
//        [SerializeField] string bgmClipName;
//        [SerializeField] float fadeTime = 1;
//        [SerializeField] bool fade;
//        public override void OnInspectorGUI() {
//            base.OnInspectorGUI();
            

//            var _target = target as SoundManager;
//            EditorGUILayout.LabelField("--- Editor ---");
//            this.bgmClipName = EditorGUILayout.TextField("BGM Clip Name", this.bgmClipName);
//            this.fade = EditorGUILayout.ToggleLeft("Fade", this.fade);
//            if (!this.fade) {
//                if (GUILayout.Button("Play")) {
//                    _target._PlayBGM(this.bgmClipName);
//                }
                
//            } else {
//                this.fadeTime = EditorGUILayout.FloatField(this.fadeTime);
//                if (GUILayout.Button("Play")) {
//                    _target._PlayBGMWithFadeIn(this.bgmClipName, this.fadeTime);
//                }
//            }
//            if (GUILayout.Button("Stop")) {
//                _target._StopBGM(bgmClipName);
//            }
//            if (GUILayout.Button("StopAll")) {
//                _target._StopAllBGM();
//            }
//        }
//    }
//#endif

}