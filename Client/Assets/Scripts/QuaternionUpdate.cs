using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class QuaternionUpdate
{
    public ushort Tick { get; private set; }
    public Quaternion  Rotation { get; private set; }

    public QuaternionUpdate(ushort tick, Quaternion quaternion)
    {
        Tick = tick;
        Rotation = quaternion;
    }

}

