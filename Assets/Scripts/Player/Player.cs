using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float touchRadius = 0.5f;
    [SerializeField]
    private int maxSimultaneousTouches = 2;
    [SerializeField]
    private float minDistanceBeforeRipping = 0.5f;
    [SerializeField]
    private int baseTearingFactor = 1;
    [SerializeField]
    private int maxTearingFactor = 2;
    [SerializeField]
    private float maxTearingDeltaPosition = 40;
    [SerializeField]
    private int baseRippingFactor = 1;
    [SerializeField]
    private int maxRippingFactor = 10;

    private Grid[] dermisLayers;
    private Touch[] touches;
    private List<Grid.Mass> massesToMove = new List<Grid.Mass>();

    private int lastTouchCount;
    private Vector3 lastClickPosition;
    private float clickDeltaPosition;
    private Vector2[] lastTouchesPosition;
    private float[] touchesDeltaPosition;
    private float lastDistanceBetweenFingers;
    private Vector2 fingersVector;
    private float startingFingersDistance;
    private float fingersDistance;


    public void Init(Grid[] dermisLayers)
    {
        this.dermisLayers = dermisLayers;
        touches = new Touch[maxSimultaneousTouches];

        lastTouchCount = 0;
        clickDeltaPosition = 0;
        lastTouchesPosition = new Vector2[maxSimultaneousTouches];
        touchesDeltaPosition = new float[maxSimultaneousTouches];
        fingersVector = Vector3.zero;
        startingFingersDistance = 0;
        fingersDistance = 0;
    }

    void Update()
    {
        if (GameManager.Instance.IsReady())
        {
#if UNITY_EDITOR
            Vector3 worldPos = GetWorldPosition(Input.mousePosition);

            ProcessTearingBehaviour(worldPos); // Tearing

            if (Input.GetButton("Fire2"))
            {
                SelectSkinNode(worldPos);
            }

            if (Input.GetButtonDown("Fire3"))
            {
                CutSkin(worldPos);
            }
#elif UNITY_STANDALONE
            ProcessTearingBehaviour(GetWorldPosition(Input.mousePosition));
#else
            int fingerId;
            for (int i = 0; i < Input.touchCount && i < touches.Length; i++)
            {
                fingerId = Input.GetTouch(i).fingerId;

                touches[fingerId] = Input.GetTouch(i);

                if (touches[fingerId].phase == TouchPhase.Began)
                {
                    lastTouchesPosition[fingerId] = touches[fingerId].position;
                }
                else if (touches[fingerId].phase == TouchPhase.Moved)
                {
                    touchesDeltaPosition[fingerId] = (lastTouchesPosition[fingerId] - touches[fingerId].position).magnitude;
                    lastTouchesPosition[fingerId] = touches[fingerId].position;
                }

                // Tearing
                TouchSkin(GetWorldPosition(touches[fingerId].position), Mathf.Lerp(baseTearingFactor, maxTearingFactor, maxTearingDeltaPosition - touches[fingerId].deltaPosition.magnitude));
            }

            if (Input.touchCount > 1)
            {
                fingersVector = touches[0].position - touches[1].position;
                fingersDistance = fingersVector.magnitude;

                if (Input.touchCount != lastTouchCount)
                {
                    startingFingersDistance = fingersVector.magnitude;
                }

                if (Mathf.Abs(lastDistanceBetweenFingers - fingersDistance) >= minDistanceBeforeRipping) // Ripping
                {
                    TouchSkin(touches[1].position + fingersVector.normalized * (fingersDistance * 0.5f),
                        Mathf.Clamp(fingersDistance / startingFingersDistance, baseRippingFactor, maxRippingFactor));
                }
                lastDistanceBetweenFingers = fingersDistance;
            }

            lastTouchCount = Input.touchCount;
#endif
        }
    }

    private void ProcessTearingBehaviour(Vector3 worldPosition)
    {
        if (Input.GetButtonDown("Fire1"))
        {
            lastClickPosition = Input.mousePosition;
        }

        if (Input.GetButton("Fire1"))
        {
            if ((Input.mousePosition - lastClickPosition).magnitude > Mathf.Epsilon)
            {
                clickDeltaPosition = (lastClickPosition - Input.mousePosition).magnitude;
                lastClickPosition = Input.mousePosition;
            }

            TouchSkin(worldPosition, Mathf.Lerp(baseTearingFactor, maxTearingFactor, maxTearingDeltaPosition - clickDeltaPosition));
        }

        if (Input.GetButtonDown("Fire2"))
        {
            for (int j = 0; j < dermisLayers.Length; j++)
            {
                dermisLayers[j].GetMassesCloseTo(massesToMove, worldPosition, 0.5f);
                if (massesToMove.Count > 0)
                {
                    break;
                }
            }
        }
    }

    private Vector3 GetWorldPosition(Vector3 screenPosition)
    {
        screenPosition.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 clickPos = Camera.main.ScreenToWorldPoint(screenPosition);
        clickPos.z = transform.position.z;

        return clickPos;
    }

    private void TouchSkin(Vector3 worldPosition, float rippingFactor)
    {
        for (int j = 0; j < dermisLayers.Length; j++)
        {
            List<Grid.Mass> massesToLink = new List<Grid.Mass>();
            dermisLayers[j].GetMassesCloseTo(massesToLink, worldPosition, touchRadius);
            if (massesToLink.Count > 0)
            {
                foreach (Grid.Mass m in massesToLink)
                {
                    m.AddForce((worldPosition - m.Position) * rippingFactor + new Vector3(0, 0, 3));
                }
                break;
            }
        }
    }

    private void SelectSkinNode(Vector3 worldPosition)
    {
        foreach (Grid.Mass m in massesToMove)
        {
            Vector3 force = (worldPosition - m.Position) * 2;
            m.AddForce(force);
        }
    }

    private void CutSkin(Vector3 worldPosition)
    {
        for (int j = 0; j < dermisLayers.Length; j++)
        {
            List<Grid.Spring> springsToKill = new List<Grid.Spring>();
            dermisLayers[j].GetSpringsCloseTo(springsToKill, worldPosition, touchRadius);
            if (springsToKill.Count > 0)
            {
                foreach (Grid.Spring s in springsToKill)
                {
                    s.Detach();
                }
                break;
            }
        }
    }
}