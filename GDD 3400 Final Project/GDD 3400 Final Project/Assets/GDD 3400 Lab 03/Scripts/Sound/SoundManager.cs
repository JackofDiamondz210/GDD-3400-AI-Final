using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{

    /// <summary>
    /// Setting up an EmptyGame Object to handle the sound events
    /// </summary>
    /// 
    public static SoundManager Instance;

    public event Action<SoundEvent> OnSoundEmitted;


    private void Awake()
    {
        Instance = this;
    }


    public void EmitSound(Vector3 position, float loudness)
    {
        Debug.Log($"SOUND EMITTED at {position} loudness {loudness}");
        OnSoundEmitted?.Invoke(new SoundEvent(position, loudness));
    }

}
