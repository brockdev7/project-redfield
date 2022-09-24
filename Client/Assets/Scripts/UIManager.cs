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
    [SerializeField] private Button slot1;  
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemDesc;
    [SerializeField] private List<InventorySlot> inventorySlots;
    [SerializeField] private GameObject teamFrames;
    [SerializeField] private int inventoryMode;

    public int CurrentInventoryMode => inventoryMode;

    public bool inventoryScreenIsActive { get { return inventoryScreen.activeInHierarchy; } }
    public enum InventoryMode
    {
        view = 1,
        pickUp
    }

    #region Methods
    //View Mode
    public void OpenInventoryScreen()
    {
        SetViewMode(InventoryMode.view);
        RedrawInventorySlots();
        EnableHeaderButtons();
        UpdateInventoryData(slot1.GetComponent<InventorySlot>());
        inventoryScreen.SetActive(true);
    }

    //Pickup Mode
    public void OpenInventoryScreen(ushort _itemId, ushort _spawnerId, Player _player)
    {
        SetViewMode(InventoryMode.pickUp);

        var itemData = GameLogic.Singleton.GetItemData(_itemId);

        if (itemData)
        {
            DisableHeaderButtons();
            inventoryScreen.SetActive(true);
            slot1.Select();

            Instantiate(itemData.renderModel, renderView.transform, false);

            itemName.text = itemData.itemName;
            itemDesc.text = "Will you take it?";
        }
    }
    public void CloseInventoryScreen()
    {
        CleanupInventoryScreen();

        inventoryScreen.SetActive(false);
    }

    public void SetViewMode(InventoryMode mode)
    {
        inventoryMode = (int)mode;
    }

    public InventorySlot GetInventorySlot(ushort id)
    {
        foreach (var slot in inventorySlots)
        {
            if (slot.id == id)
                return slot;
        }
        return null;
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

    public void EnableHeaderButtons()
    {
        foreach (Transform child in header.transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    public void DisableHeaderButtons()
    {
        foreach (Transform child in header.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    public void CleanupInventoryScreen()
    {
        //Destroy any previous 3D Obj render
        if (renderView.transform.childCount > 0)
        {
            foreach (Transform child in renderView.transform)
            {
                GameObject.DestroyImmediate(child.gameObject);
            }
        }
    }

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

    #endregion

    #region Teammate Inventory Methods
    public void AddToTeammateInventory(int teammateId, ItemData item, int slotId)
    {
        var frame = GetTeammateFrame(teammateId);
   
        if(frame)
        {
            var invSlots = frame.GetComponentsInChildren<InventorySlot>();
            foreach(var slot in invSlots)
            {
                if (slot.id == slotId)
                    slot.Set(item);
            }
        }
    }

    public GameObject GetTeammateFrame(int teammateId)
    {
        foreach(Transform transform in teamFrames.transform)
        {
            var teamFrame = transform.gameObject.GetComponent<TeamFrame>();
            if (teamFrame.TeammateId == teammateId)
                return teamFrame.gameObject;
        }

        return null;
    }

    public void AssignTeamFrames()
    {
        List<Player> playerList = Player.list.Values.Where(x => !x.IsLocal).ToList();
        List<GameObject> frameList = new List<GameObject>();

        if (playerList.Count > 0)
        {
            Debug.Log("Assigning teammate inventory frames...");

            foreach (Transform frame in teamFrames.transform)
                frameList.Add(frame.gameObject);

            for (int x = 0; x < playerList.Count(); x++)
            {
                GameObject frameObj = frameList[x].gameObject;
                var teamFrame = frameObj.GetComponent<TeamFrame>();
                teamFrame.SetTeammate(playerList[x].Id);
                teamFrame.gameObject.SetActive(true);
            }
        }
    }

    #endregion

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

            //Update RenderModel
            if (_slot.renderModel)
            {
                var renderObj = Instantiate(_slot.renderModel, renderView.transform, false);
            }

            //Update Item Info
            itemName.text = _slot.itemName;
            itemDesc.text = _slot.itemDesc;
        }

    }

    public void SubmitInventorySlot(InventorySlot _slot)
    {
        // ------ VIEW MODE ----------
        if (inventoryMode == (int)InventoryMode.view)
        {
            Debug.Log($"{_slot.gameObject.name} selected.");
        }

    }

    #endregion


    //-----------------------------------------------------------------------------
    // CONNECT SCREEN
    //-----------------------------------------------------------------------------

    #region Connect Screen
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

    #endregion

    //-----------------------------------------------------------------------------
    // MESSAGES
    //-----------------------------------------------------------------------------

    #region Message Senders

    public void LogIn()
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.playerLoggingIn);
        message.AddString(usernameField.text);
        NetworkManager.Singleton.Client.Send(message);
    }


    public void SlotSelected()
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.inventorySlotSelect);   
        NetworkManager.Singleton.Client.Send(message);
    }

    #endregion





}
