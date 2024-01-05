using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour{
    const float ViewerMoveThresHoldForChunk = 25f;
    const float SquareViewMoveThres = ViewerMoveThresHoldForChunk * ViewerMoveThresHoldForChunk;
    public LODInfo[] detailLevels;
    public static float maxViewdst = 300;
    public LayerMask GroundLayer;
    public Transform viewer;
    public Material mapmaterial;
    public static Vector2 viewerPosition;
    Vector2 viewerPoistionOld;
    static MapGenerator mapGenerator;
    int chunkSize;
    int chunVisible;

   static List<TerrainChunk> TerrainChunksVisibleLastUpdate = new List<TerrainChunk>();
    Dictionary<Vector2, TerrainChunk> TerrarinChunkDict = new Dictionary<Vector2, TerrainChunk>();

    void Start() {
        mapGenerator = FindAnyObjectByType<MapGenerator>();
        maxViewdst = detailLevels[detailLevels.Length - 1].visibleDstthreshold;
        chunkSize = mapGenerator.MapChuckSize - 1;
        chunVisible = Mathf.RoundToInt(maxViewdst / chunkSize);

        UpdateVisibleChunks();
    }

    private void Update() {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / mapGenerator.terrainData.Uniformscale;
        if((viewerPoistionOld - viewerPosition).sqrMagnitude > SquareViewMoveThres) {viewerPoistionOld = viewerPosition; }
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

                if (TerrarinChunkDict.ContainsKey(viewdChunkCoord)) {TerrarinChunkDict[viewdChunkCoord].UpdateTerrainChunk();
                } else { TerrarinChunkDict.Add(viewdChunkCoord, new TerrainChunk(viewdChunkCoord, chunkSize, detailLevels, transform, mapmaterial)); }
            }
        }
    }

    public class TerrainChunk { 
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;
        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;
        LODInfo[] detailLevels;
        LODMesh[] LODMeshes;
        LODMesh CollsionLODMesh;
        Mapdata mapdata;
        bool mapdataRecieved;
        int PreviousLOD = -1;

        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material mat) {
            this.detailLevels = detailLevels;
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshRenderer.material = mat;
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();
            meshObject.layer = 10;

            meshObject.transform.position = positionV3 * mapGenerator.terrainData.Uniformscale;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * mapGenerator.terrainData.Uniformscale;
            SetVisble(false);

            LODMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++)  {
                LODMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
                if (detailLevels[i].useForCollider) { CollsionLODMesh = LODMeshes[i]; }
            }

            mapGenerator.RequestMapdata(position, OnMapDataRecieved);
        }

        void OnMapDataRecieved(Mapdata mapdata) {
            this.mapdata = mapdata;
            mapdataRecieved = true;
            Texture2D texture = TextureGenerator.TextureFromColourMap(mapdata.colorMap, mapGenerator.MapChuckSize, mapGenerator.MapChuckSize);
            meshRenderer.material.mainTexture = texture;
            UpdateTerrainChunk();
        }

        public void UpdateTerrainChunk() {
            if (mapdataRecieved) {
                float viewerDstFromEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool visible = viewerDstFromEdge <= maxViewdst;

                if (visible)  {
                    int lodIndex = 0;

                    for (int i = 0; i < detailLevels.Length - 1; i++)  {
                        if (viewerDstFromEdge > detailLevels[i].visibleDstthreshold) { lodIndex = i + 1; } 
                        else { break; }
                    }
                    if (lodIndex != PreviousLOD) {
                        LODMesh lodmesh = LODMeshes[lodIndex];
                        if (lodmesh.Hasmesh) {
                            PreviousLOD = lodIndex;
                            meshFilter.mesh = lodmesh.mesh;
                        } else if (!lodmesh.hasRequestedmesh) {lodmesh.RequestMesh(mapdata); }

                        if(lodIndex == 0) {
                            if (CollsionLODMesh.Hasmesh) { meshCollider.sharedMesh = CollsionLODMesh.mesh; }
                            else if(!CollsionLODMesh.hasRequestedmesh) { CollsionLODMesh.RequestMesh(mapdata);  }
                        }

                    } TerrainChunksVisibleLastUpdate.Add(this);
                }  SetVisble(visible);
            }
        }

        public void SetVisble(bool visble) {meshObject.SetActive(visble);}
        public bool IsVisible() { return meshObject.activeSelf;}
    }

    class LODMesh {
        public Mesh mesh;
        public bool hasRequestedmesh;
        public bool Hasmesh;
        int lod;
        System.Action updatecallback;

        public LODMesh(int lod, System.Action updatecallback) {
            this.lod = lod;
            this.updatecallback = updatecallback;
        }

        void OnMeshDataRecieved(Meshdata meshdata) {
            mesh = meshdata.CreateMesh();
            Hasmesh = true;
            updatecallback();
        }

        public void RequestMesh(Mapdata mapdata) {
            hasRequestedmesh = true;
            mapGenerator.RequestMeshData(mapdata, lod, OnMeshDataRecieved);
        }
    }

    [System.Serializable]
    public struct LODInfo {
        public int lod;
        public float visibleDstthreshold;
        public bool useForCollider;
    }
}

