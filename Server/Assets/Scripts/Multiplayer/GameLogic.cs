using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    private static GameLogic _singleton;
    public static Dictionary<Item, ItemData> itemList = new Dictionary<Item, ItemData>();
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

    public GameObject PlayerPrefab => playerPrefab;

    [Header("Prefabs")]
    [SerializeField] private GameObject playerPrefab;

    [Header("Items")]
    [SerializeField] public Item GreenHerb;
    [SerializeField] public Item RedHerb;

    private void Awake()
    {
        Singleton = this;

        itemList = new Dictionary<Item, ItemData>()
        {
            { GreenHerb, GreenHerb.itemData  },
            { RedHerb, RedHerb.itemData  }
        };
    }

}
