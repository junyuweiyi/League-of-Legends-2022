using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class List2SerSerialize
{
    [System.Serializable]
    public class SubList2SerSerialize
    {
        public List<Item> List;
    }
    public List<SubList2SerSerialize> List;
}