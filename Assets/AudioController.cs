using JSAM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public static AudioController instance;
    public SoundFileObject coin;
    public SoundFileObject hurt;
    public SoundFileObject jump;
    public SoundFileObject landing;
    public SoundFileObject move;
    public SoundFileObject placeTile;
    public SoundFileObject passLevel;
    public SoundFileObject gameOver;
    public SoundFileObject tileMove;
    public SoundFileObject spawn;

    private void Awake()
    {
        instance = this;
    }
    public void PlaySound(SoundFileObject sound)
    {
        AudioManager.PlaySound(sound);  
    }


}
