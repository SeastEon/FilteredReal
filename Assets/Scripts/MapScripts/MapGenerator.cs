using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using UnityEngine;

public class MapGenerator : MonoBehaviour {
    public enum DrawMode{NoiseMap, ColorMap, Mesh, FallOffMap }
    public DrawMode drawMode;

    public Noise.NormalizeMode normalizeMode;
    public const int MapChuckSize = 239;
    [Range(0,6)]
    public int EditorLevelOfDetail;

    public float noiseScale;

    public bool AutoUpdate;

    public bool UseFallOffMap;

    public int seed;

    public int octaves;
    [Range (0, 1)]
    public float persistance;
    public float lacunarity;

    public Vector2 offset;

    public float MeshHeightMulitplier;
    public AnimationCurve MeshHeightCurve;

    float[,] FallOffMap;
    public TerrainType[] regions;

    Queue<MapThreadInfo<Mapdata>> mapdataThreadInfoQueue = new Queue<MapThreadInfo<Mapdata>>();
    Queue<MapThreadInfo<Meshdata>> meshdataThreadInfoQueue = new Queue<MapThreadInfo<Meshdata>>();

    private void Awake()
    {
        FallOffMap = FallOffGenerator.GeneratefallOffMap(MapChuckSize);
    }

    public void DrawmapInEditor() {
        Mapdata mapdata = GeneratateMapData(Vector2.zero);
        MapDisplay display = FindAnyObjectByType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap){
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapdata.heightMap));
        } else if (drawMode == DrawMode.ColorMap){
            display.DrawTexture(TextureGenerator.TextureFromColourMap(mapdata.colorMap, MapChuckSize, MapChuckSize));
        } else if (drawMode == DrawMode.Mesh)  {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapdata.heightMap, MeshHeightMulitplier, MeshHeightCurve, EditorLevelOfDetail), TextureGenerator.TextureFromColourMap(mapdata.colorMap, MapChuckSize, MapChuckSize));
        } else if (drawMode == DrawMode.FallOffMap)   {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(FallOffGenerator.GeneratefallOffMap(MapChuckSize)));
        }
    }

    public void RequestMapdata(Vector2 centre, Action<Mapdata> callback) {
        ThreadStart threadstart = delegate { MapdataThread(centre, callback); };
        new Thread(threadstart).Start();
    }

    void MapdataThread(Vector2 centre, Action<Mapdata> callback){
        Mapdata mapdata = GeneratateMapData(centre);
        lock (mapdataThreadInfoQueue) {
            mapdataThreadInfoQueue.Enqueue(new MapThreadInfo<Mapdata>(callback, mapdata));
        }
    }

    public void RequestMeshData(Mapdata mapData,int lod, Action<Meshdata> callback) {
        ThreadStart threadStart = delegate {MeshdataThread(mapData,lod, callback); };
        new Thread(threadStart).Start();
    }
    void MeshdataThread(Mapdata mapdata, int lod, Action<Meshdata> callback) {
        Meshdata meshdata = MeshGenerator.GenerateTerrainMesh(mapdata.heightMap, MeshHeightMulitplier, MeshHeightCurve, lod);
        lock (meshdataThreadInfoQueue)  {
            meshdataThreadInfoQueue.Enqueue(new MapThreadInfo<Meshdata>(callback, meshdata));
        }
    }


    private void Update() {
        if(mapdataThreadInfoQueue.Count > 0) {
            for (int i = 0; i < mapdataThreadInfoQueue.Count; i++) {
                MapThreadInfo<Mapdata> threadInfo = mapdataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if(meshdataThreadInfoQueue.Count > 0){
            for (int i = 0;i < meshdataThreadInfoQueue.Count;i++) {
                MapThreadInfo<Meshdata> threadInfo = meshdataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    public Mapdata GeneratateMapData(Vector2 centre) {
        float[,] noiseMap = Noise.GenerateNoiseMap(MapChuckSize+2 , MapChuckSize +2 , seed, noiseScale, octaves, persistance, lacunarity, centre + offset, normalizeMode);
        Color[] colorMap = new Color[MapChuckSize * MapChuckSize];

        for (int y = 0;  y < MapChuckSize; y++) { 
            for(int x = 0; x < MapChuckSize; x++) {
                if (UseFallOffMap) { noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - FallOffMap[x, y]); }
                float CurrHeight = noiseMap[x, y];
                for(int i = 0; i < regions.Length; i++) {
                    if(CurrHeight >= regions[i].height) {
                        colorMap[y * MapChuckSize + x] = regions[i].color;
                    } else { break; }
                }
            }
        }
        return new Mapdata(noiseMap, colorMap);

    }

    private void OnValidate() {
        if(lacunarity < 1){ lacunarity = 1; }
        if(octaves < 0) { octaves = 0;}
        FallOffMap = FallOffGenerator.GeneratefallOffMap(MapChuckSize);
    }

    struct MapThreadInfo <T>{
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter) {
            this.callback = callback;
            this.parameter = parameter;
        }
    }

    [System.Serializable]
    public struct TerrainType {
        public string name;
        public float height;
        public Color color;
    }
}

public struct Mapdata
{
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;

    public Mapdata(float[,] heightMap, Color[] colorMap) {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}