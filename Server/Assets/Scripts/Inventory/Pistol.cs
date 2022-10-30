using RiptideNetworking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Pistol : Item
{
    public override void Equip(ushort playerId,ushort itemId, ushort slotId)
    {
        Debug.Log("Equipping a pistol");
        PistolEquipped(playerId, itemId, slotId);
    }

    public override void Use()
    {
        Debug.Log("Using a Green Herb");
    }

    public override void Combine()
    {
        Debug.Log("Combining a Green Herb");
    }

    public override void Present()
    {
        Debug.Log("Presenting a Green Herb");
    }


    #region Message Senders
    private void PistolEquipped(ushort _playerId, ushort _itemId, ushort _slotId)
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.inventoryItemPistolEquipped);
        message.AddUShort(_playerId);
        //message.AddUShort(_itemId);
        //message.AddUShort(_slotId);
        NetworkManager.Singleton.Server.SendToAll(message);
    }

    #endregion
}

