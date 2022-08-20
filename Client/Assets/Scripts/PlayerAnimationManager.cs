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

}
