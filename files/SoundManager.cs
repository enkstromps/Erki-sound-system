//Sound Manager script by Erik Engström 2021. Updated August 2022

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace wriks.sound
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }
        public AudioMixer audioMixer = null;

        [System.Serializable]
        public class ErkiSound
        {
            [Header("Name")] public string name = "Sound";
            [Header("Sounds")] public List<AudioClip> sounds = new List<AudioClip>();
            [Header("Data")] public float volume = 1;
            public float spatialBlend = 0;
            public bool randomPitch = false;
            public float minPitch = 0.8f;
            public float maxPitch = 1.2f;
            public bool repeating = false;
            public bool destroyOnEnd = true;
            public float timeUntilDestroyed = Mathf.Infinity;
            public float startTime = 0;
            public AudioMixerGroup audioMixerGroup = null;
        }

        public List<ErkiSound> erkiSounds = new List<ErkiSound>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogError(
                    $"Deleted object: '{gameObject.name}', as there can only be one SoundManager in a scene");
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            Mixer();
        }

        private void Mixer()
        {
            if (!audioMixer)
            {
                return;
            }

            //Put your audioMixer values here
            audioMixer.SetFloat("MixerMusicVolume", LogConvert(PlayerPrefs.GetFloat("MusicVolume")));
            audioMixer.SetFloat("MixerSoundVolume", LogConvert(PlayerPrefs.GetFloat("SoundVolume")));
        }

        private float LogConvert(float volume)
        {
            return Mathf.Clamp(20 * Mathf.Log10(volume), -55, 0);
        }

        ///<summary>
        ///Plays a sound.
        ///</summary>
        public GameObject PlaySound(string name)
        {
            return SetAudioHolderValues(null, name, Vector3.zero);
        }

        ///<summary>
        ///Plays a sound, sound object becomes parented to parent.
        ///</summary>
        public GameObject PlaySound(string name, Transform parent)
        {
            return SetAudioHolderValues(parent, name, Vector3.zero);
        }

        ///<summary>
        ///Plays a sound, sound object becomes parented to parent with an offset position. Parent can be null.
        ///</summary>
        public GameObject PlaySound(string name, Vector3 position, Transform parent)
        {
            return SetAudioHolderValues(parent, name, position);
        }

        private GameObject SetAudioHolderValues(Transform parent, string name, Vector3 position)
        {
            ErkiSound erkiSound = GetErkiSoundFromName(name);

            if (erkiSound == null)
            {
                Debug.LogError($"Couldn't find sound '{name}'");
                return null;
            }

            var audioHolderInstance = CreateAudioHolder(parent, name);
            AudioSource audioHolderInstanceAudioSource = audioHolderInstance.GetComponent<AudioSource>();

            audioHolderInstance.transform.localPosition = position;
            audioHolderInstanceAudioSource.spatialBlend = erkiSound.spatialBlend;
            audioHolderInstanceAudioSource.clip = erkiSound.sounds[Random.Range(0, erkiSound.sounds.Count)];
            audioHolderInstanceAudioSource.volume = erkiSound.volume;
            audioHolderInstanceAudioSource.loop = erkiSound.repeating;
            audioHolderInstanceAudioSource.time =
                Mathf.Clamp(erkiSound.startTime, 0, audioHolderInstanceAudioSource.clip.length);
            audioHolderInstanceAudioSource.outputAudioMixerGroup = erkiSound.audioMixerGroup;

            if (erkiSound.randomPitch == true)
            {
                audioHolderInstanceAudioSource.pitch = Random.Range(erkiSound.minPitch, erkiSound.maxPitch);
            }
            else
            {
                audioHolderInstanceAudioSource.pitch = 1;
            }

            audioHolderInstanceAudioSource.Play();

            if (erkiSound.destroyOnEnd)
            {
                Destroy(audioHolderInstance, audioHolderInstanceAudioSource.clip.length);
            }
            else
            {
                Destroy(audioHolderInstance, erkiSound.timeUntilDestroyed);
            }

            return audioHolderInstance;
        }

        public ErkiSound GetErkiSoundFromName(string name)
        {
            ErkiSound erkiSound = null;
            for (int i = 0; i < erkiSounds.Count; i++)
            {
                if (name == erkiSounds[i].name)
                {
                    erkiSound = erkiSounds[i];
                    break;
                }
            }

            return erkiSound;
        }

        public AudioClip[] GetAudioClipsFromErkiSound(string name)
        {
            return GetErkiSoundFromName(name).sounds.ToArray();
        }

        private GameObject CreateAudioHolder(Transform parent, string name)
        {
            GameObject audioHolder = new GameObject();
            audioHolder.transform.parent = parent;
            audioHolder.name = $"AudioHolder ({name})";
            audioHolder.AddComponent<AudioSource>();

            return audioHolder;
        }
    }
}
