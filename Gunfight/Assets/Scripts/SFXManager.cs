using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class SFXManager : MonoBehaviour
{
    public Sound[] sounds;
    public float destroyDelay = 2.0f;

    void Start()
    {
        SFXManager[] sfxManagersInScene = FindObjectsOfType<SFXManager>();

        foreach (SFXManager sfxManager in sfxManagersInScene)
        {
            if (sfxManager != this)
            {
                // Another SFXManager object was found in the scene
                Destroy(sfxManager.gameObject, destroyDelay);
            }
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.loop = false;
            DontDestroyOnLoad(s.source);
        }
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.Play();
    }
}
