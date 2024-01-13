using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class HeightMapSettings : UpdatableData {

    public NoiseSettings noiseSettings;
 
    public bool UseFallOffMap;
    public float HeightMult;
    public AnimationCurve HeightCurve;

    public float minHeight
    {
        get { return HeightMult * HeightCurve.Evaluate(0); }
    }

    public float maxHeight
    {
        get { return  HeightMult * HeightCurve.Evaluate(1); }
    }


#if UNITY_EDITOR
    protected override void OnValidate() {
        noiseSettings.ValidateValues();
        base.OnValidate();
    }
#endif
}

