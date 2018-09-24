using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringWheel : MonoBehaviour {
	public Transform toRotate;
	float initAngle;
	
	void Update () {
		if (Input.GetMouseButtonDown (0)) {
			initAngle = WheelAngle ();
		} else if (Input.GetMouseButton (0)) {
			float newAngle = WheelAngle ();
			float appliedRotation = Mathf.DeltaAngle (initAngle, newAngle);

            transform.Rotate(Vector3.forward, appliedRotation);

            if (GameManager.instance.GetGameState() == GameManager.GameState.Play)
                toRotate.Rotate (Vector3.forward, appliedRotation / 4);

			initAngle = newAngle;
		}
	}

	public float WheelAngle()
	{
		Vector2 dir = transform.position - Camera.main.ScreenToWorldPoint (Input.mousePosition);
		float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
		return angle;
	}
}
