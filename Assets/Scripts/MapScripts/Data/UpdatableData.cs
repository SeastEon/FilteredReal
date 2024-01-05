using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdatableData : ScriptableObject {
    public event System.Action OnValuesupdated;
    public bool autoUpdate;

    protected virtual void OnValidate() {UnityEditor.EditorApplication.delayCall += _OnValidate;}
    private void _OnValidate(){ if (autoUpdate) { NotifyOfUpdatedValues(); }}
    public void NotifyOfUpdatedValues() { if(OnValuesupdated != null){OnValuesupdated();} }
}
