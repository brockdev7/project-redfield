using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string username;
    public float health;
    public float maxHealth = 100f;

    private Animator animator;

    [SerializeField]
    private float playerSpeed;

    public int itemCount = 0;
    public MeshRenderer model;

    public Quaternion prevPlayerRotation;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }


    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        health = maxHealth;
        playerSpeed = 0f;
        prevPlayerRotation = transform.rotation;
    }

    public void SetHealth(float _health)
    {
        health = _health;

        if (health <= 0f)
        {
            PlayerDied();
        }
    }

    public void SetPlayerSpeed(float _playerSpeed)
    {
        playerSpeed = _playerSpeed;
        animator.SetFloat("PlayerSpeed", _playerSpeed);
    }

    public void PlayerDied()
    {
        model.enabled = false;
    }

    public void Respawn()
    {
        model.enabled = true;
        SetHealth(maxHealth);
    }



}
