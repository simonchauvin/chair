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
    [Range(0f, 1f)]
    private float skinResolution;
    [SerializeField]
    private float lifeExpectancy = 600;

    private Grid[] dermisLayers;

    private float age;
    private int skinWidth;
    private int skinHeight;


    public void Init(Resolution resolution, ScreenOrientation screenOrientation)
    {
        skinWidth = Mathf.CeilToInt(resolution.height * skinResolution) + 2;
        skinHeight = Mathf.CeilToInt(resolution.width * skinResolution) + 2;
        
        dermisLayers = new Grid[dermisCount];
        for (int i = 0; i < dermisCount; i++)
        {
            dermisLayers[i] = Instantiate(dermisLayerPrefab, new Vector3(0, 0, i), Quaternion.identity, transform);

            dermisLayers[i].GridInitWidth = skinWidth;
            dermisLayers[i].GridInitHeight = skinHeight;
            dermisLayers[i].CellSize = 1;
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
