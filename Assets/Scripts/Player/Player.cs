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
    private int baseRippingFactor = 1;
    [SerializeField]
    private int maxRippingFactor = 10;

    private Grid[] dermisLayers;
    private Touch[] touches;
    private List<Grid.Mass> massesToMove = new List<Grid.Mass>();

    private int lastTouchCount;
    private float lastDistanceBetweenFingers;
    private Vector2 fingersVector;
    private float startingFingersDistance;
    private float fingersDistance;


    public void Init(Grid[] dermisLayers)
    {
        this.dermisLayers = dermisLayers;
        touches = new Touch[maxSimultaneousTouches];

        lastTouchCount = 0;
        fingersVector = Vector3.zero;
        startingFingersDistance = 0;
        fingersDistance = 0;
    }

    void Update()
    {
        if (GameManager.Instance.IsReady())
        {
#if UNITY_STANDALONE
            Vector3 worldPos = GetWorldPosition(Input.mousePosition);

            if (Input.GetButton("Fire1"))
                TouchSkin(worldPos, baseRippingFactor);
#elif UNITY_EDITOR
            Vector3 worldPos = GetWorldPosition(Input.mousePosition);

            if (Input.GetButton("Fire1"))
            {
                TouchSkin(worldPos, baseRippingFactor);
            }

            if (Input.GetButtonDown("Fire2"))
            {
                for (int j = 0; j < dermisLayers.Length; j++)
                {
                    dermisLayers[j].GetMassesCloseTo(massesToMove, worldPos, 0.5f);
                    if (massesToMove.Count > 0)
                    {
                        break;
                    }
                }
            }

            if (Input.GetButton("Fire2"))
            {
                SelectSkinNode(worldPos);
            }

            if (Input.GetButtonDown("Fire3"))
            {
                CutSkin(worldPos);
            }
#else
            for (int i = 0; i < Input.touchCount && i < touches.Length; i++)
            {
                touches[i] = Input.GetTouch(i);

                TouchSkin(touches[i].position, baseRippingFactor);
            }

            if (Input.touchCount > 1)
            {
                fingersVector = touches[0].position - touches[1].position;
                fingersDistance = fingersVector.magnitude;

                if (Input.touchCount != lastTouchCount)
                {
                    startingFingersDistance = fingersVector.magnitude;
                }

                if (Mathf.Abs(lastDistanceBetweenFingers - fingersDistance) >= minDistanceBeforeRipping)
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
                    m.AddForce((worldPosition - m.Position) * rippingFactor + new Vector3(0, 0, 2));
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

    private void OnDrawGizmos()
    {
        if (Application.isEditor && Application.isPlaying)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < touches.Length; i++)
            {
                Gizmos.DrawWireSphere(touches[i].position, touchRadius);
            }
        }
    }
}