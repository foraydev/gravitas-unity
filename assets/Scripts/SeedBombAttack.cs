using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedBombAttack : MonoBehaviour
{
    private float timeActive = 0f;
    private float totalDuration = 2f;

    void Update() {
        //transform.Rotate(new Vector3(0f, 0f, 1f));
        timeActive += Time.deltaTime;
        if (timeActive >= totalDuration) {
            Destroy(gameObject);
        }
    }
}
