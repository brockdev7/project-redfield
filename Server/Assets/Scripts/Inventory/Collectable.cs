using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public abstract class Collectable : MonoBehaviour, ICollectable
{
    public string DisplayName;
    public int ItemId;
    public abstract void Collect();
    public static event Action OnItemCollected;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player _player = other.GetComponent<Player>();

            if(!_player.isPickingUpItem)
            {
                _player.AttemptItemPickup();
            }
            
            if(_player.isPickingUpItem)
            {
                if (_player.Movement.isPressed("Esc"))
                {
                    _player.ExitItemPickupAnim(ItemId);
                    Collect();
                    OnItemCollected?.Invoke();
                }
            }
        }
    }
}
