using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class AudioClipRefsSO : ScriptableObject
{
    public AudioClip[] repair;
    public AudioClip[] deliveryFail;
    public AudioClip[] deliverySuccess;
    public AudioClip[] footstep;
    public AudioClip[] objectDrop;
    public AudioClip[] objectPickup;
    public AudioClip chargerSizzle;
    public AudioClip[] trash;
    public AudioClip[] warning;
    public AudioClip[] dash;
    public AudioClip countdown;
    public AudioClip stoveFrying;
    public AudioClip stoveBurned;
    public AudioClip[] timerHalfway;
    public AudioClip[] timerFinal;
    public AudioClip[] orderSpawn; 
}
