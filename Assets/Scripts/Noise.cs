using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise {

    public enum Normailise { local, global }
    public static float[,] GenerateNoise(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, Normailise Normalisemode)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random rand = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = rand.Next(-100000, 100000) + offset.x;
            float offsetY = rand.Next(-100000, 100000) - offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= frequency;
        } 

        if(scale <= 0)
        {scale = 0.000001f;}

        float MaxLocalNoiseHeight = float.MinValue;
        float MinLocalNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2;
        float halfHeight = mapHeight / 2;
        
        for (int y = 0; y < mapHeight; y++) {
            for(int x =0; x < mapWidth; x++){

                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++) {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency ;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency ;
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 -1;

                    noiseHeight += perlinValue * amplitude;
                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if(noiseHeight > MaxLocalNoiseHeight){
                    MaxLocalNoiseHeight = noiseHeight;
                } else if(noiseHeight < MinLocalNoiseHeight){
                    MinLocalNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }

       
        for (int y = 0; y < mapHeight; y++){
            for (int x = 0; x < mapWidth; x++){
                if (Normalisemode == Normailise.local) {
                    noiseMap[x, y] = Mathf.InverseLerp(MinLocalNoiseHeight, MaxLocalNoiseHeight, noiseMap[x, y]);
                } else {
                    float NormalisedHeight = (noiseMap[x, y] + 1) / (maxPossibleHeight);
                    noiseMap[x, y] = Mathf.Clamp(NormalisedHeight, 0, int.MaxValue);
                }
            }
        }
        return noiseMap;
    }
}
