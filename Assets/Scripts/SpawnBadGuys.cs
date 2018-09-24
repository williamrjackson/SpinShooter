using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnBadGuys : MonoBehaviour {
    public float radius = 2.5f;
    public float rate = 1f;
    public float timeToArrive = 5f;
    public Wrj.WeightedGameObjects badGuys; 
	// Use this for initialization
	void Start () {
        StartCoroutine(SpawnAtPerimeter());
	}

    IEnumerator SpawnAtPerimeter()
    {
        while (GameManager.instance.GetGameState() == GameManager.GameState.Play)
        {
            // Only spawn behind. Thought here is that since you're moving backwards, and being chased, it's more fun to have 
            // Bad Guys appearing from the direction you're not facing... Makes it a little hard, though.
            Vector3 spawnPos;
            do
            {
                spawnPos = transform.position + Quaternion.AngleAxis(Random.Range(0f, 359.9f), Vector3.forward) * (Vector3.right * radius);
            }
            while (Vector3.Dot(transform.up, spawnPos) > 0);

            GameObject badGuy = Instantiate(badGuys.GetRandom());
            badGuy.transform.position = spawnPos;
            badGuy.transform.parent = null;
            //Wrj.Utils.MapToCurve.EaseIn.MoveWorld(badGuy.transform, transform.position, timeToArrive);
            yield return new WaitForSeconds(rate);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }


}
