using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI() {
        MapGenerator MapGen = (MapGenerator)target; 
        if(DrawDefaultInspector()) {
            if (MapGen.Autoupdate){MapGen.DrawMapInEditor();}
        }

        if(GUILayout.Button("Generate")){MapGen.DrawMapInEditor();}
    }
}
