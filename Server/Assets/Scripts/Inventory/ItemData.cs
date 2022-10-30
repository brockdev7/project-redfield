using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ItemData : ScriptableObject
{
    public ushort itemId;
    public string itemName;
    public bool isEquipable;
    public bool isUsable;
    public bool isCombinable;
    public bool isTradable;
}


