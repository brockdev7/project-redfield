using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using RiptideNetworking;

public class Item : MonoBehaviour
{
    public static event Action OnItemCollected;

    [SerializeField] public ItemData itemData;
    [SerializeField] public ItemSpawner itemSpawner;

    public void Awake()
    {
        itemSpawner = GetComponentInParent<ItemSpawner>();
    }

    public void Collect()
    {
        OnItemCollected?.Invoke();
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

                    _player.Inventory.isHolding = null;
                    _player.Inventory.CloseInventoryFrame();
                    _player.Inventory.ExitItemPickup();                    
                }

               
            }

        } 
    }



}
