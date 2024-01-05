using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour {
    public Renderer textureRender;
    public MeshFilter meshfilter;
    public MeshRenderer meshRenderer;


    public void DrawTexture(Texture2D texture) {
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3 (texture.width, 1, texture.height);
    }

    public void DrawMesh(Meshdata meshdata, Texture2D texture) {
        meshfilter.sharedMesh = meshdata.CreateMesh();
        meshRenderer.sharedMaterial.mainTexture = texture;
        meshfilter.transform.localScale = Vector3.one * FindAnyObjectByType<MapGenerator>().terrainData.Uniformscale;
    }   
}
