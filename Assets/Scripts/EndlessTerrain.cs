using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TerrainUtils;
using UnityEngine.UIElements;
using static EndlessTerrain;

public class EndlessTerrain : MonoBehaviour {

    const float viewrMoveThseeholdForChunkUpdate = 25f;
    const float sqrviewrMoveThseeholdForChunkUpdate = viewrMoveThseeholdForChunkUpdate * viewrMoveThseeholdForChunkUpdate;

    public lodInfo[] detailLevels;
    public static float MaxViewDistance = 450;

    const float Scale = 1f;

    public Transform viewer;
    public Material mapMaterial;

    public static Vector2 viewPosition;
    Vector2 viewPositionold;

    static MapGenerator mapGenerator; //reference to mapGenerator class

    int chunkSize;
    int chunksVisibleInViewDistance;

    Dictionary<Vector2, TerrainChunks> chunksDictionary = new Dictionary<Vector2, TerrainChunks>();
    static List<TerrainChunks> terrainChunksVisibleLastUpdate = new List<TerrainChunks>();

    void Start(){
        mapGenerator = FindObjectOfType<MapGenerator>();
        MaxViewDistance = detailLevels[detailLevels.Length - 1].VisbleDstThreshold;
        chunkSize = MapGenerator.MapChunkSize - 1;
        chunksVisibleInViewDistance = Mathf.RoundToInt(MaxViewDistance / chunkSize);
        UpdateVisibileChunks();
    }

     void Update(){
        viewPosition = new Vector2(viewer.position.x, viewer.position.z) / Scale;
        if((viewPositionold - viewPosition).sqrMagnitude > sqrviewrMoveThseeholdForChunkUpdate)
        {
            viewPositionold = viewPosition;
            UpdateVisibileChunks();
        }
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

                } else {
                    chunksDictionary.Add(viewedChunkCoord, new TerrainChunks(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial));
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
        
        lodInfo[] detailLevels;
        LODMesh[] lodMeshes;

        MapData mapData;
        bool mapdataRecieved;
        int previousLODIndex = -1;

        public TerrainChunks(Vector2 coord, int size, lodInfo[] detailLevels, Transform parent, Material material ) {
            this.detailLevels = detailLevels;
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 position3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Terrain chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer.material = material;

            meshObject.transform.position = position3 * Scale;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = position3 * Scale;
            SetVisibile(false);


            lodMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++)
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod, updateTerrainChunk);
            }

            mapGenerator.RequestMapData(position, OnMapdataRecieved);
        }


        void OnMapdataRecieved(MapData mapData) {
            this.mapData = mapData;
            mapdataRecieved = true;
            Texture2D texture = TextureGenerator.TextureFromColormap(mapData.Colormap, MapGenerator.MapChunkSize, MapGenerator.MapChunkSize);
            meshRenderer.material.mainTexture = texture;
            updateTerrainChunk();
        }

        public void updateTerrainChunk()            
        {
            if (mapdataRecieved)
            {
                float ViewrDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewPosition));
                bool visible = ViewrDstFromNearestEdge <= MaxViewDistance;

                if (visible) {
                    int lodIndex = 0;

                    for (int i = 0; i < detailLevels.Length - 1; i++){
                        if (ViewrDstFromNearestEdge > detailLevels[i].VisbleDstThreshold){lodIndex = i + 1;}
                        else { break; }
                    }

                    if (lodIndex != previousLODIndex) {
                        LODMesh lodMesh = lodMeshes[lodIndex];
                        if (lodMesh.hasMesh){
                            previousLODIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;
                        }
                        else if (!lodMesh.hasRequestedMesh){
                            lodMesh.RequestMesh(mapData);
                        }
                    }
                    terrainChunksVisibleLastUpdate.Add(this);
                }
                SetVisibile(visible);
            }
        }

        public void SetVisibile(bool visible)
        {
            meshObject.SetActive(visible);
        }


        public bool IsVisible(){
            return meshObject.activeSelf;
        }
    }

    public class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;
        System.Action updateCallback;

        public LODMesh(int lod, System.Action updateCallback) {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        void OnMeshdataRecieved(MeshData meshdata) {
            mesh = meshdata.CreateMesh();
            hasMesh = true;
            updateCallback();
        }

        public void RequestMesh(MapData mapdata){
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapdata, lod, OnMeshdataRecieved);
        }
    }

    [System.Serializable]
    public struct lodInfo {
        public int lod;
        public float VisbleDstThreshold;
    }

}
