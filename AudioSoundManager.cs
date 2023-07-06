using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SoundManagement {
    public class AudioSoundManager : MonoBehaviour {
        public static AudioSoundManager Instance { get; private set; }
        [SerializeField] List<AudioClip> _bgmClips = new List<AudioClip>();
        [SerializeField] List<AudioClip> _seClips = new List<AudioClip>();
        [SerializeField] AudioMixerGroup _masterAMG = null;
        [SerializeField] AudioMixerGroup _bgmAMG = null;
        [SerializeField] AudioMixerGroup _seAMG = null;
        private List<AudioSource> _bgmAudioSources = new List<AudioSource>();
        private List<AudioSource> _seAudioSources = new List<AudioSource>();
        void Awake() {
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(this);
            } else {
                Destroy(this);
                return;
            }
            _bgmAudioSources = InitializeAudioSources(this.gameObject, true, _bgmAMG, 2);
            _seAudioSources = InitializeAudioSources(this.gameObject, false, _seAMG, 5);
        }
        private static AudioSource InitializeAudioSource(GameObject parentGameObject, bool isLoop = false, AudioMixerGroup amg = null) {
            AudioSource audio = parentGameObject.AddComponent<AudioSource>();
            audio.loop = isLoop;
            audio.playOnAwake = false;
            if (amg != null) {
                audio.outputAudioMixerGroup = amg;
            }
            return audio;
        }
        private static List<AudioSource> InitializeAudioSources(GameObject parentGameObject, bool isLoop = false, AudioMixerGroup amg = null, int count = 1) {
            List<AudioSource> audioSources = new List<AudioSource>();
            for (int i = 0; i < count; i++) {
                AudioSource audioSource = InitializeAudioSource(parentGameObject, isLoop, amg);
                audioSources.Add(audioSource);
            }
            return audioSources;
        }
        public AudioSource PlaySE(string clipName) {
            AudioClip clip = _seClips.FirstOrDefault(clip => clip.name == clipName);
            if (clip == null) {
                Debug.LogWarning($"{clipName}が見つかりません.");
                return null;
            } else {
                AudioSource source = _seAudioSources.FirstOrDefault(s => !s.isPlaying);
                if (source == null) {
                    AudioSource temp = InitializeAudioSource(this.gameObject, false, _seAMG);
                    _seAudioSources.Add(temp);
                    source = temp;
                }
                source.volume = 1f;
                source.PlayOneShot(clip);
                return source;
            }
        }
        public AudioSource PlayBGM(string clipName, float fadeTime = 0f, bool stopOther = true) {
            AudioClip clip = _bgmClips.FirstOrDefault(c => c.name == clipName);
            if (clip == null) {
                Debug.LogWarning($"{clipName}が見つかりません.");
                return null;
            }
            AudioSource source = _bgmAudioSources.FirstOrDefault(s => s.clip == null);
            if (source == null) {
                AudioSource temp = InitializeAudioSource(this.gameObject, true, _bgmAMG);
                _bgmAudioSources.Add(temp);
                source = temp;
            }
            if (fadeTime > 0f)
                StartCoroutine(source.PlayWithFadeIn(clip, fadeTime));
            else {
                source.clip = clip;
                source.volume = 1f;
                source.Play();
            }
            if (stopOther) {
                foreach (var s in _bgmAudioSources)
                    if (s != source) StopBGM(s, fadeTime);
            }
            return source;
        }
        public void StopBGM(string clipName, float fadeTime = 0f) {
            AudioSource source = _bgmAudioSources.FirstOrDefault(bas => bas.clip.name == clipName);
            if (source == null || source.isPlaying == false) {
                return;
            }
            StopBGM(source, fadeTime);
        }
        public void StopBGM(AudioSource source, float fadeTime = 0f) {
            if (fadeTime > 0f) StartCoroutine(source.StopWithFadeOut(fadeTime));
            else {
                source.Stop();
                source.clip = null;
            }
        }
        public void StopAllBGM(float fadeTime = 1f) {
            foreach (var s in _bgmAudioSources) if (s != null) StopBGM(s, fadeTime);
        }
        public void SetMasterVolume(float vol) {
            _masterAMG.audioMixer.SetFloat("Master", Vol2Db(vol));
        }
        public void SetBGMVolume(float vol) {
            _bgmAMG.audioMixer.SetFloat("BGM", Vol2Db(vol));
        }
        public void SetSEVolume(float vol) {
            _seAMG.audioMixer.SetFloat("SE", Vol2Db(vol));
        }
        public float GetMasterVolume() {
            float db;
            _masterAMG.audioMixer.GetFloat("Master", out db);
            return Db2Vol(db);
        }
        public float GetBGMVolume() {
            float db;
            _bgmAMG.audioMixer.GetFloat("BGM", out db);
            return Db2Vol(db);
        }
        public float GetSEVolume() {
            float db;
            _seAMG.audioMixer.GetFloat("SE", out db);
            return Db2Vol(db);
        }
        public static float Vol2Db(float vol) => Mathf.Log10(vol) * 20f;
        public static float Db2Vol(float db) => Mathf.Pow(10, db / 20f);
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(AudioSoundManager))]
    public class SoundManagerEditor : Editor {
        [SerializeField] string _bgmClipName = "None";
        [SerializeField] float _bgmFadeTime = 1;
        [SerializeField] bool _bgmFade = true;
        [SerializeField] bool _bgmStopOther = true;
        [SerializeField] string _seClipName = "None";
        [SerializeField] bool _player = true;
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            var _target = target as AudioSoundManager;
            EditorGUILayout.LabelField("--- Editor ---");
            _target.SetMasterVolume(EditorGUILayout.Slider("Master Volume", _target.GetMasterVolume(), 0f, 1f));
            _target.SetBGMVolume(EditorGUILayout.Slider("BGM Volume", _target.GetBGMVolume(), 0f, 1f));
            _target.SetSEVolume(EditorGUILayout.Slider("SE Volume", _target.GetSEVolume(), 0f, 1f));
            if (_player = EditorGUILayout.Foldout(_player, "Player")) {
                EditorGUI.indentLevel++;
                {
                    this._bgmClipName = EditorGUILayout.TextField("BGM Clip Name", _bgmClipName);
                    GUILayout.BeginHorizontal();
                    _bgmFade = EditorGUILayout.ToggleLeft("Fade", _bgmFade);
                    if (_bgmFade) {
                        this._bgmFadeTime = EditorGUILayout.FloatField(Mathf.Max(0f, _bgmFadeTime));
                    }
                    GUILayout.EndHorizontal();
                    _bgmStopOther = EditorGUILayout.ToggleLeft("Stop Other", _bgmStopOther);
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Play")) {
                        _target.PlayBGM(_bgmClipName, _bgmFade ? _bgmFadeTime : 0f, _bgmStopOther);
                    }
                    if (GUILayout.Button("Stop")) {
                        _target.StopBGM(_bgmClipName, _bgmFade ? _bgmFadeTime : 0f);
                    }
                    if (GUILayout.Button("StopAll")) {
                        _target.StopAllBGM();
                    }
                    GUILayout.EndHorizontal();
                }
                {
                    _seClipName = EditorGUILayout.TextField("SE Clip Name", _seClipName);
                    if (GUILayout.Button("Play")) {
                        _target.PlaySE(_seClipName);
                    }
                }
                EditorGUI.indentLevel--;
            }
        }
    }
#endif

}