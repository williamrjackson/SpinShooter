using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGScroller : MonoBehaviour {
    public ScrollImage[] scrollImages;
    private Vector3 m_InitialPosition;
	// Use this for initialization
	void Start () {
        m_InitialPosition = transform.position;
	}

    // Update is called once per frame
    void Update() {
        foreach (ScrollImage image in scrollImages)
        {
            float x = Mathf.Repeat(m_InitialPosition.x + transform.position.x * image.scrollPerMeter.x, 1);
            float y = Mathf.Repeat(m_InitialPosition.y + transform.position.y * image.scrollPerMeter.y, 1);
            image.quad.GetComponent<Renderer>().material.SetTextureOffset("_MainTex", new Vector2(x,y));
        }
	}
}
[System.Serializable]
public struct ScrollImage
{
    public GameObject quad;
    public Vector2 scrollPerMeter;
}