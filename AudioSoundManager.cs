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
            AudioSource audioSource = _bgmAudioSources.FirstOrDefault(s => s.clip.name == clipName);
            if (audioSource == null || audioSource.isPlaying) {
                return;
            }
            audioSource.Stop();
            audioSource.clip = null;
        }
        public void PlayBGMWithFadeIn(string clipName, float fadeTime = 2f) {
            AudioClip audioClip = _bgmClips.FirstOrDefault(c => c.name == clipName);
            if (audioClip == null) {
                Debug.LogWarning($"{clipName}が見つかりません.");
                return;
            }
            var source = _bgmAudioSources.FirstOrDefault(s => s.clip == null);
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
        [SerializeField] string _bgmClipName;
        [SerializeField] float _bgmFadeTime = 1;
        [SerializeField] bool _bgmFade;
        [SerializeField] string _seClipName;
        [SerializeField] bool _player = false;
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
                        this._bgmFadeTime = EditorGUILayout.FloatField(Mathf.Min(0f, _bgmFadeTime));
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Play")) {
                        if (!_bgmFade) _target.PlayBGM(_bgmClipName);
                        else _target.PlayBGMWithFadeIn(_bgmClipName, _bgmFadeTime);
                    }
                    if (GUILayout.Button("Stop")) {
                        _target.StopBGM(_bgmClipName);
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