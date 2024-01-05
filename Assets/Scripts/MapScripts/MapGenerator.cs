using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using UnityEngine;
using Unity.VisualScripting;

public class MapGenerator : MonoBehaviour {
    public enum DrawMode{NoiseMap, ColorMap, Mesh, FallOffMap }
    public DrawMode drawMode;

    public NoiseData NoiseData;
    public TerrainData terrainData;
    public TextureData textureData;


    public Material TerrainMaterial;
    public bool AutoUpdate;

    [Range(0,6)]
    public int EditorLevelOfDetail;

    float[,] FallOffMap;
    public TerrainType[] regions;

    Queue<MapThreadInfo<Mapdata>> mapdataThreadInfoQueue = new Queue<MapThreadInfo<Mapdata>>();
    Queue<MapThreadInfo<Meshdata>> meshdataThreadInfoQueue = new Queue<MapThreadInfo<Meshdata>>();

    void OnValuesUpdated() {if (!Application.isPlaying) {DrawmapInEditor(); }}
    void OnTextureValuesUpdated() { textureData.ApplyToMaterial(TerrainMaterial);}
    public int MapChuckSize {
        get {
            if (terrainData.UseFlatShading) { return 95; }
            else { return 239; }
        }
    }

    public void DrawmapInEditor() {
        Mapdata mapdata = GeneratateMapData(Vector2.zero);
        MapDisplay display = FindAnyObjectByType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap){
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapdata.heightMap));
        } else if (drawMode == DrawMode.ColorMap){
            display.DrawTexture(TextureGenerator.TextureFromColourMap(mapdata.colorMap, MapChuckSize, MapChuckSize));
        } else if (drawMode == DrawMode.Mesh)  {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapdata.heightMap, terrainData.MeshHeightMulitplier, terrainData.MeshHeightCurve, EditorLevelOfDetail, terrainData.UseFlatShading), TextureGenerator.TextureFromColourMap(mapdata.colorMap, MapChuckSize, MapChuckSize));
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
        Meshdata meshdata = MeshGenerator.GenerateTerrainMesh(mapdata.heightMap, terrainData.MeshHeightMulitplier, terrainData.MeshHeightCurve, lod, terrainData.UseFlatShading);
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
        float[,] noiseMap = Noise.GenerateNoiseMap(MapChuckSize+2 , MapChuckSize +2 , NoiseData.seed, NoiseData.noiseScale, NoiseData.octaves, NoiseData.persistance, NoiseData.lacunarity, centre + NoiseData.offset, NoiseData.normalizeMode);
        Color[] colorMap = new Color[MapChuckSize * MapChuckSize];

        if(terrainData.UseFallOffMap){
            if(FallOffMap == null) { FallOffMap = FallOffGenerator.GeneratefallOffMap(MapChuckSize +2); }
        }

        for (int y = 0;  y < MapChuckSize + 2 ; y++) { 
            for(int x = 0; x < MapChuckSize + 2; x++) {
                if (terrainData.UseFallOffMap) { noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - FallOffMap[x, y]); }

                //only uses the +2 for the fall off map
                if(x < MapChuckSize && y < MapChuckSize) {
                    float CurrHeight = noiseMap[x, y];
                    for(int i = 0; i < regions.Length; i++) {
                        if(CurrHeight >= regions[i].height) {
                            colorMap[y * MapChuckSize + x] = regions[i].color;
                        } else { break; }
                    }
                }
            }
        }
        return new Mapdata(noiseMap, colorMap);

    }

    private void OnValidate() {
        if(terrainData != null) {
            terrainData.OnValuesupdated -= OnValuesUpdated;
            terrainData.OnValuesupdated += OnValuesUpdated;
        }
        if (NoiseData != null) {
            NoiseData.OnValuesupdated -= OnValuesUpdated;
            NoiseData.OnValuesupdated += OnValuesUpdated;
        }
        if(textureData != null) {
            NoiseData.OnValuesupdated -= OnTextureValuesUpdated;
            NoiseData.OnValuesupdated += OnTextureValuesUpdated;
        }
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

public struct Mapdata{
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;

    public Mapdata(float[,] heightMap, Color[] colorMap) {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}