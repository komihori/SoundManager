using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System.Linq;
//#if UNITY_EDITOR
//using UnityEditor;
//#endif

namespace SoundManagement {
    public class AudioSoundManager : MonoBehaviour {
        public static AudioSoundManager Instance { get; private set; }
        public List<AudioClip> bgmClips = new List<AudioClip>();
        public List<AudioClip> seClips = new List<AudioClip>();
        [SerializeField] AudioSource bgm;
        private List<AudioSource> bgmAudioSources = new List<AudioSource>();
        [SerializeField] AudioSource seSource;
        private List<AudioSource> seAudioSources = new List<AudioSource>();
        public AudioMixerGroup bgmAMG, seAMG;
        [Range(0f, 1f)] public float bgmVolume = 1f;
        [Range(0f, 1f)] public float seVolume = 1f;
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
        public void _PlaySE(string clipName, float time = 0f) {
            AudioClip audio = seClips.FirstOrDefault(clip => clip.name == clipName);
            if (audio == null) {
                Debug.Log(clipName + "????????????????");
                return;
            } else {
                foreach (var audioSource in seAudioSources) {
                    if (!audioSource.isPlaying) {
                        audioSource.time = time;
                        audioSource.pitch = 1f;
                        audioSource.PlayOneShot(audio, seVolume);
                        return;
                    }
                }
                seAudioSources[0].pitch = 1f;
                seAudioSources[0].PlayOneShot(audio, seVolume);
            }
        }
        public void _PlayBGM(string clipName) {
            AudioClip audio = bgmClips.FirstOrDefault(clip => clip.name == clipName);
            if (audio == null) {
                Debug.Log(clipName + "????????????????");
                return;
            }
            bgm.clip = audio;
            bgm.volume = bgmVolume;
            bgm.loop = true;
            bgm.Play();
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
            SetBGMVolume(vol);
            SetSEVolume(vol);
        }
        public void SetBGMVolume(float vol) {
            foreach (var s in this.bgmAudioSources) {
                s.volume = vol;
            }
            bgm.volume = vol;
            bgmVolume = vol;
        }
        public void SetSEVolume(float vol) {
            foreach (var s in this.seAudioSources) {
                s.volume = vol;
            }
            seSource.volume = vol;
            seVolume = vol;
        }
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