using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// On update d'abbord les springs qui appliquent des forces aux masses
/// On update ensuite les masses pour qu'elles bougent
/// </summary>
public class Grid : MonoBehaviour
{
    public int GridInitWidth = 20;
    public int GridInitHeight = 10;
    public float CellSize = 1;

    public Transform DebugObject;
    public bool ShowDebugSprings = true;
    public bool ShowDebugObjects = true;

    public float BaseMass = 1;
    public float BaseElasticity = 1;
    public float BaseSpringMinLength = 0.1f; //Pour une base de 1
    public float BaseSpringMaxLength = 1.9f; //Pour une base de 1
    public float BaseBoostSpringForce = 10; //Force qui augmente aux limites du ressort
    public float BaseMaxSpringForce = 10; //Force max du ressort

    List<Mass> Masses = new List<Mass>();
    List<Spring> Springs = new List<Spring>();

    public void Init()
    {
        Masses.Clear();
        Springs.Clear();

        for (int x = 0; x < GridInitWidth; x++)
        {
            for (int y = 0; y < GridInitHeight; y++)
            {
                Mass m = new Mass(this);
                m.Position = transform.position + new Vector3(CellSize * x, CellSize * y, 0);

                if (x == 0 || y == 0 || x == GridInitWidth - 1 || y == GridInitHeight - 1)
                    m.Fixed = true;

                Masses.Add(m);
            }
        }

        for (int x = 0; x < GridInitWidth; x++)
        {
            for (int y = 0; y < GridInitHeight; y++)
            {
                Vector3 position = transform.position + new Vector3(CellSize * x, CellSize * y, 0);

                List<Mass> massesToLink = new List<Mass>();
                GetMassesCloseTo(massesToLink, position, 0.01f);
                Mass m = massesToLink[0];
                m.AttachToNeighbor(Mathf.Sqrt(2 * (this.CellSize * this.CellSize)) * 1.01f);
            }
        }
    }


    public class Mass
    {
        public Vector3 Position;
        public float M = 1;
        public bool Fixed = false;
        

        private Vector3 Forces;
        private Vector3 PrevForce;
        private float ForceMax = 2;
        public int NbSpringsAttached = 0;
        private bool KillMe = false;
        private Grid G;
        private Transform DebugObject;

        public Mass(Grid g)
        {
            M = g.BaseMass;
            G = g;
        }
        public bool ToBeKilled()
        {
            return KillMe;
        }

        public void AddForce(Vector3 force)
        {
            Forces += force;
        }

        public void Update(float deltaTime)
        {
            if (G.DebugObject != null && DebugObject == null)
            {
                DebugObject = GameObject.Instantiate<Transform>(G.DebugObject);
                DebugObject.localScale = Vector3.one * G.CellSize / 3.0f;
            }

            if (Fixed)
                return;

            Forces /= M;

            Forces = Vector3.Lerp(PrevForce,Forces,0.1f);
            PrevForce = Forces;

            Position += Forces * deltaTime;
            Forces = new Vector3();

            if (DebugObject)
            {
                DebugObject.position = Position;
                DebugObject.GetComponent<MeshRenderer>().enabled = G.ShowDebugObjects;
            }    
                    
            

            if (NbSpringsAttached <= 0)
                KillMe = true;            
        }

        public void AttachToNeighbor(float distance)
        {
            List<Mass> massesToLink = new List<Mass>();
            G.GetMassesCloseTo(massesToLink, Position, distance, true);

            foreach (Mass m2 in massesToLink)
            {
                G.AddSpring(this, m2);
            }
        }
    }

    public class Spring
    {
        public float K = 1;
        public Mass A;
        public Mass B;

        private float Length = 1;
        private float MinLength = 0.1f;
        private float MaxLength = 1.9f;
        private bool KillMe = false;
        private Grid G;
        private float Force;

        public Spring(Grid g)
        {
            G = g;
            K = g.BaseElasticity;
        }

        public bool ToBeKilled()
        {
            return KillMe;
        }

        public void SetLength(float length)
        {
            Length = length;
            MinLength = G.BaseSpringMinLength * length;
            MaxLength = G.BaseSpringMaxLength * length;
        }
        
        public void Update()
        {
            //On applique les forces aux masses
            Vector3 dir = B.Position - A.Position;
            float lengthCur = dir.magnitude;
            Force = (lengthCur - Length) * K;

            //On booste la force quand on se rapproche des points limites
            float distToMin = Mathf.Max(0, lengthCur - MinLength) / (Length- MinLength);
            float distToMax = Mathf.Max(0, MaxLength - lengthCur) / (MaxLength - Length);
            float boost = Mathf.Min(distToMin, distToMax);

            if (boost <= 1/ G.BaseBoostSpringForce)
                boost = 1 / G.BaseBoostSpringForce;
            Force *= 1/boost;

            if (Force > G.BaseMaxSpringForce)
                Force = G.BaseMaxSpringForce;

            //Kill
            if (lengthCur > MaxLength)
                Detach();

                    
            //On applique la force
            Vector3 dirNorm = dir.normalized;
            A.AddForce(dirNorm * Force);
            B.AddForce(dirNorm *(-Force));
        }

        public float distanceToMe(Vector3 v)
        {
            Vector3 AB = B.Position - A.Position;
            Vector3 BV = (v - B.Position);
            Vector3 AV = (v - A.Position);
            if (Vector3.Dot(AB, AV) <= 0)
                return float.PositiveInfinity; //AV.magnitude;
            if (Vector3.Dot(AB, BV) >= 0)
                return float.PositiveInfinity; //BV.magnitude

            return (Vector3.Cross(AB, AV).magnitude / AB.magnitude);
        }

        public void Detach()
        {
            if (!KillMe)
            {
                A.NbSpringsAttached--;
                B.NbSpringsAttached--;
                KillMe = true;
            }
            
        }

        internal float GetForcesStrength()
        {
            return Mathf.Abs(Force);
        }
    }

   

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

    public void GetSpringsCloseTo(List<Spring> found, Vector3 point, float distance, bool reset = true)
    {
        if (reset)
            found.Clear();
        foreach (Spring s in Springs)
        {
            if (s.distanceToMe(point) < distance)
                found.Add(s);
        }

    }

    void GetSpringsBetween(List<Spring> found, Mass A, Mass B, bool reset = true)
    {
        if (reset)
            found.Clear();
        foreach (Spring s in Springs)
        {
            if (s.A == A && s.B == B || s.A == B && s.B == A)
                found.Add(s);
        }

    }

    void GetSpringsOn(List<Spring> found, Mass A, bool reset = true)
    {
        if (reset)
            found.Clear();
        foreach (Spring s in Springs)
        {
            if (s.A == A && s.B == A )
                found.Add(s);
        }

    }

    void AddSpring(Mass A, Mass B)
    {
        if (A != B)
        {
            GetSpringsBetween(listTemp, A, B);
            if (listTemp.Count == 0)
            {
                Spring s = new Spring(this);
                s.A = A;
                s.B = B;
                s.A.NbSpringsAttached++;
                s.B.NbSpringsAttached++;
                s.SetLength((s.A.Position - s.B.Position).magnitude * 0.8f);
                Springs.Add(s);
            }
        }
    }

    // Start is called before the first frame update
    List<Spring> listTemp = new List<Spring>();
    void Start()
    {
        
    }

    private static bool WantsToDie(Spring s)
    {
        return s.ToBeKilled();
    }

    private static bool WantsToDie(Mass m)
    {
        return m.ToBeKilled();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach (Spring s in Springs)
        {
            s.Update();
        }

        Springs.RemoveAll(WantsToDie);

        Masses.RemoveAll(WantsToDie);

        foreach (Mass m in Masses)
            m.Update(Time.fixedDeltaTime);
    }

    private void OnDrawGizmos()
    {
        /*foreach (Mass m in Masses)
        {
            Gizmos.DrawSphere(m.Position, this.CellSize/3.0f);

        }*/

        if (ShowDebugSprings)
        {
            foreach (Spring s in Springs)
            {
                Gizmos.color = Color.Lerp(Color.white, Color.red, s.GetForcesStrength());
                Gizmos.DrawLine(s.A.Position, s.B.Position);
            }
        }
        
    }


}
