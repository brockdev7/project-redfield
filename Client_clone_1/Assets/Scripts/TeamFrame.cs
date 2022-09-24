using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamFrame : MonoBehaviour
{
    [SerializeField] private int teammateId;
    public int TeammateId => teammateId;

    public void SetTeammate(int playerId)
    {
        teammateId = playerId;
    }


}
