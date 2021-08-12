using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SavaData
{
    //public Dictionary<string, int> localLeaderBoardData = new Dictionary<string, int>();
    [SerializeField]
    public List<string> keys = new List<string>();
    public List<int> values = new List<int>();

    //public List<KeyValuePair<string, int>> localLeaderBoardData = 
    //  new List<KeyValuePair<string, int>>();
}
