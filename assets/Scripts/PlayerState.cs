using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState
{
    public Vector3 position = Vector3.zero;
    public bool facingRight = false;
    public string moveMode = "normal";

    public PlayerState(Player player) {
        this.position = player.gameObject.transform.position;
        this.facingRight = player.facingRight;
        this.moveMode = player.moveMode;
    }
}
