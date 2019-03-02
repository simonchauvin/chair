using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// On update d'abbord les springs qui appliquent des forces aux masses
/// On update ensuite les masses pour qu'elles bougent
/// </summary>
public class Grid : MonoBehaviour
{

    public class Mass
    {
        public Vector3 Position;
        public float M = 1;
        public Vector3 Forces;
        public void AddForce(Vector3 force)
        {
            Forces += force;
        }

        public void Update(float deltaTime)
        {
            Position += Forces * deltaTime;
            Forces = new Vector3();
        }
    }

    public class Spring
    {
        public float K = 1;
        public float Length = 1;
        public float MinLength = 0.1f;
        public float MaxLength = 1.9f;
        public bool KillMe = false;
        public Mass A;
        public Mass B;


        public void Update()
        {
            //On applique les forces aux masses
            Vector3 dir = B.Position - A.Position;
            float lengthCur = dir.magnitude;
            float force = (lengthCur - Length) * K;

            //On booste la force quand on se rapproche des points limites
            float distToMin = Mathf.Max(0, lengthCur - MinLength) / (Length- MinLength);
            float distToMax = Mathf.Max(0, MaxLength - lengthCur) / (MaxLength - Length);
            float boost = Mathf.Min(distToMin, distToMax);

            if (boost <= 0.1f)
                boost = 0.1f;
            force *= 1/boost;

            if (force > 10)
            {
                force = 10;
            }

            //Kill
            if (lengthCur > MaxLength)
                KillMe = true;



            //On applique la force
            Vector3 dirNorm = dir.normalized;
            A.AddForce(dirNorm * force);
            B.AddForce(dirNorm *(-force));
        }
    }

    List<Mass> Masses = new List<Mass>();
    List<Spring> Springs = new List<Spring>();

    public int GridInitWidth = 20;
    public int GridInitHeight = 10;
    public float CellSize = 1;

    public void GetMassesCloseTo(List<Mass> found, Vector3 point, float distance, bool reset = true)
    {
        if (reset)
            found.Clear();
        foreach (Mass m in Masses)
        {
            if ((m.Position - point).sqrMagnitude < distance * distance)
                found.Add(m);
        }

    }

    public void GetSpringCloseTo(List<Spring> found, Vector3 point, float distance, bool reset = true)
    {
        if (reset)
            found.Clear();
        foreach (Spring m in Springs)
        {
            if ((m.Position - point).sqrMagnitude < distance * distance)
                found.Add(m);
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        for(int x=0;x< GridInitWidth; x++)
        {
            for (int y = 0; y < GridInitHeight; y++)
            {
                Mass m = new Mass();
                m.Position = transform.position + new Vector3(CellSize * x, CellSize * y, 0);
                Masses.Add(m);
            }
        }

        for (int x = 0; x < GridInitWidth; x++)
        {
            for (int y = 0; y < GridInitHeight; y++)
            {
                Vector3 position = transform.position + new Vector3(CellSize * x, CellSize * y, 0);

                List<Mass> massesToLink = new List<Mass>();
                GetMassesCloseTo(massesToLink, position, 0.1f);
                Mass m = massesToLink[0];
                GetMassesCloseTo(massesToLink, position,1.8f,true);

               
                foreach (Mass m2 in massesToLink)
                {
                    Spring s = new Spring();
                    s.A = m;
                    s.B = m2;
                    s.Length = (s.A.Position - s.B.Position).magnitude;
                    Springs.Add(s);
                }

            }
        }
    }

    List<Mass> massesToMove = new List<Mass>();
    public void Update()
    {
        if (Input.GetButton("Fire1"))
        {
            List<Mass> massesToLink = new List<Mass>();
            Vector3 clickPosScreen = Input.mousePosition;
            clickPosScreen.z = Mathf.Abs(Camera.main.transform.position.z);
            Vector3 clickPos = Camera.main.ScreenToWorldPoint(clickPosScreen);
            clickPos.z = transform.position.z;
            GetMassesCloseTo(massesToLink, clickPos, 1.0f);
            foreach(Mass m in massesToLink)
                m.AddForce((clickPos - m.Position)*2);
        }

        if (Input.GetButtonDown("Fire2"))
        {
            
            Vector3 clickPosScreen = Input.mousePosition;
            clickPosScreen.z = Mathf.Abs(Camera.main.transform.position.z);
            Vector3 clickPos = Camera.main.ScreenToWorldPoint(clickPosScreen);
            clickPos.z = transform.position.z;
            GetMassesCloseTo(massesToMove, clickPos, 0.5f);
            
        }

        if (Input.GetButton("Fire2"))
        {
            Vector3 clickPosScreen = Input.mousePosition;
            clickPosScreen.z = Mathf.Abs(Camera.main.transform.position.z);
            Vector3 clickPos = Camera.main.ScreenToWorldPoint(clickPosScreen);
            clickPos.z = transform.position.z;

            foreach (Mass m in massesToMove)
                m.AddForce((clickPos - m.Position) * 2);
        }
    }

    private static bool WantsToDie(Spring s)
    {
        return s.KillMe;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach (Spring s in Springs)
        {
            s.Update();
        }

        Springs.RemoveAll(WantsToDie);

        foreach (Mass m in Masses)
            m.Update(Time.fixedDeltaTime);
    }

    private void OnDrawGizmos()
    {
        foreach (Mass m in Masses)
        {
            Gizmos.DrawSphere(m.Position, 0.3f);
        }
        foreach (Spring s in Springs)
        {
            Gizmos.DrawLine(s.A.Position, s.B.Position);
        }
    }


}
