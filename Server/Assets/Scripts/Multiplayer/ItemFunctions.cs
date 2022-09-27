using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemCommands
{
    Equip = 1,
    Use,
    Combine,
    Present
}

public static class ItemFunctions 
{
    public static void Equip(Player player, ItemData itemData)
    {
        if (itemData.isEquipable)
        {
            //player.Inventory.EquipItem(itemData);
        }
    }

    public static void Use(Player player,ItemData itemData)
    {
        if(itemData.isUsable)
        {
            //player.Inventory.EquipItem(itemData);
        }
    }


    public static void Combine(Player player, ItemData itemData)
    {
        if (itemData.isCombinable)
        {

        }
    }




}
