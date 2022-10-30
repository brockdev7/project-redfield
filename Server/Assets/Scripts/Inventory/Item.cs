using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using RiptideNetworking;

public class Item : MonoBehaviour
{
    [SerializeField] public ItemData itemData;
    [SerializeField] public ItemSpawner itemSpawner;

    public virtual void Equip(ushort playerId, ushort itemId, ushort slotId) { }
    public virtual void Use() { }
    public virtual void Combine() { }
    public virtual void Present() { }

    public void Awake()
    {
        itemSpawner = GetComponentInParent<ItemSpawner>();
    }

    private void OnTriggerStay(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player _player = collision.GetComponent<Player>();

            if (!_player.Inventory.isPickingUpItem)
            {
                if (_player.Movement.isPressed("E"))
                {
                    if(itemSpawner.hasItem)
                        _player.Inventory.AttemptItemPickup(this);
                }
            }

            if (_player.Inventory.isPickingUpItem)
            {
                //Accept Item
                if (_player.Movement.isPressed("Return"))
                {
                    //ItemSpawner has item
                    if (itemSpawner.hasItem)
                    {
                        _player.Inventory.AddToInventory(this);
                        _player.Inventory.CloseInventoryFrame();
                    }
                }

                //Decline Item
                if (_player.Movement.isPressed("Esc"))
                {
                    _player.Movement.EnableMovement();

                    _player.Inventory.CloseInventoryFrame();
                    _player.Inventory.ExitItemPickup();                    
                }

               
            }

        } 
    }



}
