using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(InventorySlot))]
public class InventorySlotEditor : UnityEditor.UI.ButtonEditor
{
    public override void OnInspectorGUI()
    {
        InventorySlot targetSlot = (InventorySlot)target;
        DrawDefaultInspector();
    }
}
