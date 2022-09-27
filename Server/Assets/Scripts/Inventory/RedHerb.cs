using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class RedHerb : Item
{

    public override void Use()
    {
        Debug.Log("Using a Red Herb");
        base.Use();
    }

    public override void Combine()
    {
        Debug.Log("Combining a Red Herb");
        base.Use();
    }

    public override void Present()
    {
        Debug.Log("Presenting a Red Herb");
        base.Use();
    }
}

