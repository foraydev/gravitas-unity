using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transition : MonoBehaviour
{
    public string targetScene;
    public float targetX = 0.0f;
    public float targetXFacingLeft;
    public float targetY = 0.0f;
    public string transitionType;

    public string GetTargetTransitionType() {
        return transitionType.ToUpper().Substring(transitionType.Length-1);
    }
}
