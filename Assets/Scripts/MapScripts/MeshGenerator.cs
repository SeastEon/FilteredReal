using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator {

    public static Meshdata GenerateTerrainMesh(float[,] heightmap, float heightMulitplier, AnimationCurve _heightCurve, int LevelOfDetail) {
        AnimationCurve HeightCurve = new AnimationCurve(_heightCurve.keys);
        int width = heightmap.GetLength(0);
        int height = heightmap.GetLength(1);

        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        int SimpInc = (LevelOfDetail == 0) ? 1 : LevelOfDetail * 2;
        int vertsPerLine = (width - 1)/ SimpInc + 1;

        Meshdata meshdata = new Meshdata(vertsPerLine, vertsPerLine);
        int VertIndex = 0;

        for (int y = 0; y < height; y+= SimpInc)  {
            for (int x = 0; x < width; x+= SimpInc) {
                meshdata.verts[VertIndex] = new Vector3(topLeftX + x, HeightCurve.Evaluate(heightmap[x,y]) * heightMulitplier, topLeftZ - y);
                meshdata.Uvs[VertIndex] = new Vector2(x / (float)width, y / (float)height);

                if(x < width - 1 && y < height - 1) {
                    meshdata.Addtriangle(VertIndex, VertIndex + vertsPerLine + 1, VertIndex + vertsPerLine);
                    meshdata.Addtriangle(VertIndex + vertsPerLine + 1, VertIndex, VertIndex + 1);
                }
                VertIndex++;
            }
        }
        return meshdata;
    }
}

public class Meshdata
{
    public Vector3[] verts;
    public int[] triangles;
    public Vector2[] Uvs;

    int triangleIndex;

    public Meshdata(int MeshWidth, int MeshHeight)
    {
        verts = new Vector3[MeshWidth * MeshHeight];
        Uvs = new Vector2[MeshHeight * MeshWidth];
        triangles = new int[(MeshWidth - 1) * (MeshHeight - 1) * 6];
    }

    public void Addtriangle(int a, int b, int c){
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    public Mesh CreateMesh() {
        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = triangles;
        mesh.uv = Uvs;
        mesh.RecalculateBounds();
        return mesh;
    }
}
