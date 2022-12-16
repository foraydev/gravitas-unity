using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
	// global player attributes
	protected float movementSmoothing = .05f;
	public HealthBarManager hpBar = null;
	public HealthBarManager mpBar = null;
	public GameManager gameManager = null;
	public bool facingRight { get; set; } = true;

	// physics
    protected float horizontalMove = 0f;
    protected bool jumping = false;
    protected bool canJump = true;
	public string moveMode { get; set; } = "normal";
	protected float jumpForce = 900f;
    protected float runSpeed = 40f;

	// normal grounded info
    protected const float groundedRadius = .2f;
	protected bool isGrounded;
	protected LayerMask whatIsGround;
	protected Transform groundCheck;

	// component references
	protected Rigidbody2D rigidbody2D;
	protected Vector3 velocity = Vector3.zero;
    protected Vector3 respawnPoint;

	// fallstun
	protected float fallStartY = 0f;
	protected float fallStunY = 10f;
	protected float fallStunTimer = 0f;

	// grace frames
	protected int framesSinceLeavingGround = 0;
	protected int graceFrames = 6;

    protected void Start()
	{
		rigidbody2D = GetComponent<Rigidbody2D>();
        hpBar = GameObject.Find("Healthbar").GetComponent<HealthBarManager>();
        mpBar = GameObject.Find("Magicbar").GetComponent<HealthBarManager>();
		whatIsGround = LayerMask.GetMask("Ground");
		groundCheck = transform.Find("GroundCheck");
        gameManager = GameManager.Instance;
        respawnPoint = new Vector3(0f, 0f, 0f);
		hpBar.setMaxVal(gameManager.maxHP);
		mpBar.setMaxVal(gameManager.maxMP);
        gameManager.playerMP = gameManager.maxMP;
	}

    protected void Update()
    {
		horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
		if (Input.GetButtonDown("Float"))
		{
			moveMode = "float";
		}
		if (moveMode == "normal")
		{
			if (Input.GetButtonDown("Jump"))
			{
				jumping = true;
			} else if (Input.GetButtonUp("Jump")) {
				canJump = true;
			}
		}
    }

    protected void FixedUpdate()
    {
		if (fallStunTimer > 0) {
			fallStunTimer -= Time.deltaTime;
			if (fallStunTimer <= 0) {
				fallStunTimer = 0;
				moveMode = "normal";
			}
		}
		bool wasGrounded = isGrounded;
		isGrounded = this.CheckGrounded();
		if (isGrounded && moveMode == "up-transition") {
			moveMode = "normal";
			Input.ResetInputAxes();
			horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
		}
		if ((wasGrounded && !isGrounded) || (jumping && isGrounded)) {
			OnLeaveGround();
		}
		if (!wasGrounded && isGrounded) {
			OnLand();
		}
		if (moveMode == "stun" || gameManager.gameState == "dialogue" || gameManager.gameState == "saving") {
			rigidbody2D.velocity = Vector2.zero;
		}
		if (moveMode == "normal") {
			Move(horizontalMove * Time.fixedDeltaTime, jumping);
		} else if (moveMode == "float") {
			Float();
		}
		jumping = false;
		framesSinceLeavingGround++;
		rigidbody2D.velocity = Vector2.Max(rigidbody2D.velocity, new Vector2(rigidbody2D.velocity.x, Mathf.Clamp(rigidbody2D.velocity.y, -15, float.MaxValue)));
		hpBar.setVal(gameManager.playerHP);
		mpBar.setVal(gameManager.playerMP);
    }

    protected virtual void Move(float move, bool jump)
	{
		Vector3 targetVelocity = new Vector2(move * 10f, rigidbody2D.velocity.y);
		rigidbody2D.velocity = Vector3.SmoothDamp(rigidbody2D.velocity, targetVelocity, ref velocity, movementSmoothing);
		if (move > 0 && !facingRight)
		{
			Flip();
		}
		else if (move < 0 && facingRight)
		{
			Flip();
		}
		if (jumping && canJump && (isGrounded || framesSinceLeavingGround < graceFrames))
		{
			rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, 0);
			isGrounded = false;
			rigidbody2D.AddForce(new Vector2(0f, jumpForce));
			//framesSinceLeavingGround = graceFrames;
		}
	}

	protected virtual void Float() {}

	public void StopFloat() {}

	protected void Flip()
	{
		facingRight = !facingRight;
		transform.Rotate(0f, 180f, 0f);
	}

	public bool FacingRight()
	{
		return facingRight;
	}

    protected void TakeDamage(float dmg) {
		gameManager.playerHP -= dmg;
    }

    protected void TakeDamageAndRespawn(float dmg) {
        TakeDamage(dmg);
        transform.position = respawnPoint;
		rigidbody2D.velocity = Vector3.zero;
    }

    protected void OnTriggerEnter2D(Collider2D collision) {
		if (collision.tag == "Transition") {
			gameManager.ChangeScene(collision.gameObject.GetComponent<Transition>(), new PlayerState(this));
		} else if (collision.tag == "Respawn") {
            respawnPoint = collision.gameObject.transform.position;
		} else if (collision.tag == "Hazard") {
            TakeDamageAndRespawn(20);
        } else if (collision.tag == "Collectible") {
            gameManager.GetCollectible(collision.gameObject.name);
			collision.gameObject.SetActive(false);
        }
	}

	public void LoadFromPlayerState(PlayerState ps) {
		transform.position = ps.position;
		if (!ps.facingRight) { Flip(); }
		this.moveMode = ps.moveMode;
		rigidbody2D = GetComponent<Rigidbody2D>();
		/*if (this.moveMode == "up-transition") {
			rigidbody2D.AddForce(new Vector2(facingRight ? 110f : -110f, 900f));
		}*/
	}

	protected bool CheckGrounded() {
		Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundedRadius, whatIsGround);
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i].gameObject != gameObject)
			{
				return true;
			}
		}
		return false;
	}

	protected void OnLeaveGround() {
		fallStartY = transform.position.y;
		framesSinceLeavingGround = 0;
	}

	protected void OnLand() {
		if (fallStartY - transform.position.y > fallStunY) {
			moveMode = "stun";
			fallStunTimer = .75f;
			GameObject.Find("Main Camera").GetComponent<Camera2DFollow>().StartShake(0.5f, 0.08f, true);
		}
	}

	public float DistanceToGround() {
		return transform.position.y - groundCheck.transform.position.y;
	}
}
