using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MystSwordController : MonoBehaviour
{
    public Transform target;
    public float damping;
    public float followDistance;
    public MystPlayerController player;

    Vector3 currentVelocity;
    bool facingRight = true;
    float timer = 0.0f;
    Vector3 platformTarget = new Vector3(-1, -1, -1);
    float targetAngle = 0.0f;
    Collider2D collider2D;

    GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        collider2D = GetComponent<Collider2D>();
        collider2D.enabled = false;
        gameManager = GameManager.Instance;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (player.swordMode == "normal") {
            Move();
        } else if (player.swordMode == "platform") {
            Platform();
        }
        //OrientSelf();
        timer += 0.02f;
        if (timer >= 6.28f) {
            timer = 0.0f;
        }
    }

    void Move()
    {
        Vector3 newTarget = target.position;
        if (player.FacingRight()) {
            newTarget = new Vector3(newTarget.x - followDistance, newTarget.y + Mathf.Sin(timer)*0.3f, 0);
        } else {
            newTarget = new Vector3(newTarget.x + followDistance, newTarget.y + Mathf.Sin(timer)*0.3f, 0);
        }
        transform.position = Vector3.SmoothDamp(transform.position, newTarget, ref currentVelocity, damping);
        if (transform.position.x > target.position.x) {
            EditRotation(-20);
        } else if (transform.position.x < target.position.x) {
            EditRotation(20);
        }
    }

    void Platform()
    {
        if (platformTarget.x == -1)
        {
            if (player.facingRight) {
                if (player.wallSliding) {
                    platformTarget = new Vector3(target.position.x - 5, target.position.y, 0);
                    EditRotation(-90);
                } else {
                    platformTarget = new Vector3(target.position.x + 5, target.position.y, 0);
                    EditRotation(90);
                }
            } else {
                if (player.wallSliding) {
                    platformTarget = new Vector3(target.position.x + 5, target.position.y, 0);
                    EditRotation(90);
                } else {
                    platformTarget = new Vector3(target.position.x - 5, target.position.y, 0);
                    EditRotation(-90);
                }
            }
        }
        transform.position = Vector3.SmoothDamp(transform.position, platformTarget, ref currentVelocity, 0.05f);
        if (Vector3.Distance(player.gameObject.transform.position, transform.position) > 1)
        {
            collider2D.enabled = true;
        }
        if (Vector3.Distance(platformTarget, transform.position) < 0.3)
        {
            GetComponent<Rigidbody2D>().isKinematic = true;
        }
        GameManager.Instance.playerMP -= 1f;
        if (GameManager.Instance.playerMP <= 0.0f) {
            EndPlatformMode();
        }
    }

    void OrientSelf()
    {
        Quaternion targetRot = Quaternion.Euler(0, 0, targetAngle);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 0.05f);
    }

    void Flip()
    {
        EditRotation(-transform.rotation.z);
        facingRight = !facingRight;
    }

    void EditRotation(float newRot)
    {
        Quaternion currAngle = new Quaternion(0, 0, 0, 0);
        Vector3 newRotation = new Vector3(0, 0, newRot);
        currAngle.eulerAngles = newRotation;
        transform.rotation = currAngle;
    }

    public void EndPlatformMode()
    {
        if (transform.position.x > target.position.x) {
            EditRotation(20);
        } else if (transform.position.x < target.position.x) {
            EditRotation(-20);
        }
        platformTarget = new Vector3(-1, -1, -1);
        collider2D.enabled = false;
        player.swordMode = "normal";
        GetComponent<Rigidbody2D>().isKinematic = false;
    }

    void OnCollisionEnter2D(Collision2D col) {
        if (col.gameObject.tag == "Destructible") {
            GameManager.Instance.DestroyTerrain(col.gameObject);
        }
    }

    public void ResetPosition() {
        Vector3 playerPos = GameManager.Instance.currentPlayerTransform.position;
        transform.position = new Vector3(playerPos.x + (facingRight ? -followDistance : followDistance), playerPos.y, playerPos.z);
    }
}
