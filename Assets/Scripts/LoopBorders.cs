using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopBorders : MonoBehaviour {
    [SerializeField]
    private float m_xBorderBuffer = 10f;
    [SerializeField]
    private float m_yBorderBuffer = 10f;

    // Update is called once per frame
    void Update()
    {
        Vector2 transformPos = Camera.main.WorldToScreenPoint(transform.position);
        if (transformPos.x > Screen.width + m_xBorderBuffer)
        {
            transform.position =
                Camera.main.ScreenToWorldPoint(new Vector3(-m_xBorderBuffer, transformPos.y, transform.position.z - Camera.main.transform.position.z));

        }
        else if (transformPos.x < -m_xBorderBuffer)
        {
            transform.position =
                Camera.main.ScreenToWorldPoint(new Vector3(Screen.width + m_xBorderBuffer, transformPos.y, transform.position.z - Camera.main.transform.position.z));
        }

        if (transformPos.y > Screen.height + m_yBorderBuffer)
        {
            transform.position =
                Camera.main.ScreenToWorldPoint(new Vector3(transformPos.x, -m_yBorderBuffer, transform.position.z - Camera.main.transform.position.z));

        }
        else if (transformPos.y < -m_yBorderBuffer)
        {
            transform.position =
                Camera.main.ScreenToWorldPoint(new Vector3(transformPos.x, Screen.height + m_yBorderBuffer, transform.position.z - Camera.main.transform.position.z));
        }
    }
}
