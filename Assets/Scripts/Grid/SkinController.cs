using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinController : MonoBehaviour
{
    [SerializeField]
    private Grid dermisLayerPrefab;
    [SerializeField]
    private int dermisCount;
    [SerializeField]
    private int skinWidth = 20;
    [SerializeField]
    private int skinHeight = 10;
    [SerializeField]
    private float skinCellSize = 1;
    [SerializeField]
    private float lifeExpectancy = 600;

    private Grid[] dermisLayers;

    private float age;


    public void Init()
    {
        dermisLayers = new Grid[dermisCount];
        for (int i = 0; i < dermisCount; i++)
        {
            dermisLayers[i] = Instantiate(dermisLayerPrefab, Vector3.zero, Quaternion.identity, transform);

            dermisLayers[i].GridInitWidth = skinWidth;
            dermisLayers[i].GridInitHeight = skinHeight;
            dermisLayers[i].CellSize = skinCellSize;
            dermisLayers[i].Init();
        }

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

    public Grid[] GetDermisLayers()
    {
        return dermisLayers;
    }
}
