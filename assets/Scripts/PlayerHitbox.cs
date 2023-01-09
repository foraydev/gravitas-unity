using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    public float xForce;
    public float yForce;

    protected void OnTriggerEnter2D(Collider2D col) {
        Debug.Log("entered a trigger...");
		if (col.gameObject.name == "SeedBomb") {
            col.gameObject.GetComponent<SeedBombProjectile>().rb.velocity = new Vector3(xForce, yForce, 0f);
        }
	}
}
