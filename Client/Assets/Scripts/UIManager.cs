using RiptideNetworking;
using Steamworks;
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
        InventorySlot.OnInventorySlotSubmit += SubmitInventorySlot;
        InventorySlot.OnInventorySlotMove += UpdateInventoryData;
        InfoButton.OnInfoButtonMove += UpdateInventoryData;
    }

    public void OnDisable()
    {
        InventorySlot.OnInventorySlotSubmit -= SubmitInventorySlot;
        InventorySlot.OnInventorySlotMove -= UpdateInventoryData;
        InfoButton.OnInfoButtonMove -= UpdateInventoryData;
    }

    public void Awake()
    {
        Singleton = this;
        mainMenu.SetActive(true);
        connectionState.SetActive(true);
        SetViewMode(InventoryMode.view);
        InitializeObjectViewer();
    }


    //-----------------------------------------------------------------------------
    // SCREENS
    //-----------------------------------------------------------------------------
    [Header("-------------------------------------")]
    [Header("Screens")]
    [Header("-------------------------------------")]

    #region In-Game
    [SerializeField] private GameObject inventoryScreen;
    #endregion

    #region Pre-game

    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject fusionMenu;
    [SerializeField] private GameObject lobbyHeader;
    [SerializeField] private GameObject lobbyScreen;
    [SerializeField] private GameObject pregameLobbyScreen;

    public Image steamAvatar;
    public TextMeshProUGUI steamName;
    #endregion

    //-----------------------------------------------------------------------------
    // MAIN MENU
    //-----------------------------------------------------------------------------
    #region Main Menu
    [Header("-------------------------------------")]
    [Header("Main Menu")]
    [Header("-------------------------------------")]
    [SerializeField] private InputField usernameField;
    [SerializeField] private GameObject connectionState;

    public void ConnectLocalMultiplayer()
    {
        usernameField.interactable = false;
        mainMenu.SetActive(false);
        NetworkManager.Singleton.TransportMode = TransportMode.LocalMp;
        NetworkManager.Singleton.ConnectLocalMultiplayer();
    }
    public void ConnectSteam()
    {
        mainMenu.SetActive(false);
        lobbyHeader.SetActive(true);
        lobbyScreen.SetActive(true);
        steamName.text = SteamFriends.GetPersonaName();
        steamAvatar.sprite = ProjectRedfield.SteamUtils.SteamUtils.GetMediumFriendAvatar(SteamUser.GetSteamID());

        SteamLobby.Singleton.GetLobbiesList();
        NetworkManager.Singleton.TransportMode = TransportMode.Steam;
        NetworkManager.Singleton.ConnectSteam();
    }
    #endregion

    //-----------------------------------------------------------------------------
    // LOBBY SCREEN
    //-----------------------------------------------------------------------------
    [Header("-------------------------------------")]
    [Header("Lobby Screen")]
    [Header("-------------------------------------")]
    public GameObject lobbyRoomContainerPrefab;
    public GameObject lobbyRoomPrefab;
    public GameObject lobbyPlayerCardPrefab;
    public GameObject lobbyListContent;
    public GameObject hostButton;

    public List<GameObject> lobbyRoomContainers = new List<GameObject>();

    public void BackToMain()
    {
        lobbyScreen.SetActive(false);
        pregameLobbyScreen.SetActive(false);

        mainMenu.SetActive(true);
    }
    public void HostClicked()
    {
        SteamLobby.Singleton.CreateLobby();
    }
    public void LobbyCreationFailed()
    {
        pregameLobbyScreen.SetActive(false);
        lobbyScreen.SetActive(true);
    }
    public void LobbyCreationSucceeded()
    {
        lobbyScreen.SetActive(false);
        pregameLobbyScreen.SetActive(true);
    }
    public void LobbyEntered()
    {
        pregameLobbyScreen.SetActive(true);
    }
    public void LeaveLobbyClicked()
    {
        SteamLobby.Singleton.LeaveLobby();
        pregameLobbyScreen.SetActive(false);
        lobbyScreen.SetActive(true);
    }
    public void SetLobbyData(string lobbyName)
    {
        lobbyNameText.text = lobbyName;
    }
    public void SearchLobbies(TMP_InputField searchText)
    {
        SteamLobby.Singleton.SearchLobbies(searchText.text);
    }
    public void DestroyLobbies()
    {
        foreach (GameObject lobbyContainer in lobbyRoomContainers)
            Destroy(lobbyContainer);

        lobbyRoomContainers.Clear();
    }
    public void DisplayLobbies(List<CSteamID> lobbyIds, LobbyDataUpdate_t result)
    {
        for (int i = 0; i < lobbyIds.Count; i++)
        {
            //Found Lobby Id that matches our previous search
            if (lobbyIds[i].m_SteamID == result.m_ulSteamIDLobby)
            {
                //Add Lobby Room Container if doesn't exist
                if (lobbyRoomContainers.Count() == 0)
                {

                    var lobbyContainer = Instantiate(lobbyRoomContainerPrefab);
                    lobbyContainer.transform.SetParent(lobbyListContent.transform);
                    lobbyContainer.transform.localScale = Vector3.one;
                    lobbyRoomContainers.Add(lobbyContainer);

                    //Create Lobby Room GameObject inside Lobby Container
                    var lobbyRoom = Instantiate(lobbyRoomPrefab);
                    lobbyRoom.GetComponent<LobbyRoom>().lobbyId = (CSteamID)lobbyIds[i].m_SteamID;
                    lobbyRoom.GetComponent<LobbyRoom>().lobbyName = SteamMatchmaking.GetLobbyData((CSteamID)lobbyIds[i].m_SteamID, "name");
                    lobbyRoom.transform.SetParent(lobbyContainer.transform);
                    lobbyRoom.transform.localScale = Vector3.one;
                    lobbyRoom.GetComponent<LobbyRoom>().SetLobbyData();
                }
                else
                {
                    //Get last added lobby room container
                    var lastContainer = lobbyRoomContainers.Last();

                    //Check container room counter
                    if (lastContainer.transform.childCount == 3)
                    {
                        var lobbyContainer = Instantiate(lobbyRoomContainerPrefab);
                        lobbyContainer.transform.SetParent(lobbyListContent.transform);
                        lobbyContainer.transform.localScale = Vector3.one;
                        lobbyRoomContainers.Add(lobbyContainer);

                        //Create Lobby Room GameObject inside Lobby Container
                        var lobbyRoom = Instantiate(lobbyRoomPrefab);
                        lobbyRoom.GetComponent<LobbyRoom>().lobbyId = (CSteamID)lobbyIds[i].m_SteamID;
                        lobbyRoom.GetComponent<LobbyRoom>().lobbyName = SteamMatchmaking.GetLobbyData((CSteamID)lobbyIds[i].m_SteamID, "name");
                        lobbyRoom.transform.SetParent(lobbyContainer.transform);
                        lobbyRoom.transform.localScale = Vector3.one;
                        lobbyRoom.GetComponent<LobbyRoom>().SetLobbyData();
                    }
                    else
                    {

                        //Create Lobby Room GameObject inside Lobby Container
                        var lobbyRoom = Instantiate(lobbyRoomPrefab);
                        lobbyRoom.GetComponent<LobbyRoom>().lobbyId = (CSteamID)lobbyIds[i].m_SteamID;
                        lobbyRoom.GetComponent<LobbyRoom>().lobbyName = SteamMatchmaking.GetLobbyData((CSteamID)lobbyIds[i].m_SteamID, "name");
                        lobbyRoom.transform.SetParent(lastContainer.transform);
                        lobbyRoom.transform.localScale = Vector3.one;
                        lobbyRoom.GetComponent<LobbyRoom>().SetLobbyData();

                    }

                }

                //createdItem.GetComponent<LobbyDataEntry>().lobbyId = (CSteamID)lobbyIds[i].m_SteamID;
                //createdItem.GetComponent<LobbyDataEntry>().lobbyName = SteamMatchmaking.GetLobbyData((CSteamID)lobbyIds[i].m_SteamID, "name");
                //createdItem.GetComponent<LobbyDataEntry>().SetLobbyData();
            }
        }
    }

    //-----------------------------------------------------------------------------
    // LOBBY SCREEN
    //-----------------------------------------------------------------------------
    [Header("-------------------------------------")]
    [Header("Pre-game Lobby Screen")]
    [Header("-------------------------------------")]
    public TextMeshProUGUI lobbyNameText;

    //-----------------------------------------------------------------------------
    // INVENTORY SCREEN
    //-----------------------------------------------------------------------------
    #region INVENTORY SCREEN
    [Header("-------------------------------------")]
    [Header("Inventory Screen")]
    [Header("-------------------------------------")]
    [SerializeField] private GameObject actionMenu;
    [SerializeField] private GameObject header;
    [SerializeField] private GameObject renderView;
    [SerializeField] private Button slot1;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemDesc;
    [SerializeField] private List<InventorySlot> inventorySlots;
    [SerializeField] private GameObject teamFrames;
    [SerializeField] private int inventoryMode;
    [SerializeField] public InventoryAudio inventoryAudio;

    public int CurrentInventoryMode => inventoryMode;

    public bool inventoryScreenIsActive { get { return inventoryScreen.activeInHierarchy; } }
    public bool actionMenuIsActive { get { return actionMenu.activeInHierarchy; } }
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

    //Enables or disables info button interactivity
    public void SetInfoButtonInteractivity(bool toggle)
    {
        foreach (Transform headerObj in header.transform)
        {
            var btn = headerObj.gameObject.GetComponent<Button>();
            btn.interactable = toggle;
        }
    }

    //Enables or disables inventory slot interactivity
    public void SetInventorySlotInteractivity(bool toggle)
    {
        foreach (var slot in inventorySlots)
        {
            var btn = slot.GetComponent<Button>();
            btn.interactable = toggle;
        }
    }

    //Pickup Mode
    public void OpenInventoryScreen(ushort _itemId, ushort _spawnerId, Player _player)
    {
        SetViewMode(InventoryMode.pickUp);
        DisableHeaderButtons();
        SetTeammateInventoryInteractivity(false);

        var itemData = GameLogic.Singleton.GetItemData(_itemId);

        if (itemData)
        {
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
        SetTeammateInventoryInteractivity(true);
    }

    public InventorySlot GetSelectedSlot()
    {
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.isSelected)
                return slot;
        }

        return null;
    }
    public void ClearInventoryData()
    {
        CleanupInventoryScreen();
        itemName.text = "";
        itemDesc.text = "";
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

    #region Action Menu
    public void OpenActionMenu()
    {
        //inventoryAudio.PlayActionMenuOpen();

        //Activate/Deactivate ActionMenu Action's
        EnableActionMenuButtons();

        //Disable Interactivity
        SetInfoButtonInteractivity(false);
        SetInventorySlotInteractivity(false);
        SetTeammateInventoryInteractivity(false);

        actionMenu.SetActive(true);

        //Select first action button
        GetFirstActionMenuButton().Select();
    }

    public void CloseActionMenu()
    {
        actionMenu.SetActive(false);

        //Enable Interactivity
        SetInfoButtonInteractivity(true);
        SetInventorySlotInteractivity(true);
        SetTeammateInventoryInteractivity(true);

        //Select previously selected slot
        var slot = GetSelectedSlot();
        slot.GetComponent<Button>().Select();

    }

    public void OnActionMenu_Equip()
    {
        CloseActionMenu();
        var slotId = GetSelectedSlot().id;
        actionMenuEquip(slotId);
    }
    public void OnActionMenu_Use()
    {
        CloseActionMenu();
        var slotId = GetSelectedSlot().id;
        actionMenuUse(slotId);
    }

    public void EnableActionMenuButtons()
    {
        //Get Slot Item & determine commands
        var slot = GetSelectedSlot();
        var item = GetInventorySlot(slot.id);

        var equipBtn = actionMenu.transform.Find("EQUIP").GetComponent<Button>();
        var useBtn = actionMenu.transform.Find("USE").GetComponent<Button>();
        var combineBtn = actionMenu.transform.Find("COMBINE").GetComponent<Button>();
        var tradeBtn = actionMenu.transform.Find("TRADE").GetComponent<Button>();

        //Set Equip Btn
        if (item.itemData.isEquippable)
            equipBtn.gameObject.SetActive(true);
        else
            equipBtn.gameObject.SetActive(false);

        //Set Use Btn
        if (item.itemData.isUsable)
            useBtn.gameObject.SetActive(true);
        else
            useBtn.gameObject.SetActive(false);

        //Set Combine Btn
        if (item.itemData.isCombinable)
            combineBtn.gameObject.SetActive(true);
        else
            combineBtn.gameObject.SetActive(false);

        if (item.itemData.isTradable)
            tradeBtn.gameObject.SetActive(true);
        else
            tradeBtn.gameObject.SetActive(false);
    }

    public Button GetFirstActionMenuButton()
    {
        foreach (Transform t in actionMenu.transform)
        {
            var action = t.gameObject;

            if (action.activeSelf)
                return action.GetComponent<Button>();
        }

        return null;
    }

    #endregion

    #region Teammate Inventory Methods
    public void AddToTeammateInventory(int teammateId, ItemData item, int slotId)
    {
        var frame = GetTeammateFrame(teammateId);

        if (frame)
        {
            var invSlots = frame.GetComponentsInChildren<InventorySlot>();
            foreach (var slot in invSlots)
            {
                if (slot.id == slotId)
                    slot.Set(item);
            }
        }
    }

    public GameObject GetTeammateFrame(int teammateId)
    {
        foreach (Transform transform in teamFrames.transform)
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

    public void SetTeammateInventoryInteractivity(bool toggle)
    {
        foreach (Transform frame in teamFrames.transform)
        {
            if (!frame.gameObject.activeSelf)
                continue;

            var teamSlots = frame.GetComponentsInChildren<Button>();

            foreach (var slot in teamSlots)
                slot.interactable = toggle;
        }
    }

    #endregion

    #region Events


    //Called on InventorySlot.OnMove();
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

            if (_slot.itemData)
            {
                //Update RenderModel
                if (_slot.renderModel)
                {
                    var renderObj = Instantiate(_slot.renderModel, renderView.transform, false);
                }

                //Update Item Info
                itemName.text = _slot.itemData.itemName;
                itemDesc.text = _slot.itemData.itemDescription;
            }
            else
            {
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
            if (_slot && _slot.hasItem)
            {
                _slot.GetComponent<Button>().Select();
                UIManager.Singleton.OpenActionMenu();
            }
            else
                Debug.Log("Slot is empty, can't open ActionMenu");
        }
    }

    #endregion

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

    public void actionMenuEquip(ushort slotId)
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.actionMenu_Equip);
        message.AddUShort(slotId);
        NetworkManager.Singleton.Client.Send(message);
    }

    public void actionMenuUse(ushort slotId)
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.actionMenu_Use);
        message.AddUShort(slotId);
        NetworkManager.Singleton.Client.Send(message);
    }

    #endregion





}
