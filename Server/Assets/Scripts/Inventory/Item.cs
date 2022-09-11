using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using RiptideNetworking;

public class Item : MonoBehaviour
{
    public static event Action OnItemCollected;
    [SerializeField] public ItemData itemData;

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
                    _player.Inventory.AttemptItemPickup(_player,this);
                }
            }

            if (_player.Inventory.isPickingUpItem)
            {
                if (_player.Movement.isPressed("Esc"))
                {
                    _player.Movement.EnableMovement();
                    _player.Inventory.ExitItemPickup();
                }

                if (_player.Movement.isPressed("Return"))
                {
                    _player.Inventory.AddToInventory(_player.Id,this);
                    _player.Movement.EnableMovement();
                }
            }

        } 
    }



}
