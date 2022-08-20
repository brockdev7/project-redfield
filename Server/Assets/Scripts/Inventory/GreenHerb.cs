using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GreenHerb : Collectable
{
    public GreenHerb()
    {
        base.DisplayName = "Green Herb";
        base.ItemId = 1;
    }

    public override void Collect()
    {
        Destroy(gameObject);
    }

}
