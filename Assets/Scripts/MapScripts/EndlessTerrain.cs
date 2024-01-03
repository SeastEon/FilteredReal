using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    public const float maxViewdst = 300;
    public Transform viewer;
    public Material mapmaterial;

    public static Vector2 viewerPosition;
    static MapGenerator mapGenerator;
    int chunkSize;
    int chunVisible;

    List<TerrainChunk> TerrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    Dictionary<Vector2, TerrainChunk> TerrarinChunkDict = new Dictionary<Vector2, TerrainChunk>();

    void Start() {
        mapGenerator = FindAnyObjectByType<MapGenerator>();
        chunkSize = MapGenerator.MapChuckSize - 1;
        chunVisible = Mathf.RoundToInt(maxViewdst / chunkSize);
    }

    private void Update() {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
    }

    void UpdateVisibleChunks() {
        for (int i = 0; i < TerrainChunksVisibleLastUpdate.Count; i++) {
            TerrainChunksVisibleLastUpdate[i].SetVisble(false);
        }
        TerrainChunksVisibleLastUpdate.Clear();
        int CurrChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int CurrChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunVisible; yOffset <= chunVisible; yOffset++){
            for (int xOffset = -chunVisible; xOffset <= chunVisible; xOffset++) {
                Vector2 viewdChunkCoord = new Vector2(CurrChunkCoordX + xOffset,CurrChunkCoordY + yOffset);

                if (TerrarinChunkDict.ContainsKey(viewdChunkCoord)) {
                    TerrarinChunkDict[viewdChunkCoord].UpdateTerrainChunk();

                    if (TerrarinChunkDict[viewdChunkCoord].IsVisible()){
                        TerrainChunksVisibleLastUpdate.Add(TerrarinChunkDict[viewdChunkCoord]);
                    }
                } else { TerrarinChunkDict.Add(viewdChunkCoord, new TerrainChunk(viewdChunkCoord, chunkSize, transform, mapmaterial)); }
            }
        }
    }

    public class TerrainChunk { 
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        public TerrainChunk(Vector2 coord, int size, Transform parent, Material mat) {
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshRenderer.material = mat;
            meshFilter = meshObject.AddComponent<MeshFilter>();

            meshObject.transform.position = positionV3;
            meshObject.transform.parent = parent;
            SetVisble(false);

            mapGenerator.RequestMapdata(OnMapDataRecieved);
        }

        void OnMapDataRecieved(Mapdata mapdata) {
            mapGenerator.RequestMeshData(mapdata, OnMeshdataRecieved);
        }

        void OnMeshdataRecieved(Meshdata meshdata) {
            meshFilter.mesh = meshdata.CreateMesh();
        }

        public void UpdateTerrainChunk() {
            float viewerDstFromEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewerDstFromEdge <= maxViewdst;
            SetVisble(visible);
        }

        public void SetVisble(bool visble) {
            meshObject.SetActive(visble);
        }

        public bool IsVisible() {
            return meshObject.activeSelf;
        }
    }
}

