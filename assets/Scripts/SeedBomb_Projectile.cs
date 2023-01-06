using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedBomb_Projectile : MonoBehaviour
{
    private float speed = 8f;
    public Rigidbody2D rb;
    public GameObject platformPrefab;
    private string activationMode = "platform";

    void Start()
    {
        rb.velocity = transform.right + new Vector3(0f, 8f, 0f);
    }

    void Update() {
        transform.Rotate(new Vector3(0f, 0f, 1f));
    }

    void OnCollisionEnter2D(Collision2D col) {
        if (LayerMask.LayerToName(col.gameObject.layer) == "Ground") {
            Instantiate(platformPrefab, new Vector3(transform.position.x, transform.position.y+1.48f, transform.position.z), new Quaternion(0f, 0f, 0f, 0f));
            Destroy(gameObject);
        }
    }
}
