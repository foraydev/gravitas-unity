using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MusicZone
{
    public AudioClip music;
    public float zoneX, zoneY, zoneW, zoneH;
    public string zoneFlag;
    public bool activeValue;

    public bool IsActive() {
        if (GameManager.Instance.currentPlayerTransform == null) {
            return false;
        }
        Vector3 playerPos = GameManager.Instance.currentPlayerTransform.position;
        return playerPos.x > this.zoneX && playerPos.x < this.zoneX + this.zoneW && playerPos.y > this.zoneY - zoneH && playerPos.y < this.zoneY && (this.zoneFlag.ToLower() == "none" || (this.zoneFlag.ToLower() != "none" && GameManager.Instance.GetSceneFlag(zoneFlag) == activeValue));
    }
}
