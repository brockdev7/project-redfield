using Fusion;
using ProjectRedfield.Utilities;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Menu : Singleton<UI_Menu>
{
    [SerializeField] private Button autoModeButton;
    [SerializeField] private TMP_InputField sessionNameInputField;

    private void Start()
    {
        autoModeButton.onClick.AddListener(() =>
        {
            HandleAutoModeButtonPressed();
        });

        sessionNameInputField.onValueChanged.AddListener((text) =>
        {
            Debug.Log($"Session Code: {text}");
        });
    }

    private static void HandleAutoModeButtonPressed()
    {
        Instance.autoModeButton.interactable = false;
        App.Instance.StartRoomGame(GameMode.AutoHostOrClient, Instance.sessionNameInputField.text);
    }


}

