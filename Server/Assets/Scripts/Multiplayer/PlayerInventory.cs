using Assets.Scripts.Multiplayer;
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
                //Place item in next available slot
                var slot = GetNextAvailableSlot();
                slot.Set(item);
                
                //Add to inventory list
                list.Add(item);

                //Update Client Inventory
                AddedToInventory(player.Id, item.itemData.itemId, slot.id);
                player.Movement.EnableMovement();

                //Remove item from Item Spawner
                item.itemSpawner.ItemPickUp();

                ////Update teammate inventories
                //foreach (Player otherPlayer in player.otherPlayers)
                //    UpdateInventory(player.Id, item.itemData.itemId, otherPlayer.Id,slot.id);

                isPickingUpItem = false;
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

    public InventorySlot GetInventorySlot(ushort slotId)
    {
        foreach(var slot in inventorySlots)
        {
            if (slot.id == slotId)
                return slot;
        }

        return null;
    }

    #region Action Menu

    public static void ActionMenu_Use(ushort fromClientId, ushort slotId)
    {
        if (Player.list.TryGetValue(fromClientId, out Player player))
        {
            var slot = player.Inventory.GetInventorySlot(slotId);

            if (slot.hasItem && slot.item)
            {
                if (slot.item.itemData.isUsable)
                    slot.item.Use();
                else
                    Debug.Log($"{player.name} attempted to use an item that isn't usable.");
            }
        }
    }

    public static void ActionMenu_Equip(ushort fromClientId, ushort slotId)
    {
        if (Player.list.TryGetValue(fromClientId, out Player player))
        {
            var slot = player.Inventory.GetInventorySlot(slotId);

            if (slot.hasItem && slot.item)
            {
                if (slot.item.itemData.isEquipable)
                    slot.item.Equip(player.Id,slot.item.itemData.itemId,slot.id);
                else
                    Debug.Log($"{player.name} attempted to equip an item that isn't equippable.");
            }
        }
    }

    #endregion

    #region Message Senders

    private void PickingUpItem(ushort _itemId, ushort _spawnerId)
    {
        var player = this.gameObject.GetComponent<Player>();

        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.playerPickingItemUp);
        message.AddUShort(player.Id);
        message.AddUShort(_itemId);
        message.AddUShort(_spawnerId);
        NetworkManager.Singleton.Server.SendToAll(message);
    }

    public void ExitItemPickup()
    {
        var player = this.gameObject.GetComponent<Player>();
        isPickingUpItem = false;

        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.playerExitItemPickUp);
        message.AddUShort(player.Id);
        NetworkManager.Singleton.Server.SendToAll(message);
    }

    private void AddedToInventory(ushort _playerId, ushort _itemId, ushort _slotId)
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.inventoryItemAdded);
        message.AddUShort(_playerId);
        message.AddUShort(_itemId);
        message.AddUShort(_slotId);
        NetworkManager.Singleton.Server.SendToAll(message);
    }

    #endregion



    #region Message Handlers 

    [MessageHandler((ushort)ClientToServerId.actionMenu_Use)]
    private static void ActionMenuUse(ushort fromClientId, Message message)
    {
        ActionMenu_Use(fromClientId, message.GetUShort());
    }


    [MessageHandler((ushort)ClientToServerId.actionMenu_Equip)]
    private static void ActionMenuEquip(ushort fromClientId, Message message)
    {
        ActionMenu_Equip(fromClientId, message.GetUShort());
    }


    #endregion





}

