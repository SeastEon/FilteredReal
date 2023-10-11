using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise {
    public static float[,] GenerateNoise(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset){
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random rand = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for(int i = 0; i < octaves; i++)
        {
            float offsetX = rand.Next(-100000, 100000) + offset.x;
            float offsetY = rand.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY); 
        } 

        if(scale <= 0)
        {scale = 0.000001f;}

        float MaxNoiseHeight = float.MinValue;
        float MinNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2;
        float halfHeight = mapHeight / 2;
        
        for (int y = 0; y < mapHeight; y++) {
            for(int x =0; x < mapWidth; x++){

                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++) {
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 -1;

                    noiseHeight += perlinValue * amplitude;
                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if(noiseHeight > MaxNoiseHeight){
                    MaxNoiseHeight = noiseHeight;
                } else if(noiseHeight < MinNoiseHeight){
                    MinNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++){
            for (int x = 0; x < mapWidth; x++){
                noiseMap[x, y] = Mathf.InverseLerp(MinNoiseHeight, MaxNoiseHeight, noiseMap[x, y]);
            }
        }
                return noiseMap;
    }
}
