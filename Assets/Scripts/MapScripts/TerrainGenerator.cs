using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour{
    const float ViewerMoveThresHoldForChunk = 25f;
    const float SquareViewMoveThres = ViewerMoveThresHoldForChunk * ViewerMoveThresHoldForChunk;
    
    public int colliderLODIndex;
    public LODInfo[] detailLevels;

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureData textureSettings;

    public LayerMask GroundLayer;

    public Transform viewer;
    public Material mapmaterial;

    Vector2 viewerPosition;
    Vector2 viewerPoistionOld;

    float meshWorldSize;
    int chunkVisible;

    Dictionary<Vector2, TerrainChunk> TerrarinChunkDict = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> VisibleTerrainChunks = new List<TerrainChunk>();

    void Start() {
        textureSettings.ApplyToMaterial(mapmaterial);
        textureSettings.UpdateMeshHeights(mapmaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

        float maxViewdst = detailLevels[detailLevels.Length - 1].visibleDsthreshold;
        meshWorldSize = meshSettings.MeshWorldSize;
        chunkVisible = Mathf.RoundToInt(maxViewdst / meshWorldSize);

        UpdateVisibleChunks();
    }

    private void Update() {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        if (viewerPosition != viewerPoistionOld){
            foreach (TerrainChunk chunk in VisibleTerrainChunks) { 
                chunk.UpdateCollisionMesh();
            }
        }

        if ((viewerPoistionOld - viewerPosition).sqrMagnitude > SquareViewMoveThres) {
            viewerPoistionOld = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    void UpdateVisibleChunks() {
        HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();
        for (int i = VisibleTerrainChunks.Count -1; i >= 0; i--) {
            alreadyUpdatedChunkCoords.Add(VisibleTerrainChunks[i].coord);
            VisibleTerrainChunks[i].UpdateTerrainChunk();
         }

        int CurrChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
        int CurrChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);

        for (int yOffset = -chunkVisible; yOffset <= chunkVisible; yOffset++){
            for (int xOffset = -chunkVisible; xOffset <= chunkVisible; xOffset++) {
                Vector2 viewdChunkCoord = new Vector2(CurrChunkCoordX + xOffset,CurrChunkCoordY + yOffset);
                if(!alreadyUpdatedChunkCoords.Contains(viewdChunkCoord)) {
                    if (TerrarinChunkDict.ContainsKey(viewdChunkCoord)) {
                        TerrarinChunkDict[viewdChunkCoord].UpdateTerrainChunk(); }
                    else {
                        TerrainChunk newChunk = new TerrainChunk(viewdChunkCoord, heightMapSettings, meshSettings, detailLevels, colliderLODIndex, transform, viewer, mapmaterial);
                        TerrarinChunkDict.Add(viewdChunkCoord, newChunk);
                        newChunk.OnVisibleChanged += OnTerrainChunkVisibiltyChanged;
                        newChunk.Load();
                    }
                }
            }
        }
    }

    void OnTerrainChunkVisibiltyChanged(TerrainChunk chunk, bool isVisible) {
        if (isVisible) { VisibleTerrainChunks.Add(chunk);  }
        else { VisibleTerrainChunks.Remove(chunk); }
    }
}


[System.Serializable]
public struct LODInfo
{
    [Range(0, MeshSettings.numsupportedLOD - 1)]
    public int lod;
    public float visibleDsthreshold;
    public float sqrVisibleDstThreehold {
        get { return visibleDsthreshold * visibleDsthreshold; }
    }
}

