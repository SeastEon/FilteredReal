using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TerrainUtils;
using UnityEngine.UIElements;

public class EndlessTerrain : MonoBehaviour {
    public const float MaxViewDistance = 450;
    public Transform viewer;
    public Material mapMaterial;

    public static Vector2 viewPosition;

    static MapGenerator mapGenerator; //reference to mapGenerator class

    int chunkSize;
    int chunksVisibleInViewDistance;

    Dictionary<Vector2, TerrainChunks> chunksDictionary = new Dictionary<Vector2, TerrainChunks>();
    List<TerrainChunks> terrainChunksVisibleLastUpdate = new List<TerrainChunks>();

    void Start(){
        mapGenerator = FindObjectOfType<MapGenerator>();
        chunkSize = MapGenerator.MapChunkSize - 1;
        chunksVisibleInViewDistance = Mathf.RoundToInt(MaxViewDistance / chunkSize);
    }

     void Update(){
        viewPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibileChunks();
     }

    void UpdateVisibileChunks() {
        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++) {
            terrainChunksVisibleLastUpdate[i].SetVisibile(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewPosition.x/ chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewPosition.y / chunkSize);

        for(int yOffset = -chunksVisibleInViewDistance;  yOffset <= chunksVisibleInViewDistance; yOffset++) {
            for (int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++) {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                
                if (chunksDictionary.ContainsKey(viewedChunkCoord)) {
                    chunksDictionary[viewedChunkCoord].updateTerrainChunk();
                    if(chunksDictionary[viewedChunkCoord].IsVisible()) {
                        terrainChunksVisibleLastUpdate.Add(chunksDictionary[viewedChunkCoord]);
                    }
                } else {
                    chunksDictionary.Add(viewedChunkCoord, new TerrainChunks(viewedChunkCoord, chunkSize, transform, mapMaterial));
                }
            }
        }
    }

    public class TerrainChunks {
        GameObject meshObject;
        MeshRenderer meshRenderer;
        MeshFilter meshFilter;

        Vector2 position;
        Bounds bounds;

        public TerrainChunks(Vector2 coord, int size, Transform parent, Material material ) {
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 position3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Terrain chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer.material = material;

            meshObject.transform.position = position3;
            meshObject.transform.parent = parent;
            SetVisibile(false);

            mapGenerator.RequestMapData(OnMapdataRecieved);
        }

        void OnMapdataRecieved(MapData mapData) {
            mapGenerator.RequestMeshData(mapData, OnMeshdataRecieved);
        }

        void OnMeshdataRecieved(MeshData meshData) { 
            meshFilter.mesh = meshData.CreateMesh();
        }

        public void updateTerrainChunk(){
            float ViewrDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewPosition));
            bool visible = ViewrDstFromNearestEdge <= MaxViewDistance;
            SetVisibile(visible);
        }

        public void SetVisibile(bool visible) {
            meshObject.SetActive(visible);
        }

        public bool IsVisible(){
            return meshObject.activeSelf;
        }
    }
}
