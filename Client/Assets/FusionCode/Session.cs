using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Session : NetworkBehaviour
{
    public override void Spawned()
    {
        App.Instance.Session = this;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);
    }

    public void LoadGameScene()
    {
        if (Object.HasStateAuthority && (Runner.CurrentScene == 0 || Runner.CurrentScene == SceneRef.None))
        {
            Debug.Log("NetRunner setting active scene...");
            Runner.SetActiveScene(App.GameScene);
        }
    }
}
