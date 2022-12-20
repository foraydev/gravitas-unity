using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // physics
    public string moveMode;
    public float speed;
 
    // component references
    protected Rigidbody2D rigidbody2D;
	protected LayerMask whatIsGround;
    protected Transform groundCheck;
    protected Transform ledgeCheck;
    protected Transform wallCheck;

    // other
    protected bool isGrounded;
    protected bool facingRight = true;
    protected GameManager gameManager;
    protected bool concaveRotationLastFrame = false;
    public bool reverse = false;

    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        whatIsGround = LayerMask.GetMask("Ground");
		groundCheck = transform.Find("GroundCheck");
        ledgeCheck = transform.Find("LedgeCheck");
        wallCheck = transform.Find("WallCheck");
        gameManager = GameManager.Instance;
        rigidbody2D.gravityScale = 0.0f;
        if (reverse) {
            Flip();
        }
    }

    void Update()
    {
        bool ledgeTouchingGround = CheckTouchingGround(ledgeCheck);
        if (moveMode == "ground") {
            rigidbody2D.velocity = transform.right * speed;
            if (!ledgeTouchingGround)
            {
                Flip();
            }
        } else if (moveMode == "walls") {
            if (CheckTouchingGround(wallCheck) || (concaveRotationLastFrame && ledgeTouchingGround)) {
                concaveRotationLastFrame = true;
                rigidbody2D.velocity = Vector3.zero;
                transform.Rotate(0f, 0f, speed/2);
                if (!CheckTouchingGround(ledgeCheck)) {
                    concaveRotationLastFrame = false;
                }
            } else if (!CheckTouchingGround(groundCheck) && !ledgeTouchingGround) {
                rigidbody2D.velocity = Vector3.zero;
                transform.RotateAround(groundCheck.position, Vector3.forward, facingRight ? -speed/2 : speed/2);
            } else {
                rigidbody2D.velocity = transform.right * speed;
            }
        }
    }

    void LateUpdate()
    {
        if (concaveRotationLastFrame == false) {
            CorrectRotation();
        }
    }

    protected void Flip()
	{
		facingRight = !facingRight;
		transform.Rotate(0f, 180f, 0f);
	}

    protected bool CheckTouchingGround(Transform check) {
		Collider2D[] colliders = Physics2D.OverlapCircleAll(check.position, 0.005f, whatIsGround);
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i].gameObject != gameObject)
			{
				return true;
			}
		}
		return false;
	}

    protected void CorrectRotation() {
        RaycastHit2D rc1 = Physics2D.Raycast(groundCheck.position, transform.right, 0.5f, whatIsGround);
        RaycastHit2D rc2 = Physics2D.Raycast(groundCheck.position - new Vector3(0f, 0.1f, 0f), transform.right, 0.5f, whatIsGround);
        if (rc1.collider != null && rc2.collider != null) {
            concaveRotationLastFrame = true;
        }
    }
}
