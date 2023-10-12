using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  static class MeshGenerator
{
    public static MeshData generateTerrianMesh(float[,] heightMap, float heightMulitplier, AnimationCurve heightCurve, int levelofDetail){
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float TopLeftx = (width - 1) / -2f;
        float TopLeftz = (height - 1) / 2f;
        int meshsimplicationIncreament = (levelofDetail == 0) ? 1:levelofDetail * 2;
        int positionsPerLine = (width - 1) / meshsimplicationIncreament + 1;

        MeshData meshdata = new MeshData(positionsPerLine, positionsPerLine);
        int positionIndex = 0;

        for(int y = 0; y < height; y += meshsimplicationIncreament)
        {
            for (int x = 0; x < width; x += meshsimplicationIncreament)
            {

                meshdata.position[positionIndex] = new Vector3(TopLeftx +x, heightCurve.Evaluate( heightMap[x, y]) * heightMulitplier, TopLeftz -y);
                meshdata.UVs[positionIndex] = new Vector2(x/(float)width, y/(float)height);

                if (x < width -1 && y < height - 1) {
                    meshdata.AddTriangle(positionIndex, positionIndex + positionsPerLine + 1, positionIndex + positionsPerLine);
                    meshdata.AddTriangle(positionIndex + positionsPerLine + 1, positionIndex, positionIndex + 1);
                }
                positionIndex++;
            }
        }
        return meshdata;
    }
}


public class MeshData {
    public Vector3[] position;
    public int[] triangles;
    public Vector2[] UVs;

    int triangleIndex;

    public MeshData(int Meshwidth, int Meshheight){
        position = new Vector3[Meshwidth * Meshheight];
        UVs = new Vector2[Meshwidth * Meshheight];
        triangles = new int[(Meshwidth -1) * (Meshheight -1) * 6]; 
    }

    public void AddTriangle(int a, int b, int c){
        triangles[triangleIndex] = a;
        triangles[triangleIndex +1] = b;
        triangles[triangleIndex +2] = c;
        triangleIndex += 3;
    }

    public Mesh CreateMesh(){
        Mesh mesh = new Mesh();
        mesh.vertices = position;
        mesh.triangles = triangles;
        mesh.uv = UVs;
        mesh.RecalculateNormals();
        return mesh;
    }
}