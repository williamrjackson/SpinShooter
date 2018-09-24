using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOver : MonoBehaviour {

    void Start()
    {
        GameManager.instance.OnStateChange += StateChange;
    }

    private void StateChange(GameManager.GameState state)
    {
        if (state == GameManager.GameState.GameOver)
            Wrj.Utils.MapToCurve.Ease.Scale(transform, Vector3.one * 8, 1);
        else
            Wrj.Utils.MapToCurve.Ease.Scale(transform, Vector3.zero, .25f);
    }
}
