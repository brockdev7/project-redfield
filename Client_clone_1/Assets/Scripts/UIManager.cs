using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    //REGISTER/DE-REGISTER EVENTS
    public void OnEnable()
    {
        InventorySlot.OnInventorySlotSubmit += SubmitInventorySlot ;
        InventorySlot.OnInventorySlotMove += UpdateInventoryData;
    }

    public void OnDisable()
    {
        InventorySlot.OnInventorySlotSubmit -= SubmitInventorySlot;
        InventorySlot.OnInventorySlotMove -= UpdateInventoryData;
    }

    public void Awake()
    {
        Singleton = this;
       
        connectScreen.SetActive(true);
        usernameField.interactable = true;

        SetViewMode(InventoryMode.view);
        InitializeObjectViewer();
    }

    //-----------------------------------------------------------------------------
    // INVENTORY SCREEN
    //-----------------------------------------------------------------------------

    [Header("Inventory Screen")]
    [Header("-------------------------------------")]
    [SerializeField] private GameObject inventoryScreen;
    [SerializeField] private GameObject header;
    [SerializeField] private GameObject renderView;
    [SerializeField] private GameObject teamFrames;
    [SerializeField] private Button slot1;  
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemDesc;
  
    public int inventoryMode;
    [SerializeField] public List<InventorySlot> inventorySlots;

    public bool inventoryScreenIsActive { get { return inventoryScreen.activeInHierarchy; } }
    public bool inventorySlotSelected {  get { return inventorySlots.Any(s => s.isSelected);  } }

    public enum InventoryMode
    {
        view = 1,
        pickUp
    }
    
    public void SetViewMode(InventoryMode mode)
    {
        inventoryMode = (int)mode;
    }

    public void UpdateTeammateFrames()
    {      
        List<Player> playerList = Player.list.Values.Where(x => !x.IsLocal).ToList();
        List<GameObject> frameList = new List<GameObject>();

        if(playerList.Count > 0)
        {
            Debug.Log("Updating teammate frames...");

            foreach (Transform frame in teamFrames.transform)
                frameList.Add(frame.gameObject);

            for (int x = 0; x < playerList.Count(); x++)
            {                
                GameObject frameObj = frameList[x].gameObject;
                var teamFrame = frameObj.GetComponent<TeamFrame>();
                teamFrame.teammate = playerList[x];
            }
        }    
    }

    public void RedrawInventorySlots()
    {
        DeselectAll();

        var invSlot = slot1.GetComponent<InventorySlot>();
        invSlot.SetSelectedColor();
        invSlot.isSelected = true;

        slot1.Select();    
    }

    public void DeselectAll()
    {
        foreach (var slot in inventorySlots)
        {
            slot.isSelected = false;
            slot.SetDefaultColor();
        }
    }


    public InventorySlot GetSelectedSlot()
    {
        foreach(InventorySlot slot in inventorySlots)
        {
            if (slot.isSelected)
                return slot;
        }

        return null;
    }

    public void AddToSlot(InventorySlot _slot)
    {
        if (_slot.hasItem)
        {
            //TODO: Drop Item on Ground
            Debug.Log($"Slot already contained:{_slot.ItemData.itemName};");
        }

        Debug.Log($"{_slot.ItemData.itemName} added to Slot:{_slot.gameObject.name}");

        //Update Slot Item Data
        _slot.hasItem = true;

        //Update Slot Icon
        var slotIcon = _slot.transform.Find("Icon").GetComponent<Image>();
        slotIcon.sprite = _slot.ItemData.icon;
        slotIcon.enabled = true;


        //Reset back to View
        SetViewMode(InventoryMode.view);
    }

    #region Events
    public void UpdateInventoryData(InventorySlot _slot)
    {
        if (inventoryMode == (int)InventoryMode.view)
        {
            //Destroy any previous 3D Obj render
            if (renderView.transform.childCount > 0)
            {
                foreach (Transform child in renderView.transform)
                {
                    GameObject.DestroyImmediate(child.gameObject);
                }
            }

            if (_slot.hasItem)
            {              
                var renderObj = Instantiate(_slot.ItemData.renderModel, renderView.transform, false);

                //Update Item Info
                itemName.text = _slot.ItemData.itemName;
                itemDesc.text = _slot.ItemData.itemDescription;
            }
            else
            {
                //Clear Item Info
                itemName.text = "";
                itemDesc.text = "";
            }
        }
        
    }

    public void SubmitInventorySlot(InventorySlot _slot)
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
                Debug.Log($"Slot already contained:{_slot.ItemData.itemName};");
            }
        }
    }

    #endregion

    //View Mode
    public void OpenInventoryScreen()
    {
        SetViewMode(InventoryMode.view);

        RedrawInventorySlots();
        EnableHeaderButtons();
        UpdateInventoryData(slot1.GetComponent<InventorySlot>());

        //Activate teammate frames
        foreach (Transform child in teamFrames.transform)
        {
            var frameObj = child.gameObject;
            var teamFrame = frameObj.GetComponent<TeamFrame>();

            if (teamFrame.teammate)
            {
                //UpdateTeammateSlot(teamFrame);
                teamFrame.gameObject.SetActive(true);              
            }
        }

        inventoryScreen.SetActive(true);
    }

    //Pickup Mode
    public void OpenInventoryScreen(int _itemId, int _spawnerId, Player _player)
    {
        SetViewMode(InventoryMode.pickUp);

        if (GameLogic.itemList.TryGetValue(_itemId, out ItemData item))
        {
            DisableHeaderButtons();
            inventoryScreen.SetActive(true);
            slot1.Select();

            Instantiate(item.renderModel, renderView.transform, false);

            itemName.text = item.itemName;
            itemDesc.text = "Will you take it?";
        }
    }

    public void UpdateTeammateSlot(TeamFrame frame)
    {
        var slots = frame.GetComponentsInChildren<InventorySlot>();  
    }

    public void EnableHeaderButtons()
    {
        foreach (Transform child in header.transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    public void DisableHeaderButtons()
    {
        foreach(Transform child in header.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    public void CloseInventoryScreen()
    {
        CleanupInventoryScreen();

        inventoryScreen.SetActive(false);
    }

    public void CleanupInventoryScreen()
    {
        ////Destroy Model Preview & Reset CurrentItem
        //if (currentModel)
        //    GameObject.DestroyImmediate(currentModel);

        //if (currentItem)
        //    currentItem = null;

        //if (currentPlayer)
        //    currentPlayer = null;

        //Destroy any previous 3D Obj render
        if (renderView.transform.childCount > 0)
        {
            foreach (Transform child in renderView.transform)
            {
                GameObject.DestroyImmediate(child.gameObject);
            }
        }
    }


    //-----------------------------------------------------------------------------
    // CONNECT SCREEN
    //-----------------------------------------------------------------------------

    [Header("Connect Screen")]
    [Header("-------------------------------------")]
    [SerializeField] private GameObject connectScreen;
    [SerializeField] private InputField usernameField;

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
