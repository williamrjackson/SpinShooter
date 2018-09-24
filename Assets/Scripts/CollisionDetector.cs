using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetector : MonoBehaviour {
    public GameObject sprite;
    public ParticleSystem[] particles;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnCollisionEnter2D(Collision2D col)
    {
        if (GameManager.instance.GetGameState() == GameManager.GameState.GameOver)
            return;

        print("Hit!" + " - " + col.collider.name);
        foreach (ParticleSystem p in particles)
        {
            if (!p.isPlaying)
                p.Play();
        }
        if (sprite.activeInHierarchy)
            sprite.SetActive(false);

        GameManager.instance.SetGameState(GameManager.GameState.GameOver);
    }
}
