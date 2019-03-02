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

    public void AddToBucket(float x, float y, T truc)
    {
        int bX = Mathf.FloorToInt(x / CellWidth);
        int bY = Mathf.FloorToInt(y / CellHeight);

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

    public void GetNeighbours(Bucket Neighbours, float xPos, float yPos)
    {

        Neighbours.Clear();
        int bX = Mathf.FloorToInt(xPos / CellWidth);
        int bY = Mathf.FloorToInt(yPos / CellHeight);

        int bXStart = System.Math.Max(0, bX - 1);
        int bYStart = System.Math.Max(0, bY - 1);
        int bXEnd = System.Math.Min(XCount-1, bX + 1);
        int bYEnd = System.Math.Min(YCount-1, bY + 1);


        for (int y = bYStart; y <= bYEnd;y++)
        {
            for (int x = bXStart; x <= bXEnd; x++)
            {
                Neighbours.Add(Cells[x + y * XCount]);
            }
        }
    }

}
