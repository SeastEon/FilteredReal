using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class MeshSettings : UpdatableData {
    public const int numsupportedLOD = 5;
    public const int numsupportedChunkSizes = 9;
    public const int numsupportedFlatShadedChunkSizes = 3;
    public static readonly int[] supportedChunkSizes = { 40, 72, 96, 120, 144, 168, 192, 216, 240 };

    public float MeshScale = 5f;
    public bool UseFlatShading;

    [Range(0, numsupportedChunkSizes - 1)]
    public int ChunkSizeIndex;
    [Range(0, numsupportedFlatShadedChunkSizes - 1)]
    public int FlatShadedChunkSizeIndex;

    //number of verts per line of mesh rendered at LOD = 0. Includes two extrea verts that are excluded from final mesh but userd for calculating normals
    public int NumVertsPerLine  {
        get {  return supportedChunkSizes[(UseFlatShading) ? FlatShadedChunkSizeIndex : ChunkSizeIndex] +5; }
    }

    public float MeshWorldSize {
        get { return NumVertsPerLine - 3 * MeshScale; }
    }
}
