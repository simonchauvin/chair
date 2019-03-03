using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    private static SoundController _instance;
    public static SoundController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SoundController>();
            }
            return _instance;
        }
    }

    private AudioSource audioEffectsSource;


    public void Init()
    {
        audioEffectsSource = GetComponents<AudioSource>()[1];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayRipSound()
    {
        audioEffectsSource.Play();
    }
}
