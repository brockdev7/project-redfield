using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu]
public class ItemData : ScriptableObject
{
    public ushort itemId;
    public string itemName;
    public string itemDescription;
    public GameObject renderModel;
    public GameObject spawnerModel;
    public Sprite icon;
    public bool isEquippable;
    public bool isUsable;
    public bool isCombinable;
    public bool isTradable;
}
