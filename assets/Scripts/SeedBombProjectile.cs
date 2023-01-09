using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedBombProjectile : MonoBehaviour
{
    private float speed = 8f;
    public Rigidbody2D rb;
    public GameObject platformPrefab;
    public GameObject attackPrefab;
    private string activationMode = "platform";

    void Start()
    {
        rb.velocity = transform.right + new Vector3(0f, 8f, 0f);
    }

    void Update() {
        transform.Rotate(new Vector3(0f, 0f, 1f));
    }

    protected void OnTriggerEnter2D(Collider2D col) {
		if (col.gameObject.name == "AxisAttackHitboxN") {
            rb.velocity = new Vector3(col.transform.parent.GetComponent<Player>().facingRight ? 6f : -6f, 4f, 0f);
            activationMode = "attack";
        }
        if (col.gameObject.name == "AxisAttackHitboxU") {
            rb.velocity = new Vector3(rb.velocity.x, 6f, 0f);
            activationMode = "attack";
        }
        if (col.gameObject.name == "AxisAttackHitboxD") {
            rb.velocity = new Vector3(0f, -6f, 0f);
            activationMode = "attack";
        }
	}

    void OnCollisionEnter2D(Collision2D col) {
        if (activationMode == "platform") {
            if (col.gameObject.tag == "PlayerAttack") {
                activationMode = "attack";
                if (col.gameObject.name.Contains("GravityWave")) {
                    rb.velocity = new Vector3(col.rigidbody.velocity.x*1.5f, 5f, 0f);
                    Destroy(col.gameObject);
                }
            } else if (LayerMask.LayerToName(col.gameObject.layer) != "Player") {
                if (LayerMask.LayerToName(col.gameObject.layer) == "Ground") {
                    Instantiate(platformPrefab, new Vector3(transform.position.x, transform.position.y+1.48f, transform.position.z), new Quaternion(0f, 0f, 0f, 0f));
                }
                Destroy(gameObject);
            }
        } else {
            if (col.gameObject.tag == "Enemy" || LayerMask.LayerToName(col.gameObject.layer) == "Ground") {
                Instantiate(attackPrefab, transform.position, transform.rotation);
                Destroy(gameObject);
            }
        }
    }
}
