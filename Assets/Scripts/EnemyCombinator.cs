using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombinator : MonoBehaviour {
    public static EnemyCombinator instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void Combine(BadGuy a, BadGuy b)
    {
        if (a == null || b == null)
            return;
        print("Combining");
        b.SetMerging();
        int newVal = a.GetValue() + b.GetValue();
        Destroy(b.gameObject);
        a.SetValue(newVal, newVal);
    }
}
