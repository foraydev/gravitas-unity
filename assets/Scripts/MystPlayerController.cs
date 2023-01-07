using UnityEngine;
using System.Collections;
using System;

public class MystPlayerController : Player
{
	// particle systems
	public ParticleSystem dustPS;
	public ParticleSystem floatPS;

	// sword
	public MystSwordController sword;
	[SerializeField] private LayerMask whatIsSword;
	public string swordMode { get; set; } = "normal";
	protected bool isOnSword = false;

	// dash
	protected int framesSinceLastDash = 0;

	// wallslide
	bool isTouchingFront;
	public Transform frontCheck;
	public bool wallSliding { get; private set; }
	protected float wallSlideSpeed = 5f;

	void Start() {
		base.Start();
		runSpeed = 45f;
	}

    void Update()
    {
		if (gameManager.currentPlayer == "Myst" && gameManager.gameState == "play")
		{
			base.Update();
			if (Input.GetButtonDown("Float"))
			{
				floatPS.Play();
				rigidbody2D.gravityScale = 0.0f;
				framesSinceLastDash = 20;
			}
			if (gameManager.CanAct() && swordMode == "normal")
			{
				if (Input.GetButtonDown("Spell"))
				{
					swordMode = "platform";
				}
			} else if (swordMode == "platform")
			{
				if (Input.GetButtonDown("Spell"))
				{
					sword.EndPlatformMode();
				}
			}
		}
    }

    void FixedUpdate()
    {
		if (gameManager.currentPlayer == "Myst" && gameManager.gameState == "play")
		{
			base.FixedUpdate();
			bool wasWallSliding = wallSliding;
			isTouchingFront = this.CheckTouchingFront();
			isOnSword = this.CheckOnSword();
			wallSliding = this.CheckWallSlide();
			if (wasWallSliding && !wallSliding) {
				OnLeaveGround();
			}
			UpdateAnimator();
			if (isGrounded && moveMode == "normal" && swordMode == "normal") {
				gameManager.playerMP += 2;
			}
			if (moveMode == "altmove") {
				Vault();
			}
		}
    }

    protected override void Move(float move, bool jump)
	{
		base.Move(move, jump);
		if (jump && canJump && isOnSword)
		{
			isOnSword = false;
			canJump = false;
			rigidbody2D.AddForce(new Vector2(0f, jumpForce));
		}
		if (wallSliding) {
			if (jump && canJump) {
				rigidbody2D.AddForce(new Vector2(facingRight ? -2000f : 2000f, 1500f));
			} else {
				rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, Mathf.Clamp(rigidbody2D.velocity.y, -wallSlideSpeed, float.MaxValue));
			}
		}
	}

	protected override void Float()
	{
		if (framesSinceLastDash == 10)
		{
			rigidbody2D.velocity = new Vector2(0, 0);
		}
		if (framesSinceLastDash >= 20)
		{
			if (gameManager.playerMP <= 33f) {
				StopFloat();
				return;
			}
			rigidbody2D.velocity = new Vector2(0, 0);
			rigidbody2D.gravityScale = 0.0f;
			framesSinceLastDash = 0;
			float hmove = Input.GetAxisRaw("Horizontal");
			float vmove = Input.GetAxisRaw("Vertical");
			if (hmove < 0.1 && vmove < 0.1) {
				hmove = facingRight ? 1 : -1;
			}
			rigidbody2D.AddForce(new Vector2(hmove*800, vmove*800));
			gameManager.playerMP -= 33f;
		}
		framesSinceLastDash++;
	}

	public void StopFloat() {
		rigidbody2D.gravityScale = 7.0f;
		moveMode = "normal";
		floatPS.Clear();
		floatPS.Stop();
	}

	public void LoadFromPlayerState(PlayerState ps) {
		base.LoadFromPlayerState(ps);
		if (moveMode == "up-transition") {
			rigidbody2D.AddForce(new Vector2(facingRight ? 130f : -130f, 1100f));
		}
	}

	private void OnTriggerEnter2D(Collider2D collision) {
		base.OnTriggerEnter2D(collision);
	}

	private bool CheckTouchingFront() {
		if (isGrounded || isOnSword) {
			return false;
		}
		Collider2D[] colliders = Physics2D.OverlapCircleAll(frontCheck.position, groundedRadius, whatIsGround);
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i].gameObject != gameObject && colliders[i].gameObject.transform.rotation.eulerAngles.z == 0)
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckOnSword() {
		if (swordMode == "platform")
		{
			Collider2D[] swordCols = Physics2D.OverlapCircleAll(groundCheck.position, groundedRadius, whatIsSword);
			for (int i = 0; i < swordCols.Length; i++)
			{
				if (swordCols[i].gameObject != gameObject)
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool CheckWallSlide() {
		if (isTouchingFront && !isGrounded && horizontalMove != 0) {
			return true;
		}
		if (!isTouchingFront || isGrounded) {
			return false;
		}
		return wallSliding;
	}

	protected void Vault() {
		moveMode = "normal";
	}

	protected void UpdateAnimator() {
		base.UpdateAnimator();
		animator.SetBool("WallCling", wallSliding);
	}
}
