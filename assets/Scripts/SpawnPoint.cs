using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint
{
    public string scene;
    public Vector3 position;

    public SpawnPoint(string s, Vector3 p) {
        this.scene = s;
        this.position = p;
    }
}
