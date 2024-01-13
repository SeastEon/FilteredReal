using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;

public static class MeshGenerator {

    public static Meshdata GenerateTerrainMesh(float[,] heightmap, int LevelOfDetail, MeshSettings meshSettings) {
       
        int SkipIncreament = (LevelOfDetail == 0) ? 1 : LevelOfDetail * 2;
       int numVertsPerLine = meshSettings.NumVertsPerLine;

        Vector2 topLeft = new Vector2(-1, 1) * meshSettings.MeshWorldSize / 2f;

        Meshdata meshdata = new Meshdata(numVertsPerLine, SkipIncreament, meshSettings.UseFlatShading);

        int[,] vertsIndciesMap = new int[numVertsPerLine, numVertsPerLine];
        int meshvertsindex = 0;
        int bordervertexIndex = -1;

        for (int y = 0; y < numVertsPerLine; y ++){
            for (int x = 0; x < numVertsPerLine; x ++){
                bool isOutOfMeshvertex = y == 0 || y == numVertsPerLine - 1 || x == 0 || x == numVertsPerLine - 1;
                bool isSkippedVertex = x > 2 && x < numVertsPerLine - 3 && y > 2 && y < numVertsPerLine - 3 && ((x - 2) % SkipIncreament != 0 || (y - 2) % SkipIncreament != 0);
                if (isOutOfMeshvertex) { vertsIndciesMap[x, y] = bordervertexIndex;
                    bordervertexIndex--;
                } else if(!isSkippedVertex){
                    vertsIndciesMap[x,y] = meshvertsindex; 
                    meshvertsindex++;
                }
            }
        }

        for (int y = 0; y < numVertsPerLine; y++)  {
            for (int x = 0; x < numVertsPerLine; x++) {
                bool isSkippedVertex = x > 2 && x < numVertsPerLine - 3 && y > 2 && y < numVertsPerLine - 3 && ((x - 2) % SkipIncreament != 0 || (y - 2) % SkipIncreament != 0);

                if (!isSkippedVertex) {
                    bool isOutOfMeshvertex = y == 0 || y == numVertsPerLine - 1 || x == 0 || x == numVertsPerLine - 1;
                    bool IsMeshEdgeVertex = y == 1 || y == numVertsPerLine - 2 || x == 1 || x == numVertsPerLine - 2;
                    bool isMainVertex = (x - 2) % SkipIncreament == 0 && (y - 2) % SkipIncreament == 0 && !isOutOfMeshvertex && !IsMeshEdgeVertex;
                    bool isEdgeConnectionVertex = (y==2 || y == numVertsPerLine-3 || x == 2 || x ==numVertsPerLine-3) && !IsMeshEdgeVertex && !isOutOfMeshvertex && !isMainVertex;

                    int VertIndex = vertsIndciesMap[x, y];
                    Vector2 percent = new Vector2(x-1, y-1) / (numVertsPerLine -3);
                    Vector3 vertexPosition2D = topLeft + new Vector2(percent.x , -percent.y) * meshSettings.MeshWorldSize;

                    float height = heightmap[x, y];

                    if (isEdgeConnectionVertex) {
                        bool isvertical = x == 2 || x == numVertsPerLine - 3;
                        int dstToMainVertA = ((isvertical)? y -2: x -2) % SkipIncreament;
                        int dstToMainVertB = SkipIncreament - dstToMainVertA;
                        float dstPercentFromAToB = dstToMainVertA / (float)SkipIncreament;

                        float HeightOfMainVertA = heightmap[(isvertical)?x:x - dstToMainVertA, (isvertical)? y -dstToMainVertA : y];
                        float HeightOfMainVertB = heightmap[(isvertical) ? x : x + dstToMainVertB, (isvertical) ? y + dstToMainVertB : y];

                        height = HeightOfMainVertA * (1-dstPercentFromAToB) + HeightOfMainVertB * dstPercentFromAToB;

                    }

                    meshdata.AddVert(new Vector3(vertexPosition2D.x, height, vertexPosition2D.y), percent, VertIndex);

                    bool createTriangle = x < numVertsPerLine - 1 && y < numVertsPerLine - 1 && (!isEdgeConnectionVertex || (x != 2 && y != 2));


                    if (createTriangle) {
                        int CurrInc = (isMainVertex && x != numVertsPerLine - 3 && y != numVertsPerLine - 3) ? SkipIncreament : 1;
                        int a = vertsIndciesMap[x, y];
                        int b = vertsIndciesMap[x + CurrInc, y];
                        int c = vertsIndciesMap[x, y + CurrInc];
                        int d = vertsIndciesMap[x + CurrInc, y + CurrInc];
                        meshdata.Addtriangle(a, d, c);
                        meshdata.Addtriangle(d, a, b);
                    }
                }
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

    Vector3[] OutOfMeshVerts;
    int[] OutOfMeshTriangles;

    int triangleIndex;
    int OutOfMeshTriangleIndex;

    bool UseFlatShading;

    public Meshdata(int numVertsPerLine, int skipIncreamnet, bool UseFlatShading)  {
        this.UseFlatShading = UseFlatShading;

        int numMeshEdgeVerts = (numVertsPerLine - 2) * 4 - 4;
        int numEdgeconnectionVerts = (skipIncreamnet - 1) * (numVertsPerLine - 5) / skipIncreamnet * 4;
        int numMainVertsPerLine = (numVertsPerLine - 5) / skipIncreamnet + 1;
        int numMainVerts = numMainVertsPerLine * numMainVertsPerLine;

        verts = new Vector3[numMeshEdgeVerts + numEdgeconnectionVerts + numMainVerts];
        Uvs = new Vector2[verts.Length];

        int numMeshEdgeTriangle = (numVertsPerLine - 4) * 8;
        int numMainTriangles = (numMainVertsPerLine - 1) * (numMainVertsPerLine - 1) * 2;
        triangles = new int[(numMeshEdgeTriangle + numMainTriangles) * 3];

        OutOfMeshVerts = new Vector3[numVertsPerLine * 4 - 4];
        OutOfMeshTriangles = new int[ 24 *(numVertsPerLine -2) ];
    }

    public void AddVert(Vector3 VertPostion, Vector2 Uv, int Index){
        if(Index < 0) { OutOfMeshVerts[-Index -1] = VertPostion;} 
        else {
            verts[Index] = VertPostion;
            Uvs[Index] = Uv;
        }
    }

    public void Addtriangle(int a, int b, int c){
        if(a < 0 || b < 0 || c < 0){
            OutOfMeshTriangles[OutOfMeshTriangleIndex] = a;
            OutOfMeshTriangles[OutOfMeshTriangleIndex + 1] = b;
            OutOfMeshTriangles[OutOfMeshTriangleIndex + 2] = c;
            OutOfMeshTriangleIndex += 3;
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

        int BordertriangleCount = OutOfMeshTriangles.Length / 3;
        for (int i = 0; i < BordertriangleCount; i++) {
            int NormalTriangleIndex = i * 3;
            int vertexIndexA = OutOfMeshTriangles[NormalTriangleIndex];
            int vertexIndexB = OutOfMeshTriangles[NormalTriangleIndex + 1];
            int vertexIndexC = OutOfMeshTriangles[NormalTriangleIndex + 2];

            Vector3 TriangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            if(vertexIndexA >= 0) { vertexNormals[vertexIndexA] += TriangleNormal; }
            if (vertexIndexB >= 0) { vertexNormals[vertexIndexB] += TriangleNormal; }
            if (vertexIndexC >= 0) { vertexNormals[vertexIndexC] += TriangleNormal; }     
            
        }

        for (int i = 0; i < vertexNormals.Length; i++) {vertexNormals[i].Normalize();}
        return vertexNormals;
    }

    Vector3 SurfaceNormalFromIndices(int InA, int InB, int InC) {
        Vector3 pA = (InA < 0) ? OutOfMeshVerts[-InA-1] : verts[InA];
        Vector3 pB = (InB < 0) ? OutOfMeshVerts[-InB-1] : verts[InB];
        Vector3 pC = (InC < 0) ? OutOfMeshVerts[-InC - 1] : verts[InC];

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
