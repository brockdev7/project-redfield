using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FPSScript : MonoBehaviour
{
    public TextMeshProUGUI fpsTxt;

    float pollingTime = 1f;
    float time;
    int frameCount;

    void Update()
    {
        time += Time.deltaTime;

        frameCount++;

        if (time > pollingTime)
        {
            int frameRate = Mathf.RoundToInt(frameCount / time);
            fpsTxt.text = frameRate.ToString();
            time -= pollingTime;
            frameCount = 0;
        }

    }
}
