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
        [SerializeField] List<AudioClip> _bgmClips = new List<AudioClip>();
        [SerializeField] List<AudioClip> _seClips = new List<AudioClip>();
        [SerializeField] AudioSource _bgm = null;
        [SerializeField] AudioSource _seSource = null;
        [SerializeField] AudioMixerGroup _masterAMG = null;
        [SerializeField] AudioMixerGroup _bgmAMG = null;
        [SerializeField] AudioMixerGroup _seAMG = null;
        private List<AudioSource> _bgmAudioSources = new List<AudioSource>();
        private List<AudioSource> _seAudioSources = new List<AudioSource>();
        private void Awake() {
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(this);
            } else {
                Destroy(this);
                return;
            }
            _bgm = InitializeAudioSource(this.gameObject, true, _bgmAMG);
            _seSource = InitializeAudioSource(this.gameObject, false, _seAMG);
            _bgmAudioSources = InitializeAudioSources(this.gameObject, true, _bgmAMG, _bgmClips.Count);
            _seAudioSources = InitializeAudioSources(this.gameObject, false, _seAMG, _seClips.Count);
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
        public void PlaySE(string clipName) {
            AudioClip audio = _seClips.FirstOrDefault(clip => clip.name == clipName);
            if (audio == null) {
                Debug.LogWarning($"{clipName}が見つかりません.");
                return;
            } else {
                foreach (var audioSource in _seAudioSources) {
                    if (!audioSource.isPlaying) {
                        audioSource.pitch = 1f;
                        audioSource.PlayOneShot(audio);
                        return;
                    }
                }
                _seAudioSources[0].pitch = 1f;
                _seAudioSources[0].PlayOneShot(audio);
            }
        }
        public void PlayBGM(string clipName) {
            AudioClip audio = _bgmClips.FirstOrDefault(clip => clip.name == clipName);
            if (audio == null) {
                Debug.LogWarning($"{clipName}が見つかりません.");
                return;
            }
            _bgm.clip = audio;
            _bgm.loop = true;
            _bgm.Play();
        }
        public void StopAllBGM() {
            foreach (var s in this._bgmAudioSources) {
                s.Stop();
            }
            _bgm.Stop();
        }
        public void StopBGM(string clipName) {
            AudioSource audioSource = FindAudioSourceBGM(clipName);
            if (audioSource == null || audioSource.isPlaying) {
                return;
            }
            audioSource.Stop();
            audioSource.clip = null;
        }
        public void PlayBGMWithFadeIn(string clipName, float fadeTime = 2f) {
            AudioClip audioClip = FindAudioClipBGM(clipName);
            if (audioClip == null) {
                Debug.LogWarning($"{clipName}が見つかりません.");
                return;
            }
            var source = AllocateAudioSourceBGM();
            StartCoroutine(source.PlayWithFadeIn(audioClip, fadeTime));
        }
        public void StopBGMWithFadeOut(string clipName, float fadeTime = 2f) {
            AudioSource audioSource = _bgmAudioSources.FirstOrDefault(bas => bas.clip.name == clipName);
            if (audioSource == null || audioSource.isPlaying == false) {
                Debug.LogWarning($"{clipName}が見つかりません.");
                return;
            }
            StartCoroutine(audioSource.StopWithFadeOut(fadeTime));
        }

        public void SetMasterVolume(float vol) {
            vol = Mathf.Clamp(Mathf.Log10(vol) * 20f, -80f, 0f);
            _masterAMG.audioMixer.SetFloat("Master", vol);
        }
        public void SetBGMVolume(float vol) {
            vol = Mathf.Clamp(Mathf.Log10(vol) * 20f, -80f, 0f);
            _bgmAMG.audioMixer.SetFloat("BGM", vol);
        }
        public void SetSEVolume(float vol) {
            vol = Mathf.Clamp(Mathf.Log10(vol) * 20f, -80f, 0f);
            _seAMG.audioMixer.SetFloat("SE", vol);
        }

        private AudioSource FindAudioSourceBGM(string clipName) {
            return _bgmAudioSources.FirstOrDefault(s => s.clip.name == clipName);
        }
        private AudioClip FindAudioClipBGM(string clipName) {
            foreach (var c in _bgmClips) {
                if (c.name == clipName) return c;
            }
            return null;
        }
        private AudioSource AllocateAudioSourceBGM() {
            foreach (var s in _bgmAudioSources) {
                if (s.clip == null) return s;
            }
            return null;
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