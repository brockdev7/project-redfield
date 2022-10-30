using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryAudio : MonoBehaviour
{
    public AudioSource audioSource;
    [SerializeField] public AudioClip actionMenuOpen;
    [SerializeField] public AudioClip actionMenuClose;
    [SerializeField] public AudioClip inventorySlotMove;

    public void PlayActionMenuOpen()
    {
        audioSource.clip = actionMenuOpen;
        audioSource.Play();
    }

    public void PlayActionMenuClose()
    {
        audioSource.clip = actionMenuClose;
        audioSource.Play();
    }

    public void PlayInventorySlotMove()
    {
        audioSource.clip = inventorySlotMove;
        audioSource.Play();
    }


}
