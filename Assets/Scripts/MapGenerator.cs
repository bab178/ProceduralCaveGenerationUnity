using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    public int Width;
    public int Height;

    public string Seed;
    public bool UseRandomSeed;

    public int SmallestRoomSize;
    public int SmallestWallSize;

    [Range(1,100)]
    public float WallHeight;

    [Range(1, 100)]
    public float TileSize;

    [Range(0,100)]
    public int RandomFillPercent;

    [Range(0, 50)]
    public int SmoothMapCount;

    private int[,] map;

    private enum wallType { Space, Wall }

    private MeshGenerator meshGen;

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
        map = new int[Width, Height];
        RandomFillMap(RandomFillPercent);
        SmoothMap();

        ProcessMap();

        int borderSize = 1;
        int[,] borderedMap = new int[Width + borderSize * 2, Height + borderSize * 2];

        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                if (x >= borderSize && x < Width + borderSize && y >= borderSize && y < Height + borderSize)
                {
                    borderedMap[x, y] = map[x - borderSize, y - borderSize];
                }
                else
                {
                    borderedMap[x, y] = (int) wallType.Wall;
                }
            }
        }

        meshGen = GetComponent<MeshGenerator>();
        meshGen.GenerateMesh(borderedMap, TileSize, WallHeight);
    }

    void ProcessMap()
    {
        List<List<Coord>> wallRegions = GetRegions((int) wallType.Wall);
        ProcessRegion((int)wallType.Space, wallRegions, SmallestWallSize);

        List<List<Coord>> roomRegions = GetRegions((int) wallType.Space);
        ProcessRegion((int)wallType.Wall, roomRegions, SmallestRoomSize);
    }

    List<List<Coord>> GetRegions(int tileType)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[Width, Height];

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                {
                    List<Coord> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach (Coord tile in newRegion)
                    {
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }

        return regions;
    }

    List<Coord> GetRegionTiles(int startX, int startY)
    {
        List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[Width, Height];
        int tileType = map[startX, startY];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (IsInMapRange(x, y) && (y == tile.tileY || x == tile.tileX))
                    {
                        if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Coord(x, y));
                        }
                    }
                }
            }
        }

        return tiles;
    }

    void ProcessRegion(int tileType, List<List<Coord>> regions, int threshold)
    {
        foreach (List<Coord> region in regions)
        {
            if (region.Count < threshold)
            {
                foreach (Coord tile in region)
                {
                    map[tile.tileX, tile.tileY] = tileType;
                }
            }
        }
    }

    bool IsInMapRange(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    struct Coord
    {
        public int tileX;
        public int tileY;

        public Coord(int x, int y)
        {
            tileX = x;
            tileY = y;
        }
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
                    map[x, y] = (int) wallType.Wall;
                }
                else
                {
                    map[x, y] = (pseudoRandom.Next(0, 100) < RandomFillPercent) ? (int)wallType.Wall : (int)wallType.Space;
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
                        map[x, y] = (int)wallType.Wall;
                    }
                    else if(count < 4)
                    {
                        map[x, y] = (int)wallType.Space;
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
                        wallCount += (map[neighborX, neighborY] != (int) wallType.Space)? 1 : 0; // allows for more wall types
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
}
