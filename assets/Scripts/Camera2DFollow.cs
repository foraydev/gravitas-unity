using UnityEngine;
using System.Collections;

public class Camera2DFollow : MonoBehaviour {
	
	public float damping = 1;
	public float lookAheadFactor = 3;
	public float lookAheadReturnSpeed = 0.5f;
	public float lookAheadMoveThreshold = 0.1f;
	public float yPosRestriction = -1;
	public float sceneBottomEdge;
	public float sceneLeftEdge;
	public float sceneWidth;
	public float sceneHeight;
	public GameManager gameManager;

	public CameraLockZone[] zones;
	
	float offsetZ;
	Vector3 lastTargetPosition;
	Vector3 currentVelocity;
	Vector3 lookAheadPos;

	float nextTimeToSearch = 0;

	private Vector3 target;
	private float shakeTimeRemaining;
    private float shakeIntensity;
	private float shakeFadeTime;
	private int direction; // -1 = pan down; 0 = normal; 1 = pan up
	
	// Use this for initialization
	void Start () {
		lastTargetPosition = target;
		offsetZ = (transform.position - target).z;
		transform.parent = null;
		gameManager = GameManager.Instance;
	}
	
	// Update is called once per frame
	void Update () {
		target = GetTarget();

		float xMoveDelta = (target - lastTargetPosition).x;

	    bool updateLookAheadTarget = Mathf.Abs(xMoveDelta) > lookAheadMoveThreshold;

		if (updateLookAheadTarget) {
			lookAheadPos = lookAheadFactor * Vector3.right * Mathf.Sign(xMoveDelta);
		} else {
			lookAheadPos = Vector3.MoveTowards(lookAheadPos, Vector3.zero, Time.deltaTime * lookAheadReturnSpeed);	
		}

		if (CanPanUp() && Input.GetAxisRaw("Vertical") > 0.9) {
			direction = 1;
		} else if (CanPanDown() && Input.GetAxisRaw("Vertical") < -0.9) {
			direction = -1;
		} else {
			direction = 0;
		}
		
		Vector3 aheadTargetPos = target + lookAheadPos + Vector3.forward * offsetZ + Vector3.up * direction * 4;
		aheadTargetPos = new Vector3(Mathf.Clamp(aheadTargetPos.x, sceneLeftEdge + Constants.CAM_VIEWPORT_WIDTH/2, sceneLeftEdge + sceneWidth - Constants.CAM_VIEWPORT_WIDTH/2), Mathf.Clamp(aheadTargetPos.y, sceneBottomEdge + Constants.CAM_VIEWPORT_HEIGHT/2, sceneBottomEdge + sceneHeight - Constants.CAM_VIEWPORT_HEIGHT/2), Constants.CAM_Z);
		Vector3 newPos = Vector3.SmoothDamp(transform.position, aheadTargetPos, ref currentVelocity, damping);

		transform.position = newPos;
		
		lastTargetPosition = target;
	}

	void LateUpdate() {
		if (shakeTimeRemaining > 0) {
			shakeTimeRemaining -= Time.deltaTime;
			float xShake = Random.Range(-1f, 1f) * shakeIntensity;
			float yShake = Random.Range(-1f, 1f) * shakeIntensity;
			transform.position += new Vector3(xShake, yShake, 0);
			shakeIntensity = Mathf.MoveTowards(shakeIntensity, 0, shakeFadeTime * Time.deltaTime);
		}
	}

	Vector3 GetTarget() {
		if (zones.Length == 0) { return CameraLockZone.DefaultCameraTarget(); }
		foreach (CameraLockZone z in zones) {
			if (z.IsActive()) {
				return z.GetCameraTarget();
			}
		}
		return CameraLockZone.DefaultCameraTarget();
	}

    public void StartShake(float duration, float intensity, bool fadeOut)
    {
        shakeTimeRemaining = duration;
        shakeIntensity = intensity;
		shakeFadeTime = fadeOut ? intensity / duration : 0;
    }

	private bool CanPanUp() {
		foreach (CameraLockZone z in zones) {
			if (z.IsActive() && z.zoneType == "fake-top" && transform.position.y + 4 > z.zoneY) {
				return false;
			}
		}
		return CanPan();
	}

	private bool CanPanDown() {
		foreach (CameraLockZone z in zones) {
			if (z.IsActive() && z.zoneType == "fake-bottom" && transform.position.y - 4 < z.zoneY + z.zoneH) {
				return false;
			}
		}
		return CanPan();
	}

	private bool CanPan() {
		foreach (CameraLockZone z in zones) {
			if (z.IsActive() && z.zoneType == "fixed") {
				return false;
			}
		}
		return gameManager.currentPlayerTransform.gameObject.GetComponent<Rigidbody2D>().velocity.magnitude < 0.5 && gameManager.currentPlayerTransform.gameObject.GetComponent<Player>().moveMode == "normal";
	}
}
