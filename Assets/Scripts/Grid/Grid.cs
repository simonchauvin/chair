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

    public float BaseMass = 2;
    public float BaseElasticity = 3;
    public float BaseSpringMinLength = 0.4f; //Pour une base de 1
    public float BaseSpringMaxLength = 1.5f; //Pour une base de 1
    public float BaseBoostSpringForce = 20; //Force qui augmente aux limites du ressort
    public float BaseMaxSpringForce = 7; //Force max du ressort
    public float TemporalSmooth = 0.6f;
    public float SpeedFatigueUp = 0.6f;
    public float SpeedFatigueDown = 0.3f;

    private Transform thisTransform;

    List<Mass> Masses = new List<Mass>();
    List<Spring> Springs = new List<Spring>();

    private Buckets<Mass> Buckets = new Buckets<Mass>();

    public void Init()
    {
        thisTransform = transform;

        Masses.Clear();
        Springs.Clear();

        for (int x = 0; x < GridInitWidth; x++)
        {
            for (int y = 0; y < GridInitHeight; y++)
            {
                Mass m = new Mass(this);
                m.Position = thisTransform.position + new Vector3(CellSize * x, CellSize * y, 0);

                if (x == 0 || y == 0 || x == GridInitWidth - 1 || y == GridInitHeight - 1)
                    m.Fixed = true; 

                Masses.Add(m);
            }
        }

        int nbCellX = GridInitWidth ;
        int nbCellY = GridInitHeight;
        Buckets.Create(nbCellX, nbCellY, (GridInitWidth * CellSize) / nbCellX, (GridInitHeight * CellSize) / nbCellY);
        PutMassesInBuckets();

        Buckets<Mass>.Bucket massesToLink = new Buckets<Mass>.Bucket();
        for (int x = 0; x < GridInitWidth; x++)
        {
            for (int y = 0; y < GridInitHeight; y++)
            {
                Vector3 position = thisTransform.position + new Vector3(CellSize * x, CellSize * y, 0);

                
                GetMassesCloseTo(massesToLink, position, 0.01f);
                Mass m = massesToLink.Trucs[0];
                //m.AttachToNeighbor(Mathf.Sqrt(2 * (this.CellSize * this.CellSize)) * 1.01f);
                m.AttachToNeighbor(this.CellSize * 1.01f);
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
        private float SumFatigue = 0;
        private float TemporalSmooth = 0.1f;
        private int NbSpringsAttachedMax = 0;

        public Mass(Grid g)
        {
            M = g.BaseMass;
            G = g;
            TemporalSmooth = g.TemporalSmooth;
        }
        public bool ToBeKilled()
        {
            return KillMe;
        }

        public void AddForce(Vector3 force)
        {
            Forces += force;
        }

        public void AddFatigue(float fatigue)
        {
            SumFatigue += fatigue;
        }

        public float GetFatigue()
        {
            return (SumFatigue  + (NbSpringsAttachedMax - NbSpringsAttached))/ NbSpringsAttachedMax;
        }

        public void PreUpdate()
        {
            if (NbSpringsAttachedMax < NbSpringsAttached)
                NbSpringsAttachedMax = NbSpringsAttached;
            SumFatigue = 0;
        }

        public void Update(float deltaTime)
        {
            if (G.DebugObject != null && G.ShowDebugObjects && DebugObject == null)
            {
                DebugObject = GameObject.Instantiate<Transform>(G.DebugObject);
                DebugObject.localScale = Vector3.one * G.CellSize / 3.0f;
            }

            if (Fixed)
                return;

            Forces /= M;

            Forces = Vector3.Lerp(PrevForce,Forces,Mathf.Pow(1-TemporalSmooth,Time.deltaTime));
            PrevForce = Forces;

            //Deplacement en vitesse limitée
            if ((Forces).sqrMagnitude > 3)
                Forces = Forces.normalized * 3;

            Position += Forces * deltaTime;
            Forces = Vector3.zero;

            if (DebugObject)
            {
                DebugObject.position = Position;
                //DebugObject.GetComponent<MeshRenderer>().enabled = G.ShowDebugObjects;
            }    
                    
            

            if (NbSpringsAttached <= 0)
            {
                if (DebugObject)
                {
                    Destroy(DebugObject.gameObject);
                }
                KillMe = true;
            }
                          
        }

        public void AttachToNeighbor(float distance)
        {
            Buckets<Grid.Mass>.Bucket massesToLink = new Buckets<Grid.Mass>.Bucket();
            G.GetMassesCloseTo(massesToLink, Position, distance, true);

            for (int i=0; i< massesToLink.Count; i++)
            {
                Mass m = massesToLink.Trucs[i];
                G.AddSpring(this, m);
            }
        }

      
    }

    public class Spring
    {
        public float K = 1;
        public Mass A;
        public Mass B;

        private float Length = 1;
        private float PrevLengthSq = 1;
        private float MinLength = 0.1f;
        private float MaxLength = 1.9f;
        private bool KillMe = false;
        private Grid G;
        private float Force;
        private float PrevForce;
        private float Fatigue;
        private float SpeedFatigueUp = 0.1f;
        private float SpeedFatigueDown = 0.05f;
        private float Adaptivity = 0;


        public Spring(Grid g)
        {
            G = g;
            K = g.BaseElasticity;
            SpeedFatigueUp = 0.3f * g.SpeedFatigueUp;
            SpeedFatigueDown = 0.3f * g.SpeedFatigueDown;
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

            //On regarde si la length a changé
            Vector3 dir = B.Position - A.Position;
            float lengthCurSq = dir.sqrMagnitude;
            if (Mathf.Abs(PrevLengthSq - lengthCurSq) > 0.0001f )
            {
                
                //On applique les forces aux masses

                float lengthCur = Mathf.Sqrt(lengthCurSq);
                //On adapte la length
                //SetLength(Mathf.Lerp(Length, lengthCur, Adaptivity));

                Force = (lengthCur - Length) * K;

                //On booste la force quand on se rapproche des points limites
                float distToMin = Mathf.Max(0, lengthCur - MinLength) / (Length - MinLength);
                float distToMax = Mathf.Max(0, MaxLength - lengthCur) / (MaxLength - Length);
                float boost = Mathf.Min(distToMin, distToMax);

                if (boost <= 1 / G.BaseBoostSpringForce)
                    boost = 1 / G.BaseBoostSpringForce;
                Force *= 1 / boost;

                if (Force > G.BaseMaxSpringForce)
                    Force = G.BaseMaxSpringForce;

                //if (Force < 0.1f)
                //  Force *= Force;


                if (Mathf.Abs(Force - PrevForce) < 0.9f)
                {
                    float boostFatigue = SpeedFatigueDown;
                    if ((Force - PrevForce) > 0 && Mathf.Abs(Force - PrevForce) > 0.5f)
                        boostFatigue = SpeedFatigueUp;
                    Fatigue += (Force - PrevForce) * boostFatigue;
                }
                PrevForce = Force;

                /*if (Mathf.Abs(Force) > 0.1f)
                {   
                    Fatigue += Mathf.Abs(Force* Force)*0.1f * SpeedFatigueUp * Time.deltaTime;
                }
                Fatigue -= Time.deltaTime * SpeedFatigueDown;

                Fatigue = Mathf.Max(Fatigue, 0);*/


                //Kill
                if (Fatigue > 1)
                    Detach();


                //On applique la force
                Vector3 dirNorm = dir / lengthCur;
                A.AddForce(dirNorm * Force);
                B.AddForce(dirNorm * (-Force));
                A.AddFatigue(Fatigue);
                B.AddFatigue(Fatigue);

                PrevLengthSq = lengthCurSq;
            }

            
        }

        public float distanceToMe(Vector3 v)
        {

            Vector3 AB = B.Position - A.Position;
            Vector3 BV = (v - B.Position);
            Vector3 AV = (v - A.Position);
            AB.z = 0;
            BV.z = 0;
            AV.z = 0;
            if (Vector3.Dot(AB, AV) <= 0)
                return float.PositiveInfinity; //AV.magnitude;
            if (Vector3.Dot(AB, BV) >= 0)
                return float.PositiveInfinity; //BV.magnitude

            return (Vector3.Cross(AB, AV).magnitude / AB.magnitude);
        }

        public float GetFatigue()
        {
            return Fatigue;
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

    private void PutMassesInBuckets()
    {
        Buckets.Clear();
        foreach (Mass m in Masses)
        {
            Buckets.AddToBucket(m.Position - thisTransform.position, m);
        }
    }

    Buckets<Grid.Mass>.Bucket tempNeighbours = new Buckets<Grid.Mass>.Bucket(); 
    /*public void GetMassesCloseTo(List<Mass> found, Vector3 point, float distance, bool reset = true)
    {
        if (reset)
            found.Clear();
        Vector3 posLocale = point - thisTransform.position;
        Buckets.GetNeighbours(tempNeighbours,posLocale.x, posLocale.y, distance);
        for (int i=0;i< tempNeighbours.Count;i++)
        {
            Mass m = tempNeighbours.Trucs[i];
            if ((m.Position - point).sqrMagnitude < distance * distance)
                found.Add(m);
        }

    }*/

    //En mode juste Bucket
    public void GetMassesCloseTo(Buckets<Grid.Mass>.Bucket found, Vector3 point, float distance, bool reset = true)
    {
        float sqrDist = distance * distance;
        if (reset)
            found.Clear();
        Vector3 posLocale = point - thisTransform.position;
        Buckets.GetNeighbours(tempNeighbours, posLocale.x, posLocale.y, distance);
        for (int i = 0; i < tempNeighbours.Count; i++)
        {
            Mass m = tempNeighbours.Trucs[i];
            if ((m.Position - point).sqrMagnitude < sqrDist)
                found.Add(m);
        }

    }

 

    public Mass GetClosestMassTo(Vector3 point, float distance, bool reset = true)
    {
        Vector3 posLocale = point - thisTransform.position;
        Buckets.GetNeighbours(tempNeighbours, posLocale.x, posLocale.y, distance);
        float distanceMin = distance * distance, distSq;
        Mass bestM = null, m;
        for (int i = 0; i < tempNeighbours.Count; i++)
        {
            m = tempNeighbours.Trucs[i];
            distSq = (m.Position - point).sqrMagnitude;
            if (distSq < distanceMin)
            {
                distanceMin = distSq;
                bestM = m;
            }
        }
        return bestM;
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
    void Update()
    {
        foreach(Mass m in Masses)
            m.PreUpdate();

        foreach (Spring s in Springs)
        {
            s.Update();
        }

        if(Springs.RemoveAll(WantsToDie) > 0)
            SoundController.Instance.PlayRipSound();

        Masses.RemoveAll(WantsToDie);

        foreach (Mass m in Masses)
            m.Update(Time.deltaTime);

        PutMassesInBuckets();
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
                //Gizmos.color = Color.Lerp(Color.white, Color.red, s.GetForcesStrength());
                Gizmos.color = Color.Lerp(Color.white, Color.red, s.GetFatigue());
                Gizmos.DrawLine(s.A.Position, s.B.Position);
            }
        }
        
    }


}
