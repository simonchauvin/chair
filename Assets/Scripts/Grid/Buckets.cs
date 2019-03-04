using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Buckets<T>
{
    public class Bucket
    {
        public Bucket()
        {
            Trucs = new T[300];
        }
        public T[] Trucs;
        public int Count;
        public void Create(int size)
        {
            Trucs = new T[size];
        }

        public void Add(T truc)
        {
            Trucs[Count] = truc;
            Count++;
        }
        public void Add(Bucket b)
        {
            for (int i = 0; i < b.Count; i++){
                Add(b.Trucs[i]);
            }
        }

        public void Clear()
        {
            Count = 0;
        }
    }

    private Bucket[] Cells;
    private int XCount;
    private int YCount;
    private float CellWidth;
    private float CellHeight;

    public void Create(int xCount, int yCount, float cellWidth, float cellHeight)
    {
        Cells = new Bucket[xCount * yCount];
        for (int i = 0; i < Cells.Length; i++)
            Cells[i] = new Bucket();

        CellWidth = cellWidth;
        CellHeight = cellHeight;

        XCount = xCount;
        YCount = yCount;
    }

    public void AddToBucket(Vector3 position, T truc)
    {
        int bX = Mathf.FloorToInt(position.x / CellWidth);
        int bY = Mathf.FloorToInt(position.y / CellHeight);

        bX = System.Math.Max(0, bX);
        bY = System.Math.Max(0, bY);
        bX = System.Math.Min(XCount-1, bX);
        bY = System.Math.Min(YCount-1, bY);
        Cells[bX + bY * XCount].Add(truc);
    }

    public void Clear()
    {
        for (int i = 0; i < Cells.Length; i++)
            Cells[i].Clear();
    }

    public void GetNeighbours(Bucket Neighbours, float xPos, float yPos, float distance)
    {

        Neighbours.Clear();
        int bX = Mathf.FloorToInt(xPos / CellWidth);
        int bY = Mathf.FloorToInt(yPos / CellHeight);
        int nbCellDistX = Mathf.CeilToInt(distance / CellWidth);
        int nbCellDistY = Mathf.CeilToInt(distance / CellHeight);

        int bXStart = System.Math.Max(0, bX - nbCellDistX);
        int bYStart = System.Math.Max(0, bY - nbCellDistY);
        int bXEnd = System.Math.Min(XCount-1, bX + nbCellDistX);
        int bYEnd = System.Math.Min(YCount-1, bY + nbCellDistY);
        Bucket cell;


        for (int y = bYStart; y <= bYEnd;y++)
        {
            for (int x = bXStart; x <= bXEnd; x++)
            {
                cell = Cells[x + y * XCount];
                for (int n = 0; n < cell.Count; n++)
                {
                    Neighbours.Add(cell.Trucs[n]);
                }
            }
        }
    }

}
