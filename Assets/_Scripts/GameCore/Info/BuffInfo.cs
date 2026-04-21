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
        public Action onPartActive;
        public Action onPartTrigger;
        public Action onPartActionOver;
        public Action onPartDie;
        public Action onTurnOver;
        public Action onTotalTurnOver;
        public Action onPartGetHit;
        public Action onPartGetEffect;
        public BuffInfo(BuffRefObj _buffRefObj,int _layer, PartInfo _creator,PartInfo _owner)
        { 
            buffRefObj = _buffRefObj;
            buffType = _buffRefObj.buffType;
            buffValue = _buffRefObj.buffValue;
            isPositive = _buffRefObj.isPositive;
            buffLayer = Mathf.Clamp(_layer, -GameConst.BUFF_LAYER_MAX, GameConst.BUFF_LAYER_MAX);
            creator = _creator;
            owner = _owner;
        }
        /// <summary> Stack delta; negative layers mean reverse effect (e.g. STRONG reduces attack). </summary>
        public void AddBuffLayer(int _layer = 1)
        {
            buffLayer = Mathf.Clamp(buffLayer + _layer, -GameConst.BUFF_LAYER_MAX, GameConst.BUFF_LAYER_MAX);
        }
        /// <summary> Move stack toward zero by <paramref name="_layer"/> (peels both positive and negative stacks). </summary>
        public void ReduceBuffLayer(int _layer = 1)
        {
            if (_layer <= 0)
                return;
            if (buffLayer > 0)
                buffLayer = Mathf.Max(0, buffLayer - _layer);
            else if (buffLayer < 0)
                buffLayer = Mathf.Min(0, buffLayer + _layer);
        }
    }
}
