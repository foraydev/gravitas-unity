using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
	// global player attributes
	protected float movementSmoothing = .05f;
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
    protected const float groundedRadius = .1f;
	protected bool isGrounded;
	protected LayerMask whatIsGround;
	protected Transform groundCheck;

	// component references
	protected Rigidbody2D rigidbody2D;
	protected Vector3 velocity = Vector3.zero;
    protected Vector3 respawnPoint;
	protected Animator animator;
	protected ParticleSystem parryPS;

	// fallstun
	protected float fallStartY = 0f;
	protected float fallStunY = 10f;
	protected float fallStunTimer = 0f;

	// grace frames
	protected int framesSinceLeavingGround = 0;
	protected int graceFrames = 6;

	// invincibility
	protected int framesSinceDamage = 0;
	protected int knockbackFrames = 5;
	protected int iFrames = 20;

	//looking up/down
	protected int lookDirection = 0;

	//parry
	protected float parryActiveTime = 0f;
	protected float parryDuration = 0.5f;
	protected float parrySuccessDuration = 0.3f;
	protected float parryActiveDuration = 0.33f;
	protected float parryStartTime = 0.083f;
	protected int parryState = 0; // 0 = normal parry, 1 = normal parry successful, 2 = chaos parry, 3 = chaos parry successful
	protected bool hasChaosShield = true;
	protected float timeSinceChaosUsed = 0f;
	protected float chaosCooldown = 1.5f;

    protected void Start()
	{
		rigidbody2D = GetComponent<Rigidbody2D>();
		whatIsGround = LayerMask.GetMask("Ground");
		groundCheck = transform.Find("GroundCheck");
        gameManager = GameManager.Instance;
        respawnPoint = new Vector3(0f, 0f, 0f);
        gameManager.playerMP = gameManager.maxMP;
		animator = GetComponent<Animator>();
		parryPS = transform.Find("ParryPS").gameObject.GetComponent<ParticleSystem>();
	}

    protected void Update()
    {
		horizontalMove = 0f;
		if (moveMode != "damaged" && moveMode != "seedbomb" && moveMode != "cast" && moveMode != "cast2") {
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
				if (Input.GetButtonDown("AltMove")) {
					moveMode = "altmove";
				}
			}
		}
		UpdateAnimator();
    }

    protected void FixedUpdate()
    {
		if (moveMode == "damaged") {
			if (framesSinceDamage == knockbackFrames-1) {
				rigidbody2D.velocity = Vector2.zero;
			}
			if (framesSinceDamage > knockbackFrames) {
				moveMode = "normal";
			}
		}
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
		framesSinceDamage++;
		rigidbody2D.velocity = Vector2.Max(rigidbody2D.velocity, new Vector2(rigidbody2D.velocity.x, Mathf.Clamp(rigidbody2D.velocity.y, -15, float.MaxValue)));
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
		if (framesSinceDamage > iFrames) {
			gameManager.playerHP -= dmg;
			framesSinceDamage = 0;
			moveMode = "damaged";
			if (gameManager.playerHP <= 0) {
				rigidbody2D.velocity = Vector2.zero;
				rigidbody2D.isKinematic = true;
				rigidbody2D.simulated = false;
			}
		}
    }

    protected void TakeDamageAndRespawn(float dmg) {
        TakeDamage(dmg);
		if (gameManager.playerHP > 0) {
			transform.position = new Vector3(respawnPoint.x, respawnPoint.y + DistanceToGround(), respawnPoint.z);
		}
		rigidbody2D.velocity = Vector3.zero;
		if (gameManager.playerHP <= 0) {
			rigidbody2D.isKinematic = true;
			rigidbody2D.simulated = false;
		}
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

	protected void OnCollisionEnter2D(Collision2D col) {
		if (gameManager.gameState == "play") {
			if (col.gameObject.tag == "Enemy") {
				if (moveMode == "parry" && framesSinceDamage >= iFrames) {
					if (parryState == 0) {
						parryPS.Play();
					}
					parryState = parryState == 2 ? 3 : 1;
					framesSinceDamage = 0;
					rigidbody2D.AddForce(new Vector2(col.transform.position.x < transform.position.x ? 1500f : -1200f, 0f));
					GameObject.Find("Main Camera").GetComponent<Camera2DFollow>().StartShake(0.2f, 0.03f, true);
				} else {
					TakeDamage(20);
					rigidbody2D.AddForce(new Vector2(col.transform.position.x < transform.position.x ? 1500f : -1500f, 600f));
				}
			}
		}
    }

	public void LoadFromPlayerState(PlayerState ps) {
		transform.position = ps.position;
		if (!ps.facingRight) { Flip(); }
		this.moveMode = ps.moveMode;
		rigidbody2D = GetComponent<Rigidbody2D>();
		rigidbody2D.isKinematic = false;
		rigidbody2D.simulated = true;
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

	protected void UpdateAnimator() {
		if (moveMode == "normal") {
			animator.SetInteger("MoveMode", 0);
		} else if (moveMode == "stun") {
			animator.SetInteger("MoveMode", 1);
		} else if (moveMode == "float") {
			animator.SetInteger("MoveMode", 2);
		} else if (moveMode == "damaged") {
			animator.SetInteger("MoveMode", 3);
		} else if (moveMode == "cast") {
			animator.SetInteger("MoveMode", 4);
		} else if (moveMode == "cast2") {
			animator.SetInteger("MoveMode", 5);
		} else if (moveMode == "altmove") {
			animator.SetInteger("MoveMode", 6);
		} else if (moveMode == "seedbomb") {
			animator.SetInteger("MoveMode", 7);
		} else if (moveMode == "seedbomb") {
			animator.SetInteger("MoveMode", 7);
		} else if (moveMode == "attack") {
			animator.SetInteger("MoveMode", 8);
		} else if (moveMode == "attack-up") {
			animator.SetInteger("MoveMode", 9);
		} else if (moveMode == "attack-down") {
			animator.SetInteger("MoveMode", 10);
		} else if (moveMode == "parry") {
			animator.SetInteger("MoveMode", 11);
		}
		animator.SetFloat("xSpeed", Mathf.Abs(rigidbody2D.velocity.x));
		animator.SetFloat("ySpeed", rigidbody2D.velocity.y);
		animator.SetBool("Jumping", jumping);
		animator.SetBool("Grounded", isGrounded);
		if (rigidbody2D.velocity.magnitude < 0.5 && Input.GetAxisRaw("Vertical") > 0.9) {
			animator.SetInteger("LookDirection", 1);
		} else if (rigidbody2D.velocity.magnitude < 0.5 && Input.GetAxisRaw("Vertical") < -0.9) {
			animator.SetInteger("LookDirection", -1);
		} else {
			animator.SetInteger("LookDirection", 0);	
		}
	}
}
