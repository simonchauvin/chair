using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinController : MonoBehaviour
{
    [SerializeField]
    private int skinWidth = 20;
    [SerializeField]
    private int skinHeight = 10;
    [SerializeField]
    private float skinCellSize = 1;
    [SerializeField]
    private float lifeExpectancy = 600;

    private Grid epidermis;
    private Grid dermis;
    private Grid hypodermis;

    private float age;


    public void Init()
    {
        epidermis = GetComponentsInChildren<Grid>()[0];
        dermis = GetComponentsInChildren<Grid>()[1];
        hypodermis = GetComponentsInChildren<Grid>()[2];

        epidermis.GridInitWidth = skinWidth;
        epidermis.GridInitHeight = skinHeight;
        epidermis.CellSize = skinCellSize;
        epidermis.Init();
        
        dermis.GridInitWidth = skinWidth;
        dermis.GridInitHeight = skinHeight;
        dermis.CellSize = skinCellSize;
        dermis.Init();

        hypodermis.GridInitWidth = skinWidth;
        hypodermis.GridInitHeight = skinHeight;
        hypodermis.CellSize = skinCellSize;
        hypodermis.Init();

        age = 0;
    }

    // Update is called once per frame
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

    public Vector3 GetSize()
    {
        return new Vector2(skinWidth, skinHeight);
    }
}
