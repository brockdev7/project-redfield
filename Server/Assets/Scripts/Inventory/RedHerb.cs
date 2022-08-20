using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RedHerb : Collectable
{
    public RedHerb()
    {
        base.DisplayName = "Red Herb";
        base.ItemId = 2;
    }
    
    public override void Collect()
    {
        Destroy(gameObject);
    }


}
