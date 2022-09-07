using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
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

    [Header("Connect Screen")]
    [SerializeField] private GameObject connectScreen;
    [SerializeField] private InputField usernameField;

    [Header("Inventory Screen")]
    [SerializeField] private GameObject inventoryScreen;
    [SerializeField] private Button Slot1;

    //REGISTER/DE-REGISTER EVENTS
    public void OnEnable()
    {
        InventorySlot.OnInventorySlotSelect += SelectSlot;
    }

    public void OnDisable()
    {
        InventorySlot.OnInventorySlotSelect -= SelectSlot;

    }

    public void Awake()
    {
        Singleton = this;
        inventoryMode = (int)InventoryMode.view;

        connectScreen.SetActive(true);
        usernameField.interactable = true;
        
        InitializeObjectViewer();
    }


    //------------ INVENTORY SCREEN ------------\\
    public bool inventoryScreenIsActive { get { return inventoryScreen.activeInHierarchy; } }
    [SerializeField] private List<InventorySlot> inventorySlots;
    private GameObject currentModel;
    private ItemData currentItem;
    public int inventoryMode;

    public enum InventoryMode
    {
        view = 1,
        pickUp
    }


    public void ClearSlot(InventorySlot _slot)
    {
        var icon = _slot.transform.Find("Icon").GetComponent<Image>();
        icon.enabled = false;
    }

    public void RedrawInventorySlots()
    {
        DeselectAll();

        var invSlot = Slot1.GetComponent<InventorySlot>();
        invSlot.SetSelectedColor();
        invSlot.isSelected = true;

        Slot1.Select();    
    }


    public void DeselectAll()
    {
        foreach (var slot in inventorySlots)
        {
            slot.isSelected = false;
            slot.SetDefaultColor();
        }
    }

    public void SelectSlot(InventorySlot _slot)
    {
        // ------ VIEW MODE ----------
        if (inventoryMode == (int)InventoryMode.view)
        {
            Debug.Log($"{_slot.gameObject.name} selected.");
        }

        // ------ PICK UP MODE ----------
        if (inventoryMode == (int)InventoryMode.pickUp)
        {
            if (_slot.hasItem)
            {
                //TODO: Drop Item on Ground
                Debug.Log($"Slot already contained:{_slot.ItemData.itemName} replacing with {currentItem.itemName}");
            }

            Debug.Log($"Item added to Slot:{_slot.gameObject.name}");

            //Update Slot Item Data
            _slot.SetItemData(currentItem);
            _slot.hasItem = true;

            //Update Slot Icon
            var slotIcon = _slot.transform.Find("Icon").GetComponent<Image>();
            slotIcon.sprite = currentItem.icon;
            slotIcon.enabled = true;

            //Reset back to View
            inventoryMode = (int)InventoryMode.view;
        }
    }

    public void OpenInventoryScreen()
    {
        inventoryMode = (int)InventoryMode.view;
        RedrawInventorySlots();
        inventoryScreen.SetActive(true);
    }

    public void OpenInventoryScreen(int _itemId)
    {
        inventoryMode = (int)InventoryMode.pickUp;

        if (GameLogic.itemList.TryGetValue(_itemId, out ItemData item))
        {
            inventoryScreen.SetActive(true);
            Slot1.Select();
          
            //Add the RenderView Model to the 3D Object UI Element
            GameObject renderView = GameObject.Find("RenderView");
            currentModel = Instantiate(item.renderModel, renderView.transform, false);
            currentItem = item;

            //Set Item Name & Description
            TextMeshProUGUI itemName = GameObject.Find("ItemName").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI itemDesc = GameObject.Find("ItemDescription").GetComponent<TextMeshProUGUI>();

            itemName.text = item.itemName;
            itemDesc.text = "Will you take it?";
        }
    }

    public void CloseInventoryScreen()
    {    
        //Destroy Model Preview & Reset CurrentItem
        if(currentModel)
            GameObject.DestroyImmediate(currentModel);

        if(currentItem)
            currentItem = null;

        inventoryScreen.SetActive(false);
    }

    //------------ CONNECT SCREEN ------------\\

    public void ConnectClicked()
    {
        usernameField.interactable = false;
        connectScreen.SetActive(false);

        NetworkManager.Singleton.Connect();
    }

    public void BackToMain()
    {
        usernameField.interactable = true;
        connectScreen.SetActive(true);
    }



    #region Message Senders
    public void LogIn()
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.playerLoggingIn);
        message.AddString(usernameField.text);
        NetworkManager.Singleton.Client.Send(message);
    }

    #endregion

    public void InitializeObjectViewer()
    {
        //Add 3D Obj Camera to Main Camera Stack
        Camera objCamera = inventoryScreen.GetComponentInChildren<Camera>();
        Camera mainCamera = Camera.main;
        UniversalAdditionalCameraData mainCameraData = mainCamera.GetUniversalAdditionalCameraData();
        mainCameraData.cameraStack.Add(objCamera);

        //Set Canvas Render Camera
        Canvas invCanvas = inventoryScreen.GetComponent<Canvas>();
        invCanvas.worldCamera = GameObject.Find("UICamera").GetComponent<Camera>();
    }
}
