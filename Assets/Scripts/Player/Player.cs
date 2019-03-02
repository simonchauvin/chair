using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float touchRadius = 0.5f;
    [SerializeField]
    private int maxSimultaneousTouches = 2;

    private Grid[] dermisLayers;
    private Touch[] touches;
    private List<Grid.Mass> massesToMove = new List<Grid.Mass>();


    public void Init(Grid[] dermisLayers)
    {
        this.dermisLayers = dermisLayers;
        touches = new Touch[maxSimultaneousTouches];
    }

    void Update()
    {
        if (GameManager.Instance.IsReady())
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetButton("Fire1"))
                TouchSkin(Input.mousePosition);
#else
            for (int i = 0; i < Input.touchCount && i < touches.Length; i++)
            {
                touches[i] = Input.GetTouch(i);

                TouchSkin(touches[i].position);
            }
#endif
        }
    }

    private void TouchSkin(Vector3 screenPosition)
    {
        screenPosition.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 clickPos = Camera.main.ScreenToWorldPoint(screenPosition);
        clickPos.z = transform.position.z;

        for (int j = 0; j < dermisLayers.Length; j++)
        {
            List<Grid.Mass> massesToLink = new List<Grid.Mass>();
            dermisLayers[j].GetMassesCloseTo(massesToLink, clickPos, touchRadius);
            if (massesToLink.Count > 0)
            {
                foreach (Grid.Mass m in massesToLink)
                {
                    m.AddForce((clickPos - m.Position) * 4);
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
