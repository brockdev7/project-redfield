using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationManager : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float playerSpeed;

    public void Animate(float _playerSpeed)
    {
        playerSpeed = _playerSpeed;
        animator.SetFloat("PlayerSpeed", _playerSpeed);
    }

    public void EnterItemPickUp()
    {
        var animator = this.GetComponent<Animator>();
        animator.Play("KneelDown");
    }

    public void ExitItemPickUp()
    {
        var animator = this.GetComponent<Animator>();
        animator.Play("KneelUp");
    }


}
