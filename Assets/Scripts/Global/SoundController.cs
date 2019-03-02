using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    private AudioSource audioEffectsSource;


    public void Init()
    {
        audioEffectsSource = GetComponents<AudioSource>()[1];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
