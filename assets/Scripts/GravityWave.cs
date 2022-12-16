using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityWave : MonoBehaviour
{
    private float speed = 8f;
    public Rigidbody2D rb;

    void Start()
    {
        rb.velocity = transform.right * speed;
    }

    void OnCollisionEnter2D(Collision2D col) {
        if (col.gameObject.tag == "Destructible") {
            GameManager.Instance.DestroyTerrain(col.gameObject);
        }
        if (LayerMask.LayerToName(col.gameObject.layer) == "Ground") {
            Destroy(gameObject);
        }
    }
}
