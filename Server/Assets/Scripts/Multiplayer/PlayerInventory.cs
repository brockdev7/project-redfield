using RiptideNetworking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private List<InventorySlot> inventorySlots;
    [SerializeField] private List<Item> list = new List<Item>();
    [SerializeField] public GameObject InventoryFrame;
    [SerializeField] public EventSystem eventSystem;

    public bool inventorySlotSelected { get { return inventorySlots.Any(s => s.isSelected); } }

    public int itemAmount { get; set; }
    public bool isPickingUpItem { get; set; }
    public ItemData isHolding { get; set; }
    private int _slotSize;


    public void AddToInventory(Item item)
    {
        Debug.Log($"Player added {item.itemData.itemName} to their inventory.");

        //Remove item from Item Spawner
        item.itemSpawner.ItemPickUp();

        //Add Item to Inventory & Slot
        var selectedSlot = GetSelectedSlot();

        if(selectedSlot)
        {
            //Slot Has Item
            if (selectedSlot.hasItem)
            {
                //Update Client-side spawner with the current item.
                //Add new item to slot
            }

            //Slot Doesn't Have Item
            if (!selectedSlot.hasItem)
            {
                //Get Next Available Slot
                var slot = GetNextAvailableSlot();
                slot.Set(item.itemData);
                
                isPickingUpItem = false;
                list.Add(item);
                AddedToInventory(player.Id, item.itemData.itemId, slot.id);
                player.Movement.EnableMovement();

                //Update other players inventory
                foreach (Player otherPlayer in player.otherPlayers)
                    UpdateInventory(player.Id, item.itemData.itemId, otherPlayer.Id,slot.id);
            }
        }
    }

    public void RemoveFromInventory(Item item)
    {
        Debug.Log($"Player removed {item.itemData.itemName} to their inventory.");
        list.Remove(item);
    }

    public void OnAwake()
    {
        itemAmount = 0;
        _slotSize = 4;

        //Set initial inventory size
        inventorySlots = new List<InventorySlot>(_slotSize);
        
        //Select first slot
        inventorySlots[0].Select();
        isPickingUpItem = false;
    }

    public void AttemptItemPickup(Item item)
    {
        OpenInventoryFrame();

        player.Movement.DisableMovement();
        isHolding = item.itemData;
        isPickingUpItem = true;

        PickingUpItem(item.itemData.itemId, item.itemSpawner.Id);
    }

    public void FixedUpdate()
    {
        if (!isPickingUpItem)
            return;

        if (player.Movement.inputDirection == Vector3.zero)
            return;

        MoveInventoryFrameSlot();
    }

    public InventorySlot GetSelectedSlot()
    {
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.isSelected)
                return slot;
        }

        return null;
    }

    public InventorySlot GetNextAvailableSlot()
    {
        foreach (InventorySlot slot in inventorySlots)
        {
            if (!slot.hasItem)
                return slot;
        }

        return null;
    }

    public void MoveInventoryFrameSlot()
    {
        var selectedSlot = GetSelectedSlot();
            
        InventorySlot topLeft = inventorySlots[0];
        InventorySlot topRight = inventorySlots[1];
        InventorySlot botLeft = inventorySlots[2];
        InventorySlot botRight = inventorySlots[3];

        if (selectedSlot == topLeft)
        {
            //Right
            if (player.Movement.inputDirection.x > 0)
            {
                topLeft.DeSelect();
                topRight.Select();
            }
                   
            //Down
            if (player.Movement.inputDirection.z < 0)
            {
                topLeft.DeSelect();
                botLeft.Select();
            }                 
        }

        if (selectedSlot == topRight)
        {
            //Left
            if (player.Movement.inputDirection.x < 0)
            {
                topRight.DeSelect();
                topLeft.Select();
            }
                    
            //Down
            if (player.Movement.inputDirection.z < 0)
            {
                topRight.DeSelect();
                botRight.Select();
            }
                  
        }

        if (selectedSlot == botLeft)
        {
            //Up
            if (player.Movement.inputDirection.z > 0)
            {
                botLeft.DeSelect();
                topLeft.Select();
            }
                    

            //Right
            if (player.Movement.inputDirection.x > 0)
            {
                botLeft.DeSelect();
                botRight.Select();
            }                  
        }

        if (selectedSlot == botRight)
        {
            //Up
            if (player.Movement.inputDirection.z > 0)
            {
                botRight.DeSelect();
                topRight.Select();
            }
                    
            //Left
            if (player.Movement.inputDirection.x < 0)
            {
                botRight.DeSelect();
                botLeft.Select();
            }
                   
        }
        
    }


    public void OpenInventoryFrame()
    {
        inventorySlots[0].Select();
        InventoryFrame.SetActive(true);    
    }

    public void CloseInventoryFrame()
    {
        InventoryFrame.SetActive(false);
    }



    #region Message Senders

    private void PickingUpItem(ushort _itemId, ushort _spawnerId)
    {
        var player = this.gameObject.GetComponent<Player>();

        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.playerPickingItemUp);
        message.AddUShort(player.Id);
        message.AddUShort(_itemId);
        message.AddUShort(_spawnerId);
        NetworkManager.Singleton.Server.Send(message, player.Id);
    }

    public void ExitItemPickup()
    {
        var player = this.gameObject.GetComponent<Player>();
        isPickingUpItem = false;

        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.playerExitItemPickUp);
        message.AddUShort(player.Id);
        NetworkManager.Singleton.Server.Send(message, player.Id);
    }

    private void AddedToInventory(ushort _playerId, ushort _itemId, ushort _slotId)
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.inventoryItemAdded);
        message.AddUShort(_playerId);
        message.AddUShort(_itemId);
        message.AddUShort(_slotId);
        NetworkManager.Singleton.Server.Send(message, _playerId);
    }

    private void UpdateInventory(ushort _playerId,ushort itemId,ushort toClientId, ushort _slotId)
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.inventoryUpdate);
        message.AddUShort(_playerId);
        message.AddUShort(itemId);
        message.AddUShort(_slotId);

        NetworkManager.Singleton.Server.Send(message,toClientId);
    }

    #endregion






}

