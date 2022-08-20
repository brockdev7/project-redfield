using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager _singleton;

    public static UIManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(UIManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    public void Awake()
    {
        Singleton = this;
    }

    [Header("Connect")]
    [SerializeField] private GameObject connectUI;
    [SerializeField] private InputField usernameField;

    [Header("Inventory")]
    [SerializeField] private GameObject inventoryUI;

    //------------ CONNECT SCREEN ------------\\
    public void ConnectClicked()
    {
        usernameField.interactable = false;
        connectUI.SetActive(false);

        NetworkManager.Singleton.Connect();
    }

    public void BackToMain()
    {
        usernameField.interactable = true;
        connectUI.SetActive(true);
    }

    //------------ INVENTORY SCREEN ------------\\

    public void OpenInventoryScreen()
    {
        inventoryUI.SetActive(true);
    }

    public void CloseInventoryScreen()
    {
        inventoryUI.SetActive(false);
    }


    #region Message Senders
    public void LogIn()
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.playerLoggingIn);
        message.AddString(usernameField.text);
        NetworkManager.Singleton.Client.Send(message);
    }

    #endregion


}
