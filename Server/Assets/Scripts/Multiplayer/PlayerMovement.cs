using RiptideNetworking;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    //Camera
    public Camera currentCamera;

    [SerializeField] private Player player;
    [SerializeField] private CharacterController controller;
    [SerializeField] private float gravity;
    [SerializeField] public float playerAnimSpeed;
    [SerializeField] public float walkSpeedModifier = 1f;
    [SerializeField] public float runSpeedModifier = 3f;
    [SerializeField] public float throttle = 0f;
    [SerializeField] public GameObject lookDirection;
    [SerializeField] public bool debugAim;

    [SerializeField]
    public Quaternion targetRotation;

    //Player Movement
    private float gravityAcceleration;
    public float groundAccel = 10f;
    public float groundDecel = 6f;
    public float moveSpeed = 3f;
    public float rotationSpeed = 3f;
    public float maxForwardSpeed = 3f;
    public Vector3 inputDirection;

    float desiredSpeed;
    float acceleration;
    private bool didTeleport;

    private bool[] inputs;
  
    private bool freezeMovement = false;
    private float yVelocity;

    Dictionary<string, bool> InputMap = new Dictionary<string, bool>();

    //Locomotion State
    public bool isIdle
    {
        get 
        {
            return !isWalking && !isRunning;
        }
    }
    public bool isWalking
    {
        get
        {
            if ((InputMap["W"] || InputMap["A"] || InputMap["S"] || InputMap["D"]) && !InputMap["LShift"])
                return true;
            else
                return false;
        }
    }
    public bool isRunning
    {
        get
        {
            if ((InputMap["W"] || InputMap["A"] || InputMap["S"] || InputMap["D"]) && InputMap["LShift"])
                return true;
            else
                return false;
        }
    }
    public bool isAttacking
    {
        get
        {
            if (debugAim)
                return true;

            if (InputMap["RightClick"])
                return true;
            else
                return false;
        }
    }


    private void OnValidate()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();

        if (player == null)
            player = GetComponent<Player>();

        Initialize();
    }

    private void Start()
    {
        Initialize();
        inputs = new bool[11];
        currentCamera = Camera.main;
        debugAim = false;

        gravityAcceleration = gravity * Time.fixedDeltaTime * Time.fixedDeltaTime;
    }

    private void FixedUpdate()
    {
        if (lookDirection)
            Debug.DrawRay(lookDirection.transform.position, lookDirection.transform.forward, Color.green);

        UpdateKeyMap(inputs);

        inputDirection = Vector3.zero;

        if (InputMap["W"])
            inputDirection.z += 1;

        if (InputMap["A"])
            inputDirection.z -= 1;

        if (InputMap["S"])
            inputDirection.x -= 1;

        if (InputMap["D"])
            inputDirection.x += 1;

        if (isWalking)
        {
            if (player.LocomotionMode != PlayerLocomotionMode.Walk)
                player.SetLocomotionMode(PlayerLocomotionMode.Walk);
        }
        else if (isRunning)
        {
            if (player.LocomotionMode != PlayerLocomotionMode.Run)
                player.SetLocomotionMode(PlayerLocomotionMode.Run);
        }
        else if(playerAnimSpeed == 0)
        {
            if (player.LocomotionMode != PlayerLocomotionMode.Idle)
                player.SetLocomotionMode(PlayerLocomotionMode.Idle);
        }

        if (isAttacking)
        {
            if(player.PlayerStance != PlayerStance.Attack)
                player.SetAttackStance(PlayerStance.Attack);
        }
        else if(!isAttacking)
        {
            player.SetAttackStance(PlayerStance.Idle);
        }
            
        Animate();
        Move(inputDirection);
    }

    public bool isPressed(string button)
    {
        if (InputMap.TryGetValue(button, out bool isPressed))
            return isPressed;
        else
            return false;
    }

    private void Initialize()
    {
        InputMap = new Dictionary<string, bool>();
    }

    public void DisableMovement()
    {
        freezeMovement = true;
    }

    public void EnableMovement()
    {
        freezeMovement = false;
    }

    private void Move(Vector3 _inputDirection)
    {
        if (freezeMovement)
            return;

        if (_inputDirection.sqrMagnitude > 1f)
            _inputDirection.Normalize();

        float xInput = _inputDirection.x;
        float zInput = _inputDirection.z;

        //Calculate Camera Right/Forward Axis
        Vector3 right = currentCamera.transform.right;
        Vector3 forward = Vector3.Cross(right, Vector3.up);
        forward.y = 0;

        //Calculate forward/right movement * moveSpeed modifier
        Vector3 movement = Vector3.zero;

        //If Running/Aiming
        if (isRunning && isAttacking)
            moveSpeed = walkSpeedModifier;

        if (isRunning && !isAttacking)
            moveSpeed = runSpeedModifier;

        if (isWalking)
            moveSpeed = walkSpeedModifier;
       
        movement += right * (xInput * moveSpeed * Time.deltaTime);
        movement += forward * (zInput * moveSpeed * Time.deltaTime);

        if (controller.isGrounded)
        {
            yVelocity = 0f;

            //Rotate players towards input direction relative to camera position.
            if (movement != Vector3.zero && !isAttacking)
            {
                targetRotation = Quaternion.LookRotation(movement, Vector3.up);
                controller.transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            //Calculate Desired Player Animation Speed
            maxForwardSpeed = isRunning ? 5 : 3;
            desiredSpeed = _inputDirection.magnitude * maxForwardSpeed;
            acceleration = isIdle ? groundDecel : groundAccel;
            playerAnimSpeed = Mathf.MoveTowards(playerAnimSpeed, desiredSpeed, acceleration * Time.deltaTime);
        }

        yVelocity += gravityAcceleration;
        movement.y = yVelocity;

        controller.Move(movement);

        SendMovement();
    }

    public void UpdateKeyMap(bool[] inputs)
    {
        InputMap = new Dictionary<string, bool>()
        {
            { "W", inputs[0] },
            { "A", inputs[1] },
            { "S", inputs[2] },
            { "D", inputs[3] },
            { "E", inputs[4] },
            { "LShift", inputs[5] },
            { "Esc", inputs[6] },
            { "Space", inputs[7] },
            { "Return", inputs[8] },
            { "RightClick", inputs[9] },
            { "RightClickUp", inputs[10] }
        };

    }

    public void SetInput(bool[] inputs)
    {
        this.inputs = inputs;
    }

    #region Message Senders
    private void SendMovement()
    {
        //Only send movement data on server's 3rd tick (reduce data being sent)
        if (NetworkManager.Singleton.CurrentTick % 3 != 0)
            return;

        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.playerMovement);
        message.AddUShort(player.Id);
        message.AddUShort(NetworkManager.Singleton.CurrentTick);
        message.AddVector3(transform.position);
        message.AddQuaternion(transform.rotation);

        NetworkManager.Singleton.Server.SendToAll(message);
    }
    #endregion

    private void Animate()
    {
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.animate);
        message.AddUShort(player.Id);
        message.AddFloat(playerAnimSpeed);
        message.AddInt((int)player.PlayerStance);
        message.AddInt((int)player.LocomotionMode);
        message.AddFloat(inputDirection.x);
        message.AddFloat(inputDirection.z);

        NetworkManager.Singleton.Server.SendToAll(message);
    }


    #region Message Handlers
    [MessageHandler((ushort)ClientToServerId.input)]
    private static void Input(ushort fromClientId, Message message)
    {
        if (Player.list.TryGetValue(fromClientId, out Player player))
        {
            player.Movement.SetInput(message.GetBools(11));
        }
    }

    #endregion
}
