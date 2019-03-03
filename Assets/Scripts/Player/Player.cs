using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float touchRadius = 0.5f;
    [SerializeField]
    private float touchDepth = 0.5f;
    [SerializeField]
    private int maxSimultaneousTouches = 2;
    [SerializeField]
    private float tapFactor = 1.25f;
    [SerializeField]
    private float baseTearingFactor = 1;
    [SerializeField]
    private float maxTearingFactor = 1.5f;
    [SerializeField]
    private float maxTearingDeltaPosition = 40;
    [SerializeField]
    private float ripRadius = 1f;
    [SerializeField]
    private float minDistanceBeforeRipping = 10f;

    private Grid[] dermisLayers;
    private Touch[] touches;
    private List<Grid.Mass> massesToMove = new List<Grid.Mass>();

    private int lastTouchCount;
    private Vector3 lastClickPosition;
    private Vector3 clickDeltaPosition;
    private Vector2[] lastTouchesPosition;
    private Vector3[] touchesDeltaPosition;
    private float startingDistanceBetweenFingers;
    private Vector3 fingersVector;
    private Vector3 rippingCenter;
    private float fingersDistance;


    public void Init(Grid[] dermisLayers)
    {
        this.dermisLayers = dermisLayers;
        touches = new Touch[maxSimultaneousTouches];

        lastTouchCount = 0;
        clickDeltaPosition = Vector3.zero;
        lastTouchesPosition = new Vector2[maxSimultaneousTouches];
        touchesDeltaPosition = new Vector3[maxSimultaneousTouches];
        startingDistanceBetweenFingers = 0;
        fingersVector = Vector3.zero;
        rippingCenter = Vector3.zero;
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
                RipSkin(worldPos, ripRadius);
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

                    TapSkin(GetWorldPosition(touches[fingerId].position), tapFactor);
                }
                else if (touches[fingerId].phase == TouchPhase.Moved)
                {
                    touchesDeltaPosition[fingerId] = touches[fingerId].position - lastTouchesPosition[fingerId];
                    lastTouchesPosition[fingerId] = touches[fingerId].position;
                }

                // Tearing
                TouchSkin(GetWorldPosition(touches[fingerId].position), touchesDeltaPosition[fingerId].normalized, Mathf.Lerp(baseTearingFactor, maxTearingFactor, maxTearingDeltaPosition - touches[fingerId].deltaPosition.magnitude));
            }

            if (Input.touchCount > 1) // Ripping
            {
                fingersVector = GetWorldPosition(touches[0].position) - GetWorldPosition(touches[1].position);
                fingersDistance = fingersVector.magnitude;

                if (Input.touchCount != lastTouchCount)
                {
                    startingDistanceBetweenFingers = fingersDistance;
                    rippingCenter = GetWorldPosition(touches[1].position) + fingersVector.normalized * (fingersDistance * 0.5f);
                }

                if (Mathf.Abs(startingDistanceBetweenFingers - fingersDistance) >= minDistanceBeforeRipping) // Ripping
                {
                    RipSkin(rippingCenter, ripRadius);
                }
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
                clickDeltaPosition = Input.mousePosition - lastClickPosition;
                lastClickPosition = Input.mousePosition;
            }

            TouchSkin(worldPosition, clickDeltaPosition.normalized, Mathf.Lerp(baseTearingFactor, maxTearingFactor, maxTearingDeltaPosition - clickDeltaPosition.magnitude));
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

    private void TouchSkin(Vector3 worldPosition, Vector3 direction, float factor)
    {
        for (int j = 0; j < dermisLayers.Length; j++)
        {
            List<Grid.Mass> massesToLink = new List<Grid.Mass>();
            dermisLayers[j].GetMassesCloseTo(massesToLink, worldPosition, touchRadius);
            if (massesToLink.Count > 0)
            {
                foreach (Grid.Mass m in massesToLink)
                {
                    m.AddForce(direction * factor + new Vector3(0, 0, touchDepth));
                }
                break;
            }
        }
    }

    private void TapSkin(Vector3 worldPosition, float factor)
    {
        for (int j = 0; j < dermisLayers.Length; j++)
        {
            List<Grid.Mass> massesToLink = new List<Grid.Mass>();
            dermisLayers[j].GetMassesCloseTo(massesToLink, worldPosition, touchRadius);
            if (massesToLink.Count > 0)
            {
                foreach (Grid.Mass m in massesToLink)
                {
                    m.AddForce((m.Position - worldPosition).normalized * factor + new Vector3(0, 0, touchDepth));
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

    private void RipSkin(Vector3 worldPosition, float radius)
    {
        for (int j = 0; j < dermisLayers.Length; j++)
        {
            List<Grid.Spring> springsToKill = new List<Grid.Spring>();
            dermisLayers[j].GetSpringsCloseTo(springsToKill, worldPosition, radius);
            if (springsToKill.Count > 0)
            {
                foreach (Grid.Spring s in springsToKill)
                {
                    s.Detach();

                    SoundController.Instance.PlayRipSound();
                }
                break;
            }
        }
    }
}