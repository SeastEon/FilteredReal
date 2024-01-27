using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


public class WaterPlane {

    public GameObject waterObject;
    public MeshRenderer waterMeshRenderer;
    public MeshFilter waterMeshFilter;
    public TerrainChunk parent;
    public float Size;

    public WaterPlane(TerrainChunk parent, float Size, Vector2 Position, Material mat) {
        this.parent = parent;
        this.Size = Size;
       
        waterObject = new GameObject("WaterLayer");
        waterMeshRenderer = waterObject.AddComponent<MeshRenderer>();
        waterMeshFilter = waterObject.AddComponent<MeshFilter>();
        waterMeshFilter.mesh = GenerateWater();
        waterMeshRenderer.material = mat;

        waterObject.transform.position = new Vector3(0, 1.7f, 0);
        waterObject.transform.parent = parent.meshObject.transform;
    }

    public void Update() {
        waterObject.SetActive(parent.IsVisible()); //sets the water to visible if the chunk it is assigned to is visble
    }

    Mesh GenerateWater() {
        Mesh watermesh = new Mesh();
        // Define vertices, triangles, normals, and UVs for the water mesh
        Vector3[] vertices = {
            new Vector3(-Size / 2, 0, -Size / 2),
            new Vector3(-Size / 2, 0, Size / 2),
            new Vector3(Size / 2, 0, Size / 2),
            new Vector3(Size / 2, 0, -Size / 2)
        };

        int[] triangles = { 0, 1, 2, 0, 2, 3 };

        Vector3[] normals = { Vector3.up, Vector3.up, Vector3.up, Vector3.up };

        Vector2[] uv = {
            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(1, 0)
        };

        // Assign data to the mesh
        watermesh.vertices = vertices;
        watermesh.triangles = triangles;
        watermesh.normals = normals;
        watermesh.uv = uv;

        return watermesh;
    }
}
