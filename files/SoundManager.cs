using System.Collections.Generic;
using UnityEngine;
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    public GameObject audioHolder = null;

    [HideInInspector]
    public float musicVolume = 0, soundEffectVolume = 0;
    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError($"Deleted object: '{gameObject.name}', as there can only be one SoundManager in a scene");
            Destroy(gameObject);
        }
    }

    [System.Serializable]
    public class ErkiSound
    {
        [Header("Name")]
        public string name = "Sound";
        [Header("Sounds")]
        public List<AudioClip> sounds = new List<AudioClip>();
        [Header("Data")]
        public float volume = 1;
        public float spatialBlend = 0;
        public bool randomPitch = false;
        public float minPitch = 0.8f;
        public float maxPitch = 1.2f;
        public bool repeating = false;
        public float timeUntilDestroyed = 1;
        public enum SoundType { SoundEffect, Music, Other}
        [Header("Type")]
        public SoundType soundType = 0;
    }

    public List<ErkiSound> erkiSounds = new List<ErkiSound>();

    ///<summary>
    ///Plays a sound.
    ///</summary>
    public GameObject PlaySound(string name)
    {
        return SetAudioHolderValues(audioHolder, null, name, Vector3.zero);
    }

    ///<summary>
    ///Plays a sound, sound object becomes parented to parent.
    ///</summary>
    public GameObject PlaySound(string name, Transform parent)
    {
        return SetAudioHolderValues(audioHolder, parent, name, Vector3.zero);
    }

    ///<summary>
    ///Plays a sound, sound object becomes parented to parent with an offset position. Parent can be null.
    ///</summary>
    public GameObject PlaySound(string name, Transform parent, Vector3 position)
    {
        return SetAudioHolderValues(audioHolder, parent, name, position);
    }

    private GameObject SetAudioHolderValues(GameObject audioHolder, Transform parent, string name, Vector3 position)
    {
        ErkiSound erkiSound = GetErkiSoundFromName(name);

        if (erkiSound == null)
        {
            Debug.LogError($"Couldn't find sound '{name}'");
            return null;
        }

        var audioHolderInstance = Instantiate(audioHolder, parent);
        AudioSource audioHolderInstanceAudioSource = audioHolderInstance.GetComponent<AudioSource>();

        audioHolderInstance.transform.localPosition = position;
        audioHolderInstanceAudioSource.spatialBlend = erkiSound.spatialBlend;
        audioHolderInstanceAudioSource.clip = erkiSound.sounds[Random.Range(0, erkiSound.sounds.Count)];
        //audioHolderInstanceAudioSource.volume = erkiSound.volume;
        audioHolderInstanceAudioSource.loop = erkiSound.repeating;

        if (erkiSound.randomPitch == true)
        {
            audioHolderInstanceAudioSource.pitch = Random.Range(erkiSound.minPitch, erkiSound.maxPitch);
        }
        else
        {
            audioHolderInstanceAudioSource.pitch = 1;
        }

        switch (erkiSound.soundType)
        {
            case ErkiSound.SoundType.SoundEffect:
                {
                    audioHolderInstanceAudioSource.volume = erkiSound.volume * soundEffectVolume;
                    break;
                }
            case ErkiSound.SoundType.Music:
                {
                    audioHolderInstanceAudioSource.volume = erkiSound.volume * musicVolume;
                    break;
                }
        }

        Destroy(audioHolderInstance, erkiSound.timeUntilDestroyed);

        audioHolderInstanceAudioSource.Play();

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
}
