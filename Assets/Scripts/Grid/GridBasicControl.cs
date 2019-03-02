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
    List<Grid.Mass> massesToMove = new List<Grid.Mass>();
    public void Update()
    {
        if (Input.GetButton("Fire1"))
        {
            List<Grid.Mass> massesToLink = new List<Grid.Mass>();
            Vector3 clickPosScreen = Input.mousePosition;
            clickPosScreen.z = Mathf.Abs(Camera.main.transform.position.z);
            Vector3 clickPos = Camera.main.ScreenToWorldPoint(clickPosScreen);
            clickPos.z = transform.position.z;
            G.GetMassesCloseTo(massesToLink, clickPos, 2.0f);
            foreach (Grid.Mass m in massesToLink)
                m.AddForce((clickPos - m.Position) * 4);
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

            foreach (Grid.Mass m in massesToMove)
            {
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
