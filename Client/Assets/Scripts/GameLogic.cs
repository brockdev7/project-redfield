using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    private static GameLogic _singleton;
    private static Dictionary<int, GameObject> itemList;
    public static Dictionary<int, ItemSpawner> itemSpawners = new Dictionary<int, ItemSpawner>();

    public static GameLogic Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(GameLogic)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    public GameObject LocalPlayerPrefab => localPlayerPrefab;
    public GameObject PlayerPrefab => playerPrefab;

    [Header("Player Prefabs")]
    [SerializeField] private GameObject localPlayerPrefab;
    [SerializeField] private GameObject playerPrefab;

    [Header("Item Prefabs")]
    [SerializeField] private GameObject GreenHerb;
    [SerializeField] private GameObject RedHerb;

    public void SpawnItem(int _spawnerId, Vector3 _position, bool _hasItem, int itemId)
    {       
        //Instantiate Item by Id
        if (itemList.TryGetValue(itemId, out GameObject item))
        {
            GameObject _spawner = Instantiate(item, _position, item.transform.rotation);
            _spawner.GetComponent<ItemSpawner>().Initialize(_spawnerId, _hasItem);

            if(!GameLogic.itemSpawners.ContainsKey(_spawnerId))
                itemSpawners.Add(_spawnerId, _spawner.GetComponent<ItemSpawner>());

        }    
    }

    public void OpenInventoryMenu() => UIManager.Singleton.OpenInventoryScreen();
    public void CloseInventoryMenu() => UIManager.Singleton.CloseInventoryScreen();


    private void Awake()
    {
        Singleton = this;

        itemList = new Dictionary<int, GameObject>()
        {
            {1, GreenHerb },
            {2, RedHerb }
        };
    }
}
