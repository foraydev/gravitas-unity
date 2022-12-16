using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneFlagManager : MonoBehaviour
{
    public ToggleSceneObject[] sceneObjs;

    void Start()
    {
        foreach(ToggleSceneObject tso in sceneObjs) {
            tso.obj.SetActive(GameManager.Instance.GetSceneFlag(tso.sceneFlag) == tso.activeValue);
        }
    }
}
