using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPreview : MonoBehaviour {
    public Renderer textureRender;
    public MeshFilter meshfilter;
    public MeshRenderer meshRenderer;

    public enum DrawMode { NoiseMap, Mesh, FallOffMap }
    public DrawMode drawMode;

    public HeightMapSettings heightmapSettings;
    public MeshSettings meshSettings;
    public TextureData textureData;

    public Material TerrainMaterial;

    public bool AutoUpdate;

    [Range(0, MeshSettings.numsupportedLOD - 1)]
    public int EditorLevelOfDetail;

    public void DrawmapInEditor() {
        textureData.ApplyToMaterial(TerrainMaterial);
        textureData.UpdateMeshHeights(TerrainMaterial, heightmapSettings.minHeight, heightmapSettings.maxHeight);
        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.NumVertsPerLine, meshSettings.NumVertsPerLine, heightmapSettings, Vector2.zero);
        
        if (drawMode == DrawMode.NoiseMap) { DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap)); }
        else if (drawMode == DrawMode.Mesh) { DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap.Values, EditorLevelOfDetail, meshSettings)); }
        else if (drawMode == DrawMode.FallOffMap) { DrawTexture(TextureGenerator.TextureFromHeightMap( new HeightMap( FallOffGenerator.GeneratefallOffMap(meshSettings.NumVertsPerLine), 0, 1)));}
    }

    private void OnValidate()
    {
        if (meshSettings != null) {
            meshSettings.OnValuesupdated -= OnValuesUpdated;
            meshSettings.OnValuesupdated += OnValuesUpdated;
        }
        if (heightmapSettings != null) {
            heightmapSettings.OnValuesupdated -= OnValuesUpdated;
            heightmapSettings.OnValuesupdated += OnValuesUpdated;
        }
        if (textureData != null){
            textureData.OnValuesupdated -= OnTextureValuesUpdated;
            textureData.OnValuesupdated += OnTextureValuesUpdated;
        }
    }


    public void DrawTexture(Texture2D texture) {
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3 (texture.width, 1, texture.height) /10f;

        textureRender.gameObject.SetActive(true);
        meshfilter.gameObject.SetActive(false);
    }

    public void DrawMesh(Meshdata meshdata) {
        meshfilter.sharedMesh = meshdata.CreateMesh();

        meshfilter.gameObject.SetActive(true);
        textureRender.gameObject.SetActive(false);
    }

    void OnValuesUpdated() { if (!Application.isPlaying) { DrawmapInEditor(); } }
    void OnTextureValuesUpdated() { textureData.ApplyToMaterial(TerrainMaterial); }
}
