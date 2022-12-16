using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneMusicManager : MonoBehaviour
{
    public AudioClip defaultMusic;
    public MusicZone[] alternateZones;

    public AudioClip GetCurrentMusic() {
        foreach (MusicZone z in alternateZones) {
            if (z.IsActive()) {
                return z.music;
            }
        }
        return defaultMusic;
    }
}
