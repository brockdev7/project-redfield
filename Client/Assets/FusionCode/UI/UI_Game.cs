using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_Game : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI startGameText;

    private void Start()
    {
        if (GameManager.InstanceExists)
            GameManager.OnGameStart.AddListener(HandleOnGameStart);
    }

    private void HandleOnGameStart()
    {
        StartCoroutine(DoGameStart());
    }

    private IEnumerator DoGameStart()
    {
        //var localPlayer = GameManager.GetLocalPlayer();
        var text = "The game has started...";

        startGameText.text = text;
        yield return new WaitForSecondsRealtime(GameManager.StartDelay);
        startGameText.text = string.Empty;
    }


}
