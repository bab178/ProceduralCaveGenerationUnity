using UnityEngine;
using System.Collections;
using System;

public class MapGenerator : MonoBehaviour
{
    public int Width;
    public int Height;

    public string Seed;
    public bool UseRandomSeed;

    [Range(0,100)]
    public int RandomFillPercent;

    [Range(0, 50)]
    public int SmoothMapCount;

    private bool[,] map;

    private enum wallType { Space, Wall }


    void Start()
    {
        GenerateMap();
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            GenerateMap();
        }
    }

    private void GenerateMap()
    {
        map = new bool[Width, Height];
        RandomFillMap(RandomFillPercent);
        SmoothMap();
    }

    private void RandomFillMap(float percent)
    {
        if(UseRandomSeed)
        {
            Seed = Time.time.ToString();
        }

        System.Random pseudoRandom = new System.Random(Seed.GetHashCode());
        
        for(int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if(x == 0 || x == Width - 1 || y == 0 || y == Height - 1)
                {
                    map[x, y] = true;
                }
                else
                {
                    map[x, y] = pseudoRandom.Next(0, 100) < RandomFillPercent;
                }
            }
        }
    }

    void SmoothMap()
    {
        for (int i = 0; i < SmoothMapCount; i++)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    int count = GetSurroundingWallCount(x, y);

                    if (count > 4)
                    {
                        map[x, y] = true;
                    }
                    else if(count < 4)
                    {
                        map[x, y] = false;
                    }
                    // Skip == 4
                }
            }
        }
    }

    int GetSurroundingWallCount(int x, int y)
    {
        int wallCount = 0;

        for(int neighborX = x - 1; neighborX <= x + 1; neighborX++)
        {
            for (int neighborY = y - 1; neighborY <= y + 1; neighborY++)
            {
                if(neighborX >= 0 && neighborX < Width && neighborY >= 0 && neighborY < Height)
                {
                    if (neighborX != x || neighborY != y)
                    {
                        wallCount += (map[neighborX, neighborY]) ? 1 : 0;
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }
        return wallCount;
    }

    void OnDrawGizmos()
    {
        if(map != null)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Gizmos.color = (map[x, y] == 1) ? Color.black : Color.white;
                    Vector3 pos = new Vector3(-Width / 2 + x + 0.5f, 0, -Height / 2 + y + 0.5f);
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }
        }
    }

  
}
