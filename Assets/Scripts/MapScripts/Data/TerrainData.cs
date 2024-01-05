using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TerrainData : UpdatableData {
    public float Uniformscale = 5f;
    public bool UseFallOffMap;
    public bool UseFlatShading;
    public float MeshHeightMulitplier;
    public AnimationCurve MeshHeightCurve;
}
