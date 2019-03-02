using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshUtils
{
    public static Mesh CreateGrid(int xCount, int yCount, float size)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[(xCount + 1) * (yCount + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        for (int i = 0, y = 0; y <= yCount; y++)
        {
            for (int x = 0; x <= xCount; x++, i++)
            {
                vertices[i] = new Vector3((float)x / xCount * size, 0, (float)y / yCount * size);
                uv[i] = new Vector2((float)x / xCount, (float)y / yCount);
            }
        }
        mesh.vertices = vertices;
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
}
