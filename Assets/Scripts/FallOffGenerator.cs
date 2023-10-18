using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FallOffGenerator {
    // Start is called before the first frame update

    public static float[,] GenerateFallOffMap(int size) {
        float[,] Map = new float[size, size];

        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                float x = i/(float) size * 2 -1;
                float y = j / (float)size * 2 - 1;

                float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                Map[i, j] = Evaulate(value);
            }
        }
        return Map;
    }

    static float Evaulate(float value){
        float a = 3;
        float b = 2.2f;

        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }
}
