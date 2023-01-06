using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedBombPlatform : MonoBehaviour
{

    public Transform ceilCheck;
    public ParticleSystem leaves;
    public Collider2D collider;
    public LayerMask whatIsGround;
    private float timeActive = 0f;
    private float platformStartTime = 1.083f;
    private float platformEndTime = 6.083f;
    private float totalDuration = 7f;
    private bool disabled = false;

    void Start() {
        collider.enabled = false;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(ceilCheck.position, 0.1f, whatIsGround);
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i].gameObject != gameObject)
			{
				leaves.Play();
                disabled = true;
                GetComponent<Animator>().SetBool("disabled", true);
			}
		}
    }

    void Update() {
        timeActive += Time.deltaTime;
        if (!disabled && timeActive >= platformStartTime) {
            collider.enabled = true;
        }
        if (!disabled && timeActive >= platformEndTime) {
            collider.enabled = false;
        }
        if (timeActive >= totalDuration) {
            Destroy(gameObject);
        }
    }
}
