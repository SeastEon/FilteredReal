using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using UnityEngine;
using static Noise;
using static UnityEngine.Mesh;

public class MapGenerator : MonoBehaviour {

    public enum DrawMode{NoiseMap, ColorMap, Mesh }
    public DrawMode drawMode;
    public NormalizeMode normalizeMode;
    public const int MapChuckSize = 241;
    [Range(0,6)]
    public int LevelOfDetail;

    public float noiseScale;

    public bool AutoUpdate;

    public int seed;
    public int octaves;
    [Range (0, 1)]
    public float persistance;
    public float lacunarity;

    public Vector2 offset;

    public float MeshHeightMulitplier;
    public AnimationCurve MeshHeightCurve;

    public TerrainType[] regions;

    Queue<MapThreadInfo<Mapdata>> mapdataThreadInfoQueue = new Queue<MapThreadInfo<Mapdata>>();
    Queue<MapThreadInfo<Meshdata>> meshdataThreadInfoQueue = new Queue<MapThreadInfo<Meshdata>>();


    public void DrawmapInEditor() {
        Mapdata mapdata = GeneratateMapData();
        MapDisplay display = FindAnyObjectByType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap){
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapdata.heightMap));
        }
        else if (drawMode == DrawMode.ColorMap){
            display.DrawTexture(TextureGenerator.TextureFromColourMap(mapdata.colorMap, MapChuckSize, MapChuckSize));
        }
        else if (drawMode == DrawMode.Mesh)  {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapdata.heightMap, MeshHeightMulitplier, MeshHeightCurve, LevelOfDetail), TextureGenerator.TextureFromColourMap(mapdata.colorMap, MapChuckSize, MapChuckSize));
        }
    }

    public void RequestMapdata(Action<Mapdata> callback) {
        ThreadStart threadstart = delegate { MapdataThread(callback); };
        new Thread(threadstart).Start();
    }

    public void RequestMeshData(Mapdata mapData, Action<Meshdata> callback) {
        ThreadStart threadStart = delegate {MeshdataThread(mapData, callback); };
        new Thread(threadStart).Start();
    }
    void MeshdataThread(Mapdata mapdata, Action<Meshdata> callback) {
        Meshdata meshdata = MeshGenerator.GenerateTerrainMesh(mapdata.heightMap, MeshHeightMulitplier, MeshHeightCurve, LevelOfDetail);
        lock (meshdataThreadInfoQueue)  {
            meshdataThreadInfoQueue.Enqueue(new MapThreadInfo<Meshdata>(callback, meshdata));
        }
    }
    void MapdataThread(Action<Mapdata> callback) {
        Mapdata mapdata = GeneratateMapData();
        lock (mapdataThreadInfoQueue) {
            mapdataThreadInfoQueue.Enqueue(new MapThreadInfo<Mapdata>(callback, mapdata));
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

    public Mapdata GeneratateMapData() {
        float[,] noiseMap = Noise.GenerateNoiseMap(MapChuckSize, MapChuckSize, seed, noiseScale, octaves, persistance, lacunarity, offset, normalizeMode);

        Color[] colorMap = new Color[MapChuckSize * MapChuckSize];

        for (int y = 0;  y < MapChuckSize; y++) { 
            for(int x = 0; x < MapChuckSize; x++) {
                float CurrHeight = noiseMap[x, y];

                for(int i = 0; i < regions.Length; i++) {
                    if(CurrHeight <= regions[i].height) {
                        colorMap[y * MapChuckSize + x] = regions[i].color;
                        break;
                    }
                }
            }
        }
        return new Mapdata(noiseMap, colorMap);

    }

    private void OnValidate() {
        if(lacunarity < 1){ lacunarity = 1; }
        if(octaves < 0) { octaves = 0;}
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