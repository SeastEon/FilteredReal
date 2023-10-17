using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { Noisemap, ColorMap, Mesh };
    public DrawMode drawMode;

    public Noise.Normailise normalisemode;
    public const int MapChunkSize = 241; //holds the size of the chunks of our map
    [Range(0, 6)] public int EditorlevelofDetail; //defines how many verticies are being drawn 
    public float noiseScale; //scales up the height of our map

    public bool Autoupdate; //allows the user to turn on or off the auto generating map

    public int octaves; 
    [Range(0, 1)] public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public float MeshHeightMuliplier;
    public AnimationCurve MeshHeightCurve;

    public TerrainType[] biomes;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> MeshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    public void DrawMapInEditor() {
        MapData mapdata = GenerateMapData(Vector2.zero);
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.Noisemap) { display.DrawTexutre(TextureGenerator.TexturefromHeightMap(mapdata.heightMap)); }
        else if (drawMode == DrawMode.ColorMap) { display.DrawTexutre(TextureGenerator.TextureFromColormap(mapdata.Colormap, MapChunkSize, MapChunkSize)); }
        else if (drawMode == DrawMode.Mesh) { display.DrawMesh(MeshGenerator.generateTerrianMesh(mapdata.heightMap, MeshHeightMuliplier, MeshHeightCurve, EditorlevelofDetail), TextureGenerator.TextureFromColormap(mapdata.Colormap, MapChunkSize, MapChunkSize)); }
    }

    //Multi-Threading 
    public void RequestMapData(Vector2 center, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate {
            MapDataThread(center, callback);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 center, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(center);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate {
            MeshDataThread(mapData, lod, callback);
        };

        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.generateTerrianMesh(mapData.heightMap, MeshHeightMuliplier, MeshHeightCurve, lod);
        lock (MeshDataThreadInfoQueue)
        {
            MeshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    void Update(){
        if(mapDataThreadInfoQueue.Count > 0){
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++){
                MapThreadInfo<MapData> threadinfo = mapDataThreadInfoQueue.Dequeue();
                threadinfo.callback(threadinfo.parameter);
            }
        }

        if(MeshDataThreadInfoQueue.Count > 0) {
            for (int i = 0; i < MeshDataThreadInfoQueue.Count; i++){
                MapThreadInfo<MeshData> threadinfo = MeshDataThreadInfoQueue.Dequeue();
                threadinfo.callback(threadinfo.parameter);
            }
        }
    }
    MapData GenerateMapData(Vector2 center)
    {
        float[,] NoiseMap = Noise.GenerateNoise(MapChunkSize, MapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, center + offset, normalisemode);
        Color[] colorMap = new Color[MapChunkSize * MapChunkSize];

        for (int y = 0; y < MapChunkSize; y++) {
            for (int x = 0; x < MapChunkSize; x++) {
                float currentHeight = NoiseMap[x, y];
                for (int i = 0; i < biomes.Length; i++) {
                    if (currentHeight >= biomes[i].height){
                        colorMap[y * MapChunkSize + x] = biomes[i].color;   
                    } else {
                        break;
                    }
                }
            }
        }
        return new MapData(NoiseMap, colorMap); //returns the map data we need to create our map
    }


    private void OnValidate() {
        if (lacunarity < 1) { lacunarity = 1; }
        if (octaves < 0) { octaves = 0; }
    }

    struct MapThreadInfo<T> {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter) {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}

[System.Serializable]
public struct TerrainType{
    public string name;
    public float height;
    public Color color;
}

public struct MapData{ //holds the height map and the colormap 
    public readonly float[,] heightMap; 
    public readonly Color[] Colormap;

    public MapData(float[,] heightMap, Color[] Colormap) { //intililises the heightmap and colormap to what we have caluculated
        this.heightMap = heightMap;
        this.Colormap = Colormap;
    }
}