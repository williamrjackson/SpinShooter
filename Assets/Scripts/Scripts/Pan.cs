using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pan : MonoBehaviour {
    public Transform targetTransform;
    public float percent = .25f;
    public float speed = 1;

    private bool lockRight;
    void Update () {
        Vector2 pos = Camera.main.WorldToScreenPoint(targetTransform.position);

        if (pos.x < Screen.width * .5)
        {
            if (pos.x < Screen.width - (Screen.width * percent))
            {
                transform.position -= transform.right * speed * Time.deltaTime;
            }
        }
        else
        {
            if (pos.x > Screen.width * percent)
            {
                transform.position += transform.right * speed * Time.deltaTime;
            }
        }
	}
}
