using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class AxisPlayerController : Player
{
	// particle systems
	public ParticleSystem dustPS;
	public ParticleSystem floatPS;
	public ParticleSystem doubleJumpPS;

	// gravity
	public float mpPerJump = 50f;

	// wavebeam
	public Transform firePoint;
	public GameObject wavePrefab;
	private int framesSinceLastWave = 0;
	private int gravWaveCooldownFrames = 20;
	private float mpPerCast = 50f;

	//seedbomb
	public GameObject seedbombPrefab;
	private int framesSinceLastBomb = 0;
	private int seedBombCooldownFrames = 30;

	void Start() {
		base.Start();
		runSpeed = 30f;
	}

    void Update()
    {
		if (gameManager.currentPlayer == "Axis" && gameManager.gameState == "play")
		{
			base.Update();
			if (gameManager.CanAct() && Input.GetButtonDown("Spell") && moveMode == "normal")
			{
				moveMode = "cast";
			}
			if (gameManager.CanAct() && Input.GetButtonDown("SeedBomb") && moveMode == "normal")
			{
				moveMode = "seedbomb";
			}
		}
    }

    void FixedUpdate()
    {
		if (gameManager.currentPlayer == "Axis" && gameManager.gameState == "play")
		{
			base.FixedUpdate();
			if (isGrounded && moveMode == "normal") {
				gameManager.playerMP += 2;
			}
			if (moveMode == "cast") {
				Cast();
			}
			if (moveMode == "seedbomb") {
				ThrowSeedBomb();
			}
			framesSinceLastWave++;
			framesSinceLastBomb++;
		}
    }

    protected override void Move(float move, bool jump)
	{
		base.Move(move, jump);
		bool wasGrounded = CheckGrounded();
		if (jump && canJump && !wasGrounded && gameManager.playerMP >= mpPerJump && framesSinceLeavingGround > graceFrames)
		{
			rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, 0);
			rigidbody2D.AddForce(new Vector2(0f, jumpForce));
			gameManager.playerMP -= mpPerJump;
			doubleJumpPS.Play();
			canJump = false;
		}
	}

	protected override void Float()
	{
		if (gameManager.playerMP <= 0) {
			rigidbody2D.gravityScale = 5.0f;
			moveMode = "normal";
			floatPS.Clear();
			floatPS.Stop();
			return;
		}
		if (!floatPS.isPlaying) {
			floatPS.Play();
		}
		rigidbody2D.gravityScale = 0.0f;
		float hmove = Input.GetAxisRaw("Horizontal");
		float vmove = Input.GetAxisRaw("Vertical");
		Vector3 targetVelocity = new Vector2(hmove * 6f, vmove * 6f);
		rigidbody2D.velocity = Vector3.SmoothDamp(rigidbody2D.velocity, targetVelocity, ref velocity, movementSmoothing);
		gameManager.playerMP -= 1f;
	}

	public void StopFloat() {
		rigidbody2D.gravityScale = 5.0f;
		moveMode = "normal";
		floatPS.Clear();
		floatPS.Stop();
	}

	public void LoadFromPlayerState(PlayerState ps) {
		base.LoadFromPlayerState(ps);
		if (this.moveMode == "up-transition") {
			rigidbody2D.AddForce(new Vector2(facingRight ? 110f : -110f, 900f));
		}
	}

	protected void Cast() {
		if (framesSinceLastWave < gravWaveCooldownFrames || gameManager.playerMP < mpPerCast) {
			moveMode = "normal";
			return;
		}
		Instantiate(wavePrefab, firePoint.position, firePoint.rotation);
		gameManager.playerMP -= mpPerCast;
		moveMode = "normal";
		framesSinceLastWave = 0;
	}

	protected void ThrowSeedBomb() {
		if (framesSinceLastBomb < seedBombCooldownFrames) {
			moveMode = "normal";
			return;
		}
		Instantiate(seedbombPrefab, firePoint.position, firePoint.rotation);
		moveMode = "normal";
		framesSinceLastBomb = 0;
	}
}
