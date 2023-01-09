using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class AxisPlayerController : Player
{
	// particle systems
	public ParticleSystem dustPS;
	public ParticleSystem floatPS;
	public ParticleSystem doubleJumpPS;
	public ParticleSystem dashPS;

	// gravity
	public float mpPerJump = 50f;

	// wavebeam
	public Transform firePoint;
	public GameObject wavePrefab;
	private float gravWaveActiveTime = 0f;
	private float gravWaveReleaseTime = 0.25f;
	private bool releasedGravWave = false;
	private float gravWaveDuration = 0.583f;
	private float mpPerCast = 50f;

	//seedbomb
	public GameObject seedBombPrefab;
	private float seedBombActiveTime = 0f;
	private float seedBombReleaseTime = 0.33f;
	private bool releasedSeedBomb, ableToUseSeedBomb, seedBombActive = false;
	private float seedBombDuration = 0.5f;
	private float timeSinceSeedBomb, seedBombCooldown = 1.5f;

	//blackhole
	public GameObject blackHolePrefab;
	private float blackHoleActiveTime = 0f;
	private float blackHoleReleaseTime = 0.4167f;
	private bool releasedBlackHole, blackHoleActive = false;
	private float blackHoleDuration = 0.83f;

	//dash
	private float dashActiveTime = 0f;
	private float dashDuration = 0.15f;
	private float dashSpeed = 20f;
	private float timeSinceDash = 0f;
	private float dashCooldown = 0.5f;

	//attack
	private float attackActiveTime = 0f;
	private float attackDuration = 0.4167f;
	private bool continueAttack = false;
	private float upAttackDuration = 0.4167f;
	private float downAttackDuration = 0.4167f;
	private float attackActivateTime = 0.083f;
	private float attackActiveDuration = 0.167f;
	private GameObject hitboxN;
	private GameObject hitboxU;
	private GameObject hitboxD;

	void Start() {
		base.Start();
		runSpeed = 30f;
		timeSinceSeedBomb = seedBombCooldown;
		ableToUseSeedBomb = true;
		hitboxN = transform.Find("AxisAttackHitboxN").gameObject;
		hitboxU = transform.Find("AxisAttackHitboxU").gameObject;
		hitboxD = transform.Find("AxisAttackHitboxD").gameObject;
		hitboxN.SetActive(false);
		hitboxU.SetActive(false);
		hitboxD.SetActive(false);
	}

    void Update()
    {
		if (gameManager.currentPlayer == "Axis" && gameManager.gameState == "play")
		{
			base.Update();
			if (gameManager.CanAct() && Input.GetButtonDown("Attack") && moveMode == "normal") {
				if (Input.GetAxisRaw("Vertical") > 0.75) {
					moveMode = "attack-up";
				} else if (Input.GetAxisRaw("Vertical") < -0.75 && !isGrounded) {
					moveMode = "attack-down";
				} else {
					moveMode = "attack";
				}
				attackActiveTime = 0f;
				continueAttack = false;
			}
			if (gameManager.CanAct() && Input.GetButtonDown("Spell") && moveMode == "normal") {
				moveMode = "cast";
				gravWaveActiveTime = 0f;
				releasedGravWave = false;
			}
			if (gameManager.CanAct() && Input.GetButtonDown("Spell2") && moveMode == "normal" && isGrounded && gameManager.playerMP == gameManager.maxMP) {
				moveMode = "cast2";
				blackHoleActiveTime = 0f;
				releasedBlackHole = false;
			}
			if (gameManager.CanAct() && Input.GetButtonDown("SeedBomb") && moveMode == "normal" && ableToUseSeedBomb) {
				moveMode = "seedbomb";
				seedBombActiveTime = 0f;
				releasedSeedBomb = false;
			}
		}
    }

    void FixedUpdate()
    {
		if (gameManager.currentPlayer == "Axis" && gameManager.gameState == "play")
		{
			base.FixedUpdate();
			if (isGrounded && (moveMode == "normal" || moveMode == "attack" || moveMode == "attack-up") && !blackHoleActive) {
				gameManager.playerMP += 2;
			}
			if (moveMode == "attack") {
				Attack();
			}
			if (moveMode == "attack-up") {
				AttackUp();
			}
			if (moveMode == "attack-down") {
				AttackDown();
			}
			if (moveMode == "cast") {
				Cast();
			}
			if (moveMode == "cast2") {
				BlackHole();
			}
			if (moveMode == "seedbomb") {
				ThrowSeedBomb();
			}
			if (moveMode == "altmove") {
				Dash();
			} else {
				timeSinceDash += Time.deltaTime;
			}
			if (!seedBombActive) {
				timeSinceSeedBomb += Time.deltaTime;
				if (!ableToUseSeedBomb && timeSinceSeedBomb >= seedBombCooldown) {
					ableToUseSeedBomb = true;
				}
			}
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
		if (gameManager.playerMP < mpPerCast) {
			moveMode = "normal";
			return;
		}
		if (gravWaveActiveTime >= gravWaveReleaseTime && !releasedGravWave) {
			Instantiate(wavePrefab, firePoint.position, firePoint.rotation);
			gameManager.playerMP -= mpPerCast;
			releasedGravWave = true;
		}
		if (gravWaveActiveTime >= gravWaveDuration) {
			moveMode = "normal";
		}
		gravWaveActiveTime += Time.deltaTime;
	}

	protected void BlackHole() {
		rigidbody2D.velocity = Vector2.zero;
		blackHoleActive = true;
		if (blackHoleActiveTime >= blackHoleReleaseTime && !releasedBlackHole) {
			Instantiate(blackHolePrefab, firePoint.position + new Vector3(facingRight ? 1.5f : -1.5f, DistanceToGround(), 0f), new Quaternion(0f, 0f, 0f, 0f));
			releasedBlackHole = true;
			GameObject.Find("Main Camera").GetComponent<Camera2DFollow>().StartShake(0.5f, 0.08f, true);
		}
		if (blackHoleActiveTime >= blackHoleDuration) {
			moveMode = "normal";
		}
		blackHoleActiveTime += Time.deltaTime;
	}

	protected void ThrowSeedBomb() {
		seedBombActive = true;
		ableToUseSeedBomb = false;
		if (seedBombActiveTime >= seedBombReleaseTime && !releasedSeedBomb) {
			Instantiate(seedBombPrefab, firePoint.position, firePoint.rotation);
			releasedSeedBomb = true;
		}
		if (seedBombActiveTime >= seedBombDuration) {
			moveMode = "normal";
		}
		seedBombActiveTime += Time.deltaTime;
	}

	protected void Dash() {
		if (timeSinceDash < dashCooldown) {
			moveMode = "normal";
			return;
		}
		if (!dashPS.isPlaying) {
			dashPS.Play();
		}
		rigidbody2D.velocity = new Vector2(facingRight ? dashSpeed : -dashSpeed, 0f);
		dashActiveTime += Time.deltaTime;
		if (dashActiveTime >= dashDuration) {
			moveMode = "normal";
			dashActiveTime = 0;
			dashPS.Clear();
			dashPS.Stop();
			timeSinceDash = 0f;
		}
	}

	protected void Attack() {
		if (Input.GetButtonDown("Attack")) {
			continueAttack = true;
		}
		if (attackActiveTime % attackDuration >= attackActivateTime && !hitboxN.activeSelf) {
			hitboxN.SetActive(true);
		}
		if (attackActiveTime % attackDuration >= attackActivateTime + attackActiveDuration && hitboxN.activeSelf) {
			hitboxN.SetActive(false);
		}
		if ((!continueAttack && attackActiveTime >= attackDuration) || (continueAttack && attackActiveTime >= attackDuration*2)) {
			moveMode = "normal";
		}
		attackActiveTime += Time.deltaTime;
	}

	protected void AttackUp() {
		if (attackActiveTime >= attackActivateTime && !hitboxU.activeSelf) {
			hitboxU.SetActive(true);
		}
		if (attackActiveTime >= attackActivateTime + attackActiveDuration && hitboxU.activeSelf) {
			hitboxU.SetActive(false);
		}
		if (attackActiveTime >= upAttackDuration) {
			moveMode = "normal";
		}
		attackActiveTime += Time.deltaTime;
	}

	protected void AttackDown() {
		if (attackActiveTime >= attackActivateTime && !hitboxD.activeSelf) {
			hitboxD.SetActive(true);
		}
		if (attackActiveTime >= attackActivateTime + attackActiveDuration && hitboxD.activeSelf) {
			hitboxD.SetActive(false);
		}
		if (attackActiveTime >= downAttackDuration) {
			moveMode = "normal";
		}
		attackActiveTime += Time.deltaTime;
	}

	public void BeginSeedBombCooldown() {
		timeSinceSeedBomb = 0f;
		seedBombActive = false;
	}

	public void DeactivateBlackHole() {
		blackHoleActive = false;
	}
}
