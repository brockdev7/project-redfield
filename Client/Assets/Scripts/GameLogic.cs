using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    private static GameLogic _singleton;

    public static Dictionary<int, ItemData> itemList = new Dictionary<int, ItemData>();
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

    [Header("Items")]
    [SerializeField] private ItemData GreenHerb;
    [SerializeField] private ItemData RedHerb;

    public void SpawnItem(int _spawnerId, Vector3 _position, bool _hasItem, int itemId)
    {       
        //Instantiate Item by Id
        if (itemList.TryGetValue(itemId, out ItemData item))
        {           
            if (!GameLogic.itemSpawners.ContainsKey(_spawnerId))
            {
                GameObject _spawner = Instantiate(item.spawnerModel, _position, item.spawnerModel.transform.rotation);
                _spawner.GetComponent<ItemSpawner>().Initialize(_spawnerId, _hasItem);

                itemSpawners.Add(_spawnerId, _spawner.GetComponent<ItemSpawner>());
            }             
        }    
    }

    private void Awake()
    {
        Singleton = this;

        itemList = new Dictionary<int, ItemData>()
        {
            {1, GreenHerb },
            {2, RedHerb }
        };
    }
}
