using UnityEngine;

public class TerrainChunk {

    const float colliderGenerationDistThreshold = 5;
    public event System.Action<TerrainChunk, bool> OnVisibleChanged;
    public Vector2 coord;

    GameObject meshObject;
    Vector2 Samplecentre;
    Bounds bounds;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;

    LODInfo[] detailLevels;
    LODMesh[] LODMeshes;
    int colliderLODIndex;

    HeightMap heightMap;
    bool HeightMapRecieved;
    int PreviousLOD = -1;
    bool hasSetCollider;
    float maxViewDst;

    HeightMapSettings heightMapSettings;
    MeshSettings meshSettings;
    Transform viewer;

    public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Transform Viewer, Material mat) {
        this.coord = coord;
        this.detailLevels = detailLevels;
        this.colliderLODIndex = colliderLODIndex;
        this.heightMapSettings = heightMapSettings;
        this.meshSettings = meshSettings;
        this.viewer = Viewer;

        Samplecentre = coord * meshSettings.MeshWorldSize / meshSettings.MeshScale;
        Vector2 position = coord * meshSettings.MeshWorldSize;
        bounds = new Bounds(position, Vector2.one * meshSettings.MeshWorldSize);

        meshObject = new GameObject("Terrain Chunk");
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshCollider = meshObject.AddComponent<MeshCollider>();
        meshRenderer.material = mat;
        meshObject.layer = 10;

        meshObject.transform.position = new Vector3(position.x, 0, position.y);
        meshObject.transform.parent = parent;
        SetVisble(false);

        LODMeshes = new LODMesh[detailLevels.Length];
        for (int i = 0; i < detailLevels.Length; i++) {
            LODMeshes[i] = new LODMesh(detailLevels[i].lod);
            LODMeshes[i].updatecallback += UpdateTerrainChunk;
            if (i == colliderLODIndex) { LODMeshes[i].updatecallback += UpdateCollisionMesh; }
        }
        maxViewDst = detailLevels[detailLevels.Length - 1].visibleDsthreshold;
    }

    public void Load() {
        ThreadDataRequester.RequestData(() => HeightMapGenerator.GenerateHeightMap(meshSettings.NumVertsPerLine, meshSettings.NumVertsPerLine, heightMapSettings, Samplecentre), OnHeightMapRecieved);
    }


    void OnHeightMapRecieved(object heightmapObject) {
        heightMap = (HeightMap)heightmapObject;
        HeightMapRecieved = true;
        UpdateTerrainChunk();
    }

    Vector2 viewerPosition {
         get { return new Vector2(viewer.position.x, viewer.position.z); }
    }

    public void UpdateTerrainChunk(){
        if (HeightMapRecieved) {
            float viewerDstFromEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            
            bool wasVisible = IsVisible();
            bool visible = viewerDstFromEdge <= maxViewDst;

            if (visible) {
                int lodIndex = 0;

                for (int i = 0; i < detailLevels.Length - 1; i++) {
                    if (viewerDstFromEdge > detailLevels[i].visibleDsthreshold) { lodIndex = i + 1; }
                    else { break; }
                }

                if (lodIndex != PreviousLOD) {
                    LODMesh lodmesh = LODMeshes[lodIndex];
                    if (lodmesh.Hasmesh) {
                        PreviousLOD = lodIndex;
                        meshFilter.mesh = lodmesh.mesh;
                    }
                    else if (!lodmesh.hasRequestedmesh) {lodmesh.RequestMesh(heightMap, meshSettings); }
                }
            }
            if (wasVisible != visible) {
                SetVisble(visible);
                if (OnVisibleChanged != null) { OnVisibleChanged(this, visible); }
            }
        }
    }

    public void UpdateCollisionMesh() {
        if (!hasSetCollider) {
            float sqrDstFromViewerToEdge = bounds.SqrDistance(viewerPosition);

            if (sqrDstFromViewerToEdge < detailLevels[colliderLODIndex].sqrVisibleDstThreehold) {
                if (!LODMeshes[colliderLODIndex].hasRequestedmesh) { LODMeshes[colliderLODIndex].RequestMesh(heightMap, meshSettings);}
            }

            if (sqrDstFromViewerToEdge < colliderGenerationDistThreshold * colliderGenerationDistThreshold) {
                if (LODMeshes[colliderLODIndex].Hasmesh) {
                    meshCollider.sharedMesh = LODMeshes[colliderLODIndex].mesh;
                    hasSetCollider = true;
                }
            }
        }
    }

    public void SetVisble(bool visble) { meshObject.SetActive(visble); }
    public bool IsVisible() { return meshObject.activeSelf; }
}

class LODMesh
{
    public Mesh mesh;
    public bool hasRequestedmesh;
    public bool Hasmesh;
    int lod;
    public event System.Action updatecallback;

    public LODMesh(int lod) { this.lod = lod; }

    void OnMeshDataRecieved(object meshdataObject) {
        mesh = ((Meshdata)meshdataObject).CreateMesh();
        Hasmesh = true;
        updatecallback();
    }

    public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings) {
        hasRequestedmesh = true;
        ThreadDataRequester.RequestData(() => MeshGenerator.GenerateTerrainMesh(heightMap.Values, lod, meshSettings), OnMeshDataRecieved);
    }
}

