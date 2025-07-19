using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.Audio;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HK
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class AudioManager : MonoBehaviour
    {
        [SerializeField]
        private AudioSource bgmSource;

        [SerializeField]
        private AudioSource sfxSource;

        [SerializeField]
        private AudioMixer audioMixer;

        [SerializeField]
        private Element.DictionaryList bgm;

        [SerializeField]
        private Element.DictionaryList sfx;

        public void PlayBgm(string key)
        {
            bgmSource.volume = 1.0f;
            bgmSource.clip = bgm.Get(key).Clip;
            bgmSource.Play();
        }

        public void StopBgm()
        {
            bgmSource.Stop();
        }
        
        public void PlaySfx(string key)
        {
            sfxSource.PlayOneShot(sfx.Get(key).Clip);
        }
        
        public UniTask FadeOutBgmAsync(float duration, CancellationToken cancellationToken)
        {
            return LMotion.Create(bgmSource.volume, 0, duration)
                .BindToVolume(bgmSource)
                .ToUniTask(cancellationToken);
        }

        public void SetVolumeMaster(float volume)
        {
            SetVolume("MasterVolume", volume);
        }

        public void SetVolumeBgm(float volume)
        {
            SetVolume("BgmVolume", volume);
        }

        public void SetVolumeSfx(float volume)
        {
            SetVolume("SfxVolume", volume);
        }

        public float GetVolumeMaster()
        {
            return GetVolume("MasterVolume");
        }

        public float GetVolumeBgm()
        {
            return GetVolume("BgmVolume");
        }

        public float GetVolumeSfx()
        {
            return GetVolume("SfxVolume");
        }

        private void SetVolume(string name, float volume)
        {
            volume = Mathf.Clamp01(volume);
            if (volume > 0)
            {
                volume = Mathf.Log10(volume) * 20;
            }
            else
            {
                volume = -80;
            }
            audioMixer.SetFloat(name, volume);
        }

        private float GetVolume(string name)
        {
            audioMixer.GetFloat(name, out var volume);
            return Mathf.Pow(10, volume / 20);
        }

        [Serializable]
        public class Element
        {
            [SerializeField]
            private string key;

            [SerializeField]
            private AudioClip clip;
            public AudioClip Clip => clip;

            public Element(string key, AudioClip clip)
            {
                this.key = key;
                this.clip = clip;
            }

            [Serializable]
            public class DictionaryList : DictionaryList<string, Element>
            {
                public DictionaryList() : base(x => x.key)
                {
                }
            }
        }
        
#if UNITY_EDITOR
        [MenuItem("Assets/Create/HK/AudioManager")]
        private static void CreateAudioManager()
        {
            var root = new GameObject("AudioManager");
            var audioManager = root.AddComponent<AudioManager>();
            var bgmSource = new GameObject("BgmSource");
            bgmSource.transform.SetParent(root.transform);
            audioManager.bgmSource = bgmSource.AddComponent<AudioSource>();
            audioManager.bgmSource.playOnAwake = false;
            audioManager.bgmSource.loop = true;
            var sfxSource = new GameObject("SfxSource");
            sfxSource.transform.SetParent(root.transform);
            audioManager.sfxSource = sfxSource.AddComponent<AudioSource>();
            audioManager.sfxSource.playOnAwake = false;
            audioManager.sfxSource.loop = false;
            EditorUtility.DisplayDialog("確認", "AudioMixerは自動で生成出来ないため、手動で生成してください。各AudioSourceのOutputの設定も忘れずに行ってください。", "OK");
            Selection.activeGameObject = root;
        }
#endif
    }
}