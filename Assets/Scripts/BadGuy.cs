using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BadGuy : MonoBehaviour {
    public TextMesh text;
    public int initialValue = 5;
    public float smoothTime = 50f;
    private int m_Value;
    private Transform player;
    private bool dontMerge = false;

    void Start()
    {
        m_Value = initialValue;
        text.text = m_Value.ToString();
        Wrj.Utils.MapToCurve.Ease.ChangeColor(transform, Color.red, 0f);
        player = GameObject.FindWithTag("Player").transform;
    }

    void Update()
    {
        transform.position += (player.position - transform.position).normalized * smoothTime * Time.fixedDeltaTime;
    }


    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.tag == "Projectile")
        {
            col.collider.gameObject.SetActive(false);
            m_Value--;
            GameManager.instance.ModifyScore(1);
            if(m_Value < 1)
            {
                GameManager.instance.ModifyScore(4); // Total 5 points for a shot that destroys a bad guy.
                Destroy(gameObject);
            }
            else
            {
                text.text = m_Value.ToString();
                TintBadGuy();
            }
        }
        else
        {
            BadGuy other = col.collider.GetComponent<BadGuy>();
            if (other != null && !dontMerge)
            {
                EnemyCombinator.instance.Combine(this, other);
            }
        }
    }

    public void SetMerging()
    {
        dontMerge = true;
    }
    private void TintBadGuy()
    {
        Wrj.Utils.MapToCurve.Ease.ChangeColor(transform, Color.Lerp(Color.red, Color.green, Wrj.Utils.Remap(m_Value, initialValue, 1, 0, 1)), .1f);
    }
    public int GetValue()
    {
        return m_Value;
    }
    public void SetValue(int newValue, int newInitValue)
    {
        print("trying to set to " + newValue);
        m_Value = newValue;
        initialValue = newInitValue;
        text.text = m_Value.ToString();
        TintBadGuy();
    }
    public int GetInitialValue()
    {
        return initialValue;
    }
}
