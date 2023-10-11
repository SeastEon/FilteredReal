using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { Noisemap, ColorMap};
    public DrawMode drawMode;

    public int mapwidth;
    public int mapHeight;

    public float noiseScale;

    public bool Autoupdate;

    public int octaves;
    [Range( 0,1)] public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public TerrainType[] biomes;

    public void GenerateMap(){
        float[,] NoiseMap = Noise.GenerateNoise(mapwidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);

        Color[] colorMap = new Color[mapwidth * mapHeight];

        for (int y = 0; y < mapHeight; y++){
            for (int x = 0; x < mapwidth; x++){
                float currentHeight = NoiseMap[x, y];
                for(int i = 0; i < biomes.Length; i++){
                    if(currentHeight <= biomes[i].height) {
                        colorMap[y * mapwidth + x] = biomes[i].color;
                        break; 
                    }
                }
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if(drawMode == DrawMode.Noisemap) { display.DrawTexutre(TextureGenerator.TexturefromHeightMap(NoiseMap)); }
        else if(drawMode == DrawMode.ColorMap ){ display.DrawTexutre(TextureGenerator.TextureFromColormap(colorMap, mapwidth, mapHeight)); }
        
    }

    private void OnValidate()
    {
        if (mapwidth < 1) { mapHeight = 1; }
        if (mapHeight < 1) { mapHeight = 1; }
        if (lacunarity < 1) { lacunarity = 1; }
        if (octaves < 0) { mapHeight = 0; }
    }  
}

[System.Serializable]
public struct TerrainType{
    public string name;
    public float height;
    public Color color;
}
