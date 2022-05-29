using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SlotGenerator))]
public class SlotGenerateButton : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SlotGenerator generator = (SlotGenerator)target;
        if(GUILayout.Button("���� ����_��"))
        {
            generator.Generate(Slot.Direction.Up);
        }
        if (GUILayout.Button("���� ����_������_��"))
        {
            generator.Generate(Slot.Direction.Up_Right);
        }
        if (GUILayout.Button("���� ����_������_�Ʒ�"))
        {
            generator.Generate(Slot.Direction.Down_Right);
        }
        if (GUILayout.Button("���� ����_�Ʒ�"))
        {
            generator.Generate(Slot.Direction.Down);
        }
        if (GUILayout.Button("���� ����_����_�Ʒ�"))
        {
            generator.Generate(Slot.Direction.Down_Left);
        }
        if (GUILayout.Button("���� ����_����_��"))
        {
            generator.Generate(Slot.Direction.Up_Left);
        }
    }
}
