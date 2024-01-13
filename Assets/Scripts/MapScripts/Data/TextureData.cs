using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu]
public class TextureData : UpdatableData {

    const int TextureSize = 512;
    const TextureFormat textureFormat = TextureFormat.RGB565;

    public Layer[] layers;
    float savedMinHeight;
    float savedMiaxeight;

    public void ApplyToMaterial(Material material) {
        material.SetInt("layersCount", layers.Length);
        material.SetColorArray("BaseColors", layers.Select(x => x.tint).ToArray());
        material.SetFloatArray("BaseStartHeights", layers.Select(x => x.startHeight).ToArray());
        material.SetFloatArray("BaseBlends", layers.Select(x => x.blendStrength).ToArray());
        material.SetFloatArray("BaseColorStrength", layers.Select(x => x.tintStrength).ToArray());
        material.SetFloatArray("BaseTextureScales", layers.Select(x => x.textureScale).ToArray());
        Texture2DArray TextureArray = GenerateTexture2D(layers.Select(x => x.texture).ToArray());
        material.SetTexture("BaseTextures", TextureArray);
        UpdateMeshHeights(material, savedMinHeight, savedMiaxeight);
    }

    public void UpdateMeshHeights(Material material, float minHeight, float maxHeight) {
        savedMinHeight = minHeight;
        savedMiaxeight = maxHeight;
        material.SetFloat("minHeight", minHeight);
        material.SetFloat("maxHeight", maxHeight);
    }

    Texture2DArray GenerateTexture2D(Texture2D[] textures) {
        Texture2DArray textureArray = new Texture2DArray(TextureSize, TextureSize, textures.Length, textureFormat, true);
        for (int i = 0; i < textures.Length; i++) { textureArray.SetPixels(textures[i].GetPixels(), i); }
        textureArray.Apply();
        return textureArray;
    }

     [System.Serializable]
    public class Layer {
        public Texture2D texture;
        public Color tint;
        [Range(0, 1)]
        public float tintStrength;
        [Range(0, 1)]
        public float startHeight;
        [Range(0, 1)]
        public float blendStrength;
        public float textureScale;
    }
}
