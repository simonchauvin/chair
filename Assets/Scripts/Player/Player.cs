using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float lifeExpectancy = 600;

    private float age;


    public void Init()
    {
        age = 0;
    }

    void Update()
    {
        if (GameManager.Instance.IsReady())
        {
            age += Time.deltaTime;
            if (age >= lifeExpectancy)
            {
                GameManager.Instance.Restart();
            }
        }
    }
}
