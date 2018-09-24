using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButton : MonoBehaviour {

	// Use this for initialization
	void Start () {
        transform.localScale = Vector3.zero;
        GameManager.instance.OnStateChange += StateChange;
	}

    private void StateChange(GameManager.GameState state)
    {
        if (state == GameManager.GameState.GameOver)
        {
            Wrj.Utils.MapToCurve.EaseIn.Scale(transform, Vector3.one, 1);
        }
        else
        {
            Wrj.Utils.MapToCurve.EaseIn.Scale(transform, Vector3.zero, .5f);
        }
    }



    public void SetPlayGameState()
    {
        GameManager.instance.SetGameState(GameManager.GameState.Play);
    }
}
