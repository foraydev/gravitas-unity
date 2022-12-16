using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CameraLockZone
{
    private GameManager gameManager;
    public string zoneType;
    public float zoneX, zoneY, zoneW, zoneH, focusX, focusY;
    public string zoneFlag;
    public bool activeValue;
    
    void Start() {
        gameManager = GameManager.Instance;
    }

    public Vector3 GetCameraTarget() {
        Vector3 playerPos = GameManager.Instance.currentPlayerTransform.position;
        if (this.zoneType == "fake-left") {
            return new Vector3(Mathf.Clamp(playerPos.x, this.zoneX + Constants.CAM_VIEWPORT_WIDTH/2, Constants.INFINTY), playerPos.y, Constants.CAM_Z);
        }
        if (this.zoneType == "fake-right") {
            return new Vector3(Mathf.Clamp(playerPos.x, -Constants.INFINTY, this.zoneX + this.zoneW - Constants.CAM_VIEWPORT_WIDTH/2), playerPos.y, Constants.CAM_Z);
        }
        if (this.zoneType == "fake-top") {
            return new Vector3(playerPos.x, Mathf.Clamp(playerPos.y, -Constants.INFINTY, this.zoneY - Constants.CAM_VIEWPORT_WIDTH/2), Constants.CAM_Z);
        }
        if (this.zoneType == "fake-bottom") {
            return new Vector3(playerPos.x, Mathf.Clamp(playerPos.y, this.zoneY - this.zoneH + Constants.CAM_VIEWPORT_HEIGHT/2, Constants.INFINTY), Constants.CAM_Z);
        }
        if (this.zoneType == "fixed") {
            return new Vector3(this.focusX, this.focusY, Constants.CAM_Z);
        }
        return DefaultCameraTarget();
    }

    public bool IsActive() {
        Vector3 playerPos = GameManager.Instance.currentPlayerTransform.position;
        return playerPos.x > this.zoneX && playerPos.x < this.zoneX + this.zoneW && playerPos.y > this.zoneY - zoneH && playerPos.y < this.zoneY && (this.zoneFlag.ToLower() == "none" || (this.zoneFlag.ToLower() != "none" && GameManager.Instance.GetSceneFlag(zoneFlag) == activeValue));
    }

    public static Vector3 DefaultCameraTarget() {
        Vector3 playerPos = GameManager.Instance.currentPlayerTransform.position;
        return new Vector3(playerPos.x, playerPos.y, Constants.CAM_Z);
    }
}
