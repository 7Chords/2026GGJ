using GameCore.RefData;
using UnityEngine;

namespace GameCore
{
    public class BuffInfo
    {
        public BuffRefObj buffRefObj;
        public EBuffType buffType;
        public float buffValue;
        public bool isPositive;
        public int buffLayer;
        public BuffInfo(BuffRefObj _buffRefObj,int _layer)
        { 
            buffRefObj = _buffRefObj;
            buffType = _buffRefObj.buffType;
            buffValue = _buffRefObj.buffValue;
            isPositive = _buffRefObj.isPositive;
            buffLayer = _layer;
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
