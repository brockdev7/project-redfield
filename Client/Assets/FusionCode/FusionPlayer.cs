using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FusionPlayer : NetworkBehaviour, ISpawned
{
    public override void Spawned()
    {
        Debug.Log($"Player {Object.InputAuthority} Spawned");

        App.Instance.SetPlayer(Object.InputAuthority, this);

        //if (Object.HasStateAuthority)
        //    Score = 0;
    }
}
