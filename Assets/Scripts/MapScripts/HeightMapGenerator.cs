using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMapGenerator {
    public static HeightMap GenerateHeightMap(int width , int height, HeightMapSettings settings, Vector2 sampleCenter) {
        float[,] values = Noise.GenerateNoiseMap(width, height, settings.noiseSettings, sampleCenter);
        
        AnimationCurve heightcurve_threadSafe = new AnimationCurve(settings.HeightCurve.keys);
        
        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        for (int i = 0; i < width; i++){
            for (int j = 0; j < height; j++) {
                values[i, j] *= heightcurve_threadSafe.Evaluate(values[i, j]) * settings.HeightMult;
                if (values[i, j] > maxValue) maxValue = values[i, j];
                if (values[i, j] < minValue) minValue = values[i, j];
            }
        }
        return new HeightMap(values, minValue, maxValue);
    }
}


public struct HeightMap {
    public readonly float[,] Values;
    public readonly float minValues;
    public readonly float maxValues;

    public HeightMap(float[,] heightMap, float minValues, float maxValues) {
        this.Values = heightMap;
        this.minValues = minValues;
        this.maxValues = maxValues;
    }
}
