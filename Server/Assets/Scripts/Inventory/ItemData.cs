using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ItemData : ScriptableObject
{
    public ushort itemId;
    public string itemName;
    public bool isUsable;
    public bool isEquipable;
    public bool isCombinable;
}


