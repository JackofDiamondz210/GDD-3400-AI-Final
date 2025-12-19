using UnityEngine;

public struct SoundEvent
{
    //setting up position and loudness values
    public Vector3 position;
    public float loudness;

    public SoundEvent(Vector3 pos, float loud)
    {
        position = pos;
        loudness = loud;
    }
}
