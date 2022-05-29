using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PuzzleCreator))]
public class PuzzleGenerateButton : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        PuzzleCreator creator = (PuzzleCreator)target;
        if(GUILayout.Button("½½·Ô »ö±ò ·£´ý"))
        {
            creator.AllSlotsRandomColor();
        }
    }
}
