using GameCore.RefData;
using System;
using UnityEngine;

namespace GameCore
{
    public class BuffInfo
    {
        public PartInfo creator;
        public PartInfo owner;
        public BuffRefObj buffRefObj;
        public EBuffType buffType;
        public float buffValue;
        public bool isPositive;
        public int buffLayer;
        public Action onPartAwake;

        public BuffInfo(BuffRefObj _buffRefObj,int _layer, PartInfo _creator,PartInfo _owner)
        { 
            buffRefObj = _buffRefObj;
            buffType = _buffRefObj.buffType;
            buffValue = _buffRefObj.buffValue;
            isPositive = _buffRefObj.isPositive;
            buffLayer = _layer;
            creator = _creator;
            owner = _owner;
        }
        public void AddBuffLayer()
        {
            buffLayer = Mathf.Min(buffLayer + 1, GameConst.BUFF_LAYER_MAX);
        }
        public void ReduceBuffLayer()
        {
            buffLayer = Mathf.Max(buffLayer - 1, 0);
        }
    }
}
