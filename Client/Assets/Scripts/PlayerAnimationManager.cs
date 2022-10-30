using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerAnimationManager : MonoBehaviour
{
    public enum PlayerStance
    {
        Idle = 1,
        Attack
    }

    public enum PlayerLocomotionMode
    {
        Idle = 1,
        Walk,
        Run
    }

    public enum AnimatorLayers
    {
        Base,
        UpperBody
    }
    
    public enum RigLayers
    {
        AimRig 
    }

    [SerializeField] private Animator animator;
    [SerializeField] private Player player;
    [SerializeField] private Rig aimRig;

    [SerializeField] public bool InAttackStance { 
        get
        {
            return animator.GetBool("InAttackStance");
        } 
    }

    [Range(0, 1f)]
    [SerializeField] public float weightVelocity = 0f;

    [Range(0, 1f)]
    [SerializeField] public float animTransSmoothTime = 0.182f;

    [Range(0, 1f)]
    [SerializeField] public float aimRigSmoothTime = 0f;


    public void Awake()
    {
        aimRig.weight = 0f;
        animator = this.GetComponent<Animator>();    
    }


    public void Animate(float playerSpeed, PlayerStance playerStance, PlayerLocomotionMode mode, float xVel, float zVel)
    {
        SetPlayerSpeed(playerSpeed);
        SetPlayerStance(playerStance);
        SetPlayerLocomotionMode(mode);
        SetPlayerXVel(xVel);
        SetPlayerZVel(zVel);
    }

    public void SetPlayerLocomotionMode(PlayerLocomotionMode mode)
    {
        switch(mode)
        {
            case PlayerLocomotionMode.Idle:
                animator.SetBool("IsIdle", true);
                animator.SetBool("IsWalking", false);
                animator.SetBool("IsRunning", false);
                break;
            case PlayerLocomotionMode.Walk:
                animator.SetBool("IsIdle", false);
                animator.SetBool("IsWalking", true);
                animator.SetBool("IsRunning", false);
                break;
            case PlayerLocomotionMode.Run:
                animator.SetBool("IsIdle", false);
                animator.SetBool("IsWalking", false);
                animator.SetBool("IsRunning", true);
                break;
        }
   
    }
    public void SetPlayerStance(PlayerStance stance)
    {
        switch (stance)
        {
            case PlayerStance.Idle:
                LerpAimWeight(PlayerStance.Idle);
                animator.SetBool("InAttackStance", false);
                break;
            case PlayerStance.Attack:
                LerpAimWeight(PlayerStance.Attack);
                animator.SetBool("InAttackStance", true);
                break;
        }

        LerpUpperBodyWeight(stance);
    }

    public void LerpAimWeight(PlayerStance stance)
    {
        var currentAimRigWeight = aimRig.weight;
        currentAimRigWeight = Mathf.SmoothDamp(currentAimRigWeight, stance == PlayerStance.Attack ? 1f: 0, ref weightVelocity, aimRigSmoothTime);
        aimRig.weight = currentAimRigWeight;
    }
    public void LerpUpperBodyWeight(PlayerStance stance)
    {
        var currentUpperBodyWeight = animator.GetLayerWeight((int)AnimatorLayers.UpperBody);
        currentUpperBodyWeight = Mathf.SmoothDamp(currentUpperBodyWeight, stance == PlayerStance.Attack ? 1f : 0f, ref weightVelocity, animTransSmoothTime);
        animator.SetLayerWeight((int)AnimatorLayers.UpperBody, currentUpperBodyWeight);
    }

    public void SetPlayerSpeed(float playerSpeed)
    {
        animator.SetFloat("PlayerSpeed", playerSpeed);
    }

    public void SetPlayerXVel(float xVel)
    {
        animator.SetFloat("PlayerXVel", xVel);
    }

    public void SetPlayerZVel(float zVel)
    {
        animator.SetFloat("PlayerZVel", zVel);
    }


    public void EquipPistol()
    {
        animator.Play("EquipPistol");
    }

    public void EnterItemPickUp()
    {
        animator.Play("KneelDown");
    }

    public void ExitItemPickUp()
    {
        animator.Play("KneelUp");
    }

    [MessageHandler((ushort)ServerToClientId.animate)]
    private static void Animate(Message message)
    {
        if (Player.list.TryGetValue(message.GetUShort(), out Player player))
        {
            player.animationManager.Animate(message.GetFloat(), (PlayerStance)message.GetInt(), (PlayerLocomotionMode)message.GetInt(), message.GetFloat(), message.GetFloat());
        }
    }
}

