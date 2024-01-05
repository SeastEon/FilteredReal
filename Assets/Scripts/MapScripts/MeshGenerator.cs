using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;

public static class MeshGenerator {

    public static Meshdata GenerateTerrainMesh(float[,] heightmap, float heightMulitplier, AnimationCurve _heightCurve, int LevelOfDetail, bool useFlatShading) {
        AnimationCurve HeightCurve = new AnimationCurve(_heightCurve.keys);
        int SimpInc = (LevelOfDetail == 0) ? 1 : LevelOfDetail * 2;

        int borderedSize = heightmap.GetLength(0);
        int meshSize = borderedSize - 2 * SimpInc;
        int meshSizeUnSimplfied = borderedSize - 2;

        float topLeftX = (meshSizeUnSimplfied - 1) / -2f;
        float topLeftZ = (meshSizeUnSimplfied - 1) / 2f;
       
        int vertsPerLine = (meshSize - 1)/ SimpInc + 1;

        Meshdata meshdata = new Meshdata(vertsPerLine, useFlatShading);
        int[,] vertsIndciesMap = new int[borderedSize, borderedSize];
        int meshvertsindex = 0;
        int bordervertexIndex = -1;


        for (int y = 0; y < borderedSize; y += SimpInc){
            for (int x = 0; x < borderedSize; x += SimpInc){
                
                bool isBorderVertex = y == 0 || y == borderedSize - 1 || x == 0 || x == borderedSize - 1;
                if (isBorderVertex) { vertsIndciesMap[x, y] = bordervertexIndex;
                    bordervertexIndex--;
                } else {
                    vertsIndciesMap[x,y] = meshvertsindex; 
                    meshvertsindex++;
                }
            }
        }

        for (int y = 0; y < borderedSize; y+= SimpInc)  {
            for (int x = 0; x < borderedSize; x+= SimpInc) {
                int VertIndex = vertsIndciesMap[x, y];
                Vector2 percent = new Vector2((x - SimpInc) / (float)meshSize, (y - SimpInc) / (float)meshSize);
                float height = HeightCurve.Evaluate(heightmap[x, y]) * heightMulitplier;
                Vector3 vertexPosition = new Vector3(topLeftX + percent.x * meshSizeUnSimplfied, height, topLeftZ - percent.y * meshSizeUnSimplfied);

                meshdata.AddVert(vertexPosition, percent, VertIndex);

                if(x < borderedSize - 1 && y < borderedSize - 1) {
                    int a = vertsIndciesMap[x, y];
                    int b = vertsIndciesMap[x + SimpInc, y];
                    int c = vertsIndciesMap[x, y + SimpInc];
                    int d = vertsIndciesMap[x + SimpInc, y + SimpInc];
                    meshdata.Addtriangle(a, d, c);
                    meshdata.Addtriangle(d, a ,b);
                }
                VertIndex++;
            }
        }
        meshdata.FinalizeNormals();
        return meshdata;
    }
}

public class Meshdata {
    Vector3[] verts;
    int[] triangles;
    Vector2[] Uvs;
    Vector3[] BakedNormals;

    Vector3[] borderVerts;
    int[] borderTriangles;

    int triangleIndex;
    int borderTriangleIndex;

    bool UseFlatShading;

    public Meshdata(int VertsPerLine, bool UseFlatShading)  {
        this.UseFlatShading = UseFlatShading;
        verts = new Vector3[VertsPerLine * VertsPerLine];
        Uvs = new Vector2[VertsPerLine * VertsPerLine];
        triangles = new int[(VertsPerLine - 1) * (VertsPerLine - 1) * 6];

        borderVerts = new Vector3[VertsPerLine * 4 + 4];
        borderTriangles = new int[24 * VertsPerLine];
    }

    public void AddVert(Vector3 VertPostion, Vector2 Uv, int Index){
        if(Index < 0) { borderVerts[-Index -1] = VertPostion;} 
        else {
            verts[Index] = VertPostion;
            Uvs[Index] = Uv;
        }
    }

    public void Addtriangle(int a, int b, int c){
        if(a < 0 || b < 0 || c < 0){
            borderTriangles[borderTriangleIndex] = a;
            borderTriangles[borderTriangleIndex + 1] = b;
            borderTriangles[borderTriangleIndex + 2] = c;
            borderTriangleIndex += 3;
        } else {
            triangles[triangleIndex] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;
            triangleIndex += 3;
        }

    }

    Vector3[] CalucalteNormals() {
        Vector3[] vertexNormals = new Vector3[verts.Length];
        int triangleCount = triangles.Length / 3;
        for (int i = 0; i < triangleCount; i++) {
            int NormalTriangleIndex = i * 3;
            int vertexIndexA = triangles[NormalTriangleIndex];
            int vertexIndexB = triangles[NormalTriangleIndex + 1];
            int vertexIndexC = triangles[NormalTriangleIndex + 2];

            Vector3 TriangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += TriangleNormal;
            vertexNormals[vertexIndexB] += TriangleNormal;
            vertexNormals[vertexIndexC] += TriangleNormal;
        }

        int BordertriangleCount = borderTriangles.Length / 3;
        for (int i = 0; i < BordertriangleCount; i++) {
            int NormalTriangleIndex = i * 3;
            int vertexIndexA = borderTriangles[NormalTriangleIndex];
            int vertexIndexB = borderTriangles[NormalTriangleIndex + 1];
            int vertexIndexC = borderTriangles[NormalTriangleIndex + 2];

            Vector3 TriangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            if(vertexIndexA >= 0) { vertexNormals[vertexIndexA] += TriangleNormal; }
            if (vertexIndexB >= 0) { vertexNormals[vertexIndexB] += TriangleNormal; }
            if (vertexIndexC >= 0) { vertexNormals[vertexIndexC] += TriangleNormal; }     
            
        }

        for (int i = 0; i < vertexNormals.Length; i++) {vertexNormals[i].Normalize();}
        return vertexNormals;
    }

    Vector3 SurfaceNormalFromIndices(int InA, int InB, int InC) {
        Vector3 pA = (InA < 0) ? borderVerts[-InA-1] : verts[InA];
        Vector3 pB = (InB < 0) ? borderVerts[-InB-1] : verts[InB];
        Vector3 pC = (InC < 0) ? borderVerts[-InC - 1] : verts[InC];

        Vector3 sideAB = pB - pA;
        Vector3 sideAC = pC - pA;
        return Vector3.Cross(sideAB, sideAC).normalized;
    }

    public void FinalizeNormals() {
        if (UseFlatShading) { FlatShading(); }
        else { BakeNormals(); }
    }

    void BakeNormals() {BakedNormals = CalucalteNormals(); }

    public void FlatShading() {
        Vector3[] FlatShadedVerts = new Vector3[triangles.Length];
        Vector2[] FlatShadeUvs = new Vector2[triangles.Length];

        for (int i = 0; i < triangles.Length; i++) {
            FlatShadedVerts[i] = verts[triangles[i]];
            FlatShadeUvs[i] = Uvs[triangles[i]];
            triangles[i] = i;
        }

        verts = FlatShadedVerts;
        Uvs = FlatShadeUvs;
    }

    public Mesh CreateMesh() {
        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = triangles;
        mesh.uv = Uvs;
        if (UseFlatShading) { mesh.RecalculateNormals(); }
        else { mesh.normals = BakedNormals; }
        return mesh;
    }
}
