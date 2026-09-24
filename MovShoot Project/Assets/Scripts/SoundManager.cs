using UnityEngine;
using System;
using System.Collections;

public enum SoundType
{
    Fist_Melee,
    Fist_Heavy,
    Projectiles,
    Fire_Projectile,
    Land,
    Jump,
    Hurt,
    Footsteps,
    Ability_Pickup,
    Grapple_Hit,
    Rock_Break,
}
//makes the script useable in edit mode
[RequireComponent(typeof(AudioSource)), ExecuteInEditMode]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private SoundList[] soundList;
    private static SoundManager instance;
    private AudioSource audioSource;
    public float pitchVar;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // PlaySound(SoundType, Volume)
    public static void PlaySound(SoundType sound, float volume = 1)
    {
        
        float randomPitch = UnityEngine.Random.Range(1f - instance.pitchVar, 1f + instance.pitchVar);
        instance.audioSource.pitch = randomPitch;
        AudioClip[] clips = instance.soundList[(int)sound].Sounds;
        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];
        instance.audioSource.PlayOneShot(randomClip, volume);
    }

    // makes sure it plays in the editor instead of game
#if UNITY_EDITOR
    private void OnEnable()
    {
        // automatically gets the names of the enums and changes the "element 1" names in the editor
        string[] names = Enum.GetNames(typeof(SoundType));
        Array.Resize(ref soundList, names.Length);
        for (int i = 0; i < soundList.Length; i++)
        {
            soundList[i].name = names[i];
        }
    }
#endif
}

// this is the drop down menu thats inside of the titles
[Serializable]
public struct SoundList
{
    // when you make a string at the top of a struct it changes the element to the "name"
    public AudioClip[] Sounds { get => sounds; }
    [HideInInspector] public string name;
    [SerializeField] private AudioClip[] sounds;
}
