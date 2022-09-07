using RiptideNetworking;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private bool[] inputs;
    public bool movementDisabled = false;

    private void Start()
    {
        inputs = new bool[9];
    }

    private void Update()
    {
        if(!movementDisabled)
        {
            if (Input.GetKey(KeyCode.W))
                inputs[0] = true;

            if (Input.GetKey(KeyCode.S))
                inputs[1] = true;

            if (Input.GetKey(KeyCode.A))
                inputs[2] = true;

            if (Input.GetKey(KeyCode.D))
                inputs[3] = true;
        }

        if (Input.GetKey(KeyCode.E))
            inputs[4] = true;

        if (Input.GetKey(KeyCode.LeftShift))
            inputs[5] = true;

        if (Input.GetKey(KeyCode.Escape))
            inputs[6] = true;

        if (Input.GetKey(KeyCode.Space))
            inputs[7] = true;

        if (Input.GetKey(KeyCode.Return))
            inputs[8] = true;

        if (Input.GetKeyUp(KeyCode.Tab))
        {
            //Must be in View mode to toggle at will (prevent tabbing out of picking up item)
            if(UIManager.Singleton.inventoryMode == (int)UIManager.InventoryMode.view)
                ToggleInventoryScreen();
        }
    }

    private void FixedUpdate()
    {
        SendInput();

        for (int i = 0; i < inputs.Length; i++)
            inputs[i] = false;
    }

    public void ToggleInventoryScreen()
    {
        if (UIManager.Singleton.inventoryScreenIsActive)
        {
            UIManager.Singleton.CloseInventoryScreen();
            movementDisabled = false;
        }
        else
        {
            UIManager.Singleton.OpenInventoryScreen();
            movementDisabled = true;
        }
    }

    #region Messages
    private void SendInput()
    {
        Message message = Message.Create(MessageSendMode.unreliable, ClientToServerId.input);
        message.AddBools(inputs, false);
        NetworkManager.Singleton.Client.Send(message);
    }

    #endregion
}