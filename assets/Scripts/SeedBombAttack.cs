using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedBombAttack : MonoBehaviour
{
    private float timeActive = 0f;
    private float totalDuration = 2f;


    void Update() {
        timeActive += Time.deltaTime;
        if (timeActive >= totalDuration) {
            Destroy(gameObject);
        }
    }

    void OnDestroy() {
        if (GameManager.Instance.currentPlayer == "Axis") {
            GameManager.Instance.currentPlayerTransform.gameObject.GetComponent<AxisPlayerController>().BeginSeedBombCooldown();
        }
    }
}
