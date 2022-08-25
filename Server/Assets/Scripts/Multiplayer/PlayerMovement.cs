using Assets.Scripts.Multiplayer;
using RiptideNetworking;
using System.Collections;
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
    [SerializeField] public float playerSpeed;
    [SerializeField] public float walkSpeedModifier = 1f;
    [SerializeField] public float runSpeedModifier = 2.5f;

    [SerializeField]
    public Quaternion targetRotation;

    //Player Movement
    private float gravityAcceleration;
    public float groundAccel = 10f;
    public float groundDecel = 5f;
    public float moveSpeed = 3f;
    public float rotationSpeed = 3f;
    public float maxForwardSpeed = 3f;
    float desiredSpeed;
    float acceleration;
    private bool didTeleport;

    private bool[] inputs;
    private bool freezeMovement = false;
    private float yVelocity;


    Dictionary<string, bool> InputMap = new Dictionary<string, bool>();

    //State
    public bool isMoving
    {
        get { return !inputs.All(x => !x); }
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

    bool isRunning
    {
        get
        {
            if ((InputMap["W"] || InputMap["A"] || InputMap["S"] || InputMap["D"]) && InputMap["LShift"])
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
        inputs = new bool[7];
        currentCamera = Camera.main;
        gravityAcceleration = gravity * Time.fixedDeltaTime * Time.fixedDeltaTime;
    }

    private void FixedUpdate()
    {     
        UpdateKeyMap(inputs);

        Vector3 inputDirection = Vector3.zero;

        if (InputMap["W"])
            inputDirection.z += 1;

        if (InputMap["A"])
            inputDirection.z -= 1;

        if (InputMap["S"])
            inputDirection.x -= 1;

        if (InputMap["D"])
            inputDirection.x += 1;

        Move(inputDirection);

        var lookDirection = transform.Find("LookDirection");
        Debug.DrawRay(lookDirection.position, transform.forward, Color.green);
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

        float xInput = _inputDirection.x;
        float zInput = _inputDirection.z;

        if (_inputDirection.sqrMagnitude > 1f)
            _inputDirection.Normalize();

        //Calculate Camera Right/Forward Axis
        Vector3 right = currentCamera.transform.right;
        Vector3 forward = Vector3.Cross(right, Vector3.up);
        forward.y = 0;

        //Calculate forward/right movement * moveSpeed modifier
        Vector3 movement = Vector3.zero;
        movement += right * (xInput * moveSpeed * Time.deltaTime);
        movement += forward * (zInput * moveSpeed * Time.deltaTime); 

        if (controller.isGrounded)
        {
            yVelocity = 0f;

            //TODO: Update last known player rotation/target rotation to the client before the lerp server side
            //Rotate players towards input direction relative to camera position.
            if (movement != Vector3.zero)
            {
                targetRotation = Quaternion.LookRotation(movement, Vector3.up);                
                controller.transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            //Affect desired speed based on player state
            if (isWalking)
            {
                //Calculate Desired Player Animation Speed
                desiredSpeed = _inputDirection.magnitude * maxForwardSpeed * 1;
                acceleration = isMoving ? groundAccel : groundDecel;
                playerSpeed = Mathf.MoveTowards(playerSpeed, desiredSpeed, acceleration * Time.deltaTime);

                moveSpeed = walkSpeedModifier;
            }

            if (isRunning)
            {
                //Calculate Desired Player Animation Speed
                desiredSpeed = _inputDirection.magnitude * maxForwardSpeed * 2;
                acceleration = isMoving ? groundAccel : groundDecel;
                playerSpeed = Mathf.MoveTowards(playerSpeed, desiredSpeed, acceleration * Time.deltaTime);

                moveSpeed = runSpeedModifier;
            }

            if (!isWalking && !isRunning)
                playerSpeed = Mathf.MoveTowards(playerSpeed, 0f, groundDecel * Time.deltaTime);


        }

        yVelocity += gravityAcceleration;
        movement.y = yVelocity;

        controller.Move(movement);

        SendMovement();
        //SendRotation();

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
            { "Esc", inputs[6] }
        };
    }

    public void SetInput(bool[] inputs)
    {
        this.inputs = inputs;
    }

    private void SendMovement()
    {
        //Only send movement data on server's 2nd tick (reduce data being sent)
        if (NetworkManager.Singleton.CurrentTick % 2 != 0)
            return;

        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.playerMovement);
        message.AddUShort(player.Id);
        message.AddUShort(NetworkManager.Singleton.CurrentTick);
        message.AddVector3(transform.position);
        message.AddQuaternion(transform.rotation);
        message.AddFloat(playerSpeed);
        NetworkManager.Singleton.Server.SendToAll(message);
    }

    private void SendRotation()
    {
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.playerRotation);
        message.AddUShort(player.Id);
        message.AddQuaternion(transform.rotation);
        NetworkManager.Singleton.Server.SendToAll(message);
    }

}
