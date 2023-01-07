using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHole : MonoBehaviour
{
    private Vector3 targetPos;
    private Vector3 currentVelocity;
    private Vector3 currentScale;
    private float heightToTravel = 1.75f;
    private bool disabled = false;

    void Start() {
        targetPos = transform.position + new Vector3(0f, heightToTravel, 0f);
        transform.localScale = new Vector3(0f, 0f, 0f);
    }

    void FixedUpdate() {
        transform.Rotate(new Vector3(0f, 0f, -5f));
        GameManager.Instance.playerMP -= 1f;
        if (GameManager.Instance.playerMP <= 0f) {
            disabled = true;
        }
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, 0.5f);
        if (!disabled) {
            transform.localScale = Vector3.SmoothDamp(transform.localScale, new Vector3(1f, 1f, 1f), ref currentScale, 0.5f);
        } else {
            transform.localScale = Vector3.SmoothDamp(transform.localScale, new Vector3(0f, 0f, 0f), ref currentScale, 0.5f);
        }
        if (disabled && Vector3.Distance(transform.localScale, new Vector3(0f, 0f, 0f)) < 0.05f) {
            Destroy(gameObject);
        }
    }

    void OnDestroy() {
        if (GameManager.Instance.currentPlayer == "Axis") {
            GameManager.Instance.currentPlayerTransform.gameObject.GetComponent<AxisPlayerController>().DeactivateBlackHole();
        }
    }
}
