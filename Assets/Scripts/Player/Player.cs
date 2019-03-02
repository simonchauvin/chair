using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Grid grid;
    private List<Grid.Mass> massesToMove = new List<Grid.Mass>();


    public void Init(Grid grid)
    {
        this.grid = grid;
    }

    void Update()
    {
        if (GameManager.Instance.IsReady())
        {
            if (Input.GetButton("Fire1"))
            {
                List<Grid.Mass> massesToLink = new List<Grid.Mass>();
                Vector3 clickPosScreen = Input.mousePosition;
                clickPosScreen.z = Mathf.Abs(Camera.main.transform.position.z);
                Vector3 clickPos = Camera.main.ScreenToWorldPoint(clickPosScreen);
                clickPos.z = transform.position.z;
                grid.GetMassesCloseTo(massesToLink, clickPos, 2.0f);
                foreach (Grid.Mass m in massesToLink)
                    m.AddForce((clickPos - m.Position) * 4);
            }

            if (Input.GetButtonDown("Fire2"))
            {

                Vector3 clickPosScreen = Input.mousePosition;
                clickPosScreen.z = Mathf.Abs(Camera.main.transform.position.z);
                Vector3 clickPos = Camera.main.ScreenToWorldPoint(clickPosScreen);
                clickPos.z = transform.position.z;
                grid.GetMassesCloseTo(massesToMove, clickPos, 0.5f);
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
                grid.GetSpringsCloseTo(springsToKill, clickPos, 0.2f);

                foreach (Grid.Spring s in springsToKill)
                {
                    s.Detach();
                }
            }
        }
    }
}
