using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBasicControl : MonoBehaviour
{
    public Grid G;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    Buckets<Grid.Mass>.Bucket massesToMove = new Buckets<Grid.Mass>.Bucket();
    Buckets<Grid.Mass>.Bucket massesToLink = new Buckets<Grid.Mass>.Bucket();

    public void Update()
    {
        if (Input.GetButton("Fire1"))
        {
            Vector3 clickPosScreen = Input.mousePosition;
            clickPosScreen.z = Mathf.Abs(Camera.main.transform.position.z);
            Vector3 clickPos = Camera.main.ScreenToWorldPoint(clickPosScreen);
            clickPos.z = transform.position.z;
            G.GetMassesCloseTo(massesToLink, clickPos, 2.0f);
            for (int i = 0; i < massesToLink.Count; i++)
            {
                Grid.Mass m = massesToMove.Trucs[i];
                m.AddForce((clickPos - m.Position) * 4);
            }
        }

        if (Input.GetButtonDown("Fire2"))
        {

            Vector3 clickPosScreen = Input.mousePosition;
            clickPosScreen.z = Mathf.Abs(Camera.main.transform.position.z);
            Vector3 clickPos = Camera.main.ScreenToWorldPoint(clickPosScreen);
            clickPos.z = transform.position.z;
            G.GetMassesCloseTo(massesToMove, clickPos, 0.5f);
        }

        if (Input.GetButton("Fire2"))
        {
            Vector3 clickPosScreen = Input.mousePosition;
            clickPosScreen.z = Mathf.Abs(Camera.main.transform.position.z);
            Vector3 clickPos = Camera.main.ScreenToWorldPoint(clickPosScreen);
            clickPos.z = transform.position.z;

            for (int i=0;i< massesToMove.Count;i++)
            {
                Grid.Mass m = massesToMove.Trucs[i];
                Vector3 force = (clickPos - m.Position) * 2;
                m.AddForce(force);
            }
        }

        if (Input.GetButtonDown("Fire3"))
        {

            Vector3 clickPosScreen = Input.mousePosition;
            clickPosScreen.z = Mathf.Abs(Camera.main.transform.position.z);
            Vector3 clickPos = Camera.main.ScreenToWorldPoint(clickPosScreen);
            clickPos.z = transform.position.z;
            List<Grid.Spring> springsToKill = new List<Grid.Spring>();
            G.GetSpringsCloseTo(springsToKill, clickPos, 0.2f);

            foreach (Grid.Spring s in springsToKill)
            {
                s.Detach();
            }
        }
    }
}
