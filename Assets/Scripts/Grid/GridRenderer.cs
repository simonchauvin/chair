using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    public Grid G;

    private MeshFilter thisMeshFilter;
    private Transform thisTransform;

    // Start is called before the first frame update
    void Start()
    {
        thisMeshFilter = GetComponent<MeshFilter>();
        thisTransform = transform;

        CreateGrid(G.GridInitWidth-1, G.GridInitHeight-1, G.CellSize);
    }

    // Update is called once per frame
    float elapsed = 0;
    void Update()
    {
        elapsed += Time.deltaTime;
        UpdateMesh();        
    }


    Vector3[] vertices;
    Color[] colors;
    int Width;
    int Height;
    float CellSize;

    public Mesh CreateGrid(int xCount, int yCount, float size)
    {
        Mesh mesh = new Mesh();

        thisMeshFilter.mesh = mesh;

        mesh.MarkDynamic();

        vertices = new Vector3[(xCount + 1) * (yCount + 1)];
        colors = new Color[(xCount + 1) * (yCount + 1)];
        Width = xCount + 1;
        Height = yCount + 1;
        CellSize = size;


        Vector2[] uv = new Vector2[vertices.Length];
        for (int i = 0, y = 0; y <= yCount; y++)
        {
            for (int x = 0; x <= xCount; x++, i++)
            {
                vertices[i] = new Vector3((float)x  * size, (float)y  * size, 0);
                uv[i] = new Vector2((float)x / xCount, (float)y / xCount);
                colors[i] = new Color(1, 0, 0, 1);
            }
        }
        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.uv = uv;

        int[] triangles = new int[xCount * yCount * 6];
        for (int ti = 0, vi = 0, y = 0; y < yCount; y++, vi++)
        {
            for (int x = 0; x < xCount; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + xCount + 1;
                triangles[ti + 5] = vi + xCount + 2;
            }
        }

        mesh.triangles = triangles;
        

        mesh.RecalculateNormals();

        
        

        return mesh;
    }

    List<Grid.Mass> masses = new List<Grid.Mass>();
    public void UpdateMesh()
    {
        Mesh mesh = thisMeshFilter.mesh;

        int i = 0;
        Vector3 basePos = new Vector3(),
            newPos;
        Grid.Mass m;
        
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                basePos.x = x * CellSize;
                basePos.y = y * CellSize;
                basePos.z = vertices[i].z;

                //Recup le plus proche
                m = G.GetClosestMassTo( basePos, CellSize*1.1f);

                colors[i].r = 1.0f;
                if (m != null)
                {
                    newPos = m.Position - thisTransform.position;
                    //dist = Mathf.Max(0.01f, dist);
                    
                    colors[i].b = m.GetFatigue()/10;
                    colors[i].r = ((basePos - newPos).magnitude / CellSize)*1.5f;

                    basePos = newPos;
                }
                else
                {
                    colors[i].r = 1;
                    colors[i].b = 0;
                }

                

                // basePos.z = vertices[i].z;
                vertices[i] = basePos;
                
                i++;
            }
        }

        mesh.MarkDynamic();
        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.RecalculateNormals();

    }
}
