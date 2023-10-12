using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { Noisemap, ColorMap, Mesh};
    public DrawMode drawMode;

    public const int MapChunkSize = 241;
    [Range(0,6)] public int levelofDetail;
    public float noiseScale;
    public bool Autoupdate;

    public int octaves;
    [Range( 0,1)] public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public float MeshHeightMuliplier;
    public AnimationCurve MeshHeightCurve;

    public TerrainType[] biomes;

    public void GenerateMap(){
        float[,] NoiseMap = Noise.GenerateNoise(MapChunkSize, MapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);

        Color[] colorMap = new Color[MapChunkSize * MapChunkSize];

        for (int y = 0; y < MapChunkSize; y++){
            for (int x = 0; x < MapChunkSize; x++){
                float currentHeight = NoiseMap[x, y];
                for(int i = 0; i < biomes.Length; i++){
                    if(currentHeight <= biomes[i].height) {
                        colorMap[y * MapChunkSize + x] = biomes[i].color;
                        break; 
                    }
                }
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if(drawMode == DrawMode.Noisemap) { display.DrawTexutre(TextureGenerator.TexturefromHeightMap(NoiseMap)); }
        else if(drawMode == DrawMode.ColorMap ){ display.DrawTexutre(TextureGenerator.TextureFromColormap(colorMap, MapChunkSize, MapChunkSize)); }
        else if( drawMode == DrawMode.Mesh) { display.DrawMesh(MeshGenerator.generateTerrianMesh(NoiseMap, MeshHeightMuliplier, MeshHeightCurve, levelofDetail), TextureGenerator.TextureFromColormap(colorMap, MapChunkSize, MapChunkSize)); }
    }

    private void OnValidate()
    {

        if (lacunarity < 1) { lacunarity = 1; }
        if (octaves < 0) { octaves = 0; }
    }  
}

[System.Serializable]
public struct TerrainType{
    public string name;
    public float height;
    public Color color;
}
