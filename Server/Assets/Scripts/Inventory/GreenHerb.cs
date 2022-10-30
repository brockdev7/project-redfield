using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GreenHerb : Item
{

    public override void Use()
    {
        Debug.Log("Using a Green Herb");
    }

    public override void Combine()
    {
        Debug.Log("Combining a Green Herb");
    }

    public override void Present()
    {
        Debug.Log("Presenting a Green Herb");
    }
}

