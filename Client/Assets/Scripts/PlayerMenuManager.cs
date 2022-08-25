using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerMenuManager : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] public GameObject inventoryUI;
    [SerializeField] public bool menuIsOpen;


    //------------ INVENTORY SCREEN ------------\\

    public void Initialize()
    {
        menuIsOpen = false;
    }

    public void OpenInventoryScreen()
    {
        if (!menuIsOpen)
        {
            var menuManager = GameObject.Find("MenuManager");
            var uiCamera = GameObject.Find("UICamera");

            inventoryUI = Instantiate(inventoryUI, menuManager.transform, false);
            inventoryUI.SetActive(true);
            
            //Add 3D Obj Camera to Main Camera Stack
            var objCamera = inventoryUI.GetComponentInChildren<Camera>();
            var mainCamera = Camera.main;
            var mainCameraData = mainCamera.GetUniversalAdditionalCameraData();
            mainCameraData.cameraStack.Add(objCamera);

            //Set Canvas Render Camera
            var invCanvas = inventoryUI.GetComponent<Canvas>();
            invCanvas.worldCamera = uiCamera.GetComponent<Camera>();

            menuIsOpen = true;
        }          
    }

    public void CloseInventoryScreen()
    {
        if(menuIsOpen)
        {
            Destroy(inventoryUI);
            inventoryUI.SetActive(false);
            menuIsOpen = false;
        }
       
    }
}
