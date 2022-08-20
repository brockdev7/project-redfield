using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Herb : MonoBehaviour, ICollectible
{
    public void Collect()
    {
        Debug.Log("You collect a herb");
    }

}
