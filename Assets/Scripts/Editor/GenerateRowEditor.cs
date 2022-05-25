using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GenerateRow))]
public class GenerateRowEditor : Editor
{
    private GenerateRow script;

    private void OnEnable()
    {
        script = (GenerateRow)target;
    }

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Generate"))
        {
            script.Generate();
        }

        if(GUILayout.Button("Clear"))
        {
            script.Clear();
        }

        base.OnInspectorGUI();
    }
}
