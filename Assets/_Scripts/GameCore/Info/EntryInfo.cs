using GameCore.RefData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public class EntryInfo 
    {
        public EntryEffectObj effectObj;
        public EAttributeTriggerPointType triggerPointType;
        public EAttributeType attributeType;
        public float attributeChance;
        public List<float> attributeValueList;
        public EntryInfo(EntryEffectObj _refObj)
        {
            if (_refObj == null)
                return;
            effectObj = _refObj;
            triggerPointType = _refObj.triggerPointType;
            attributeType = _refObj.attributeType;
            attributeChance = _refObj.attributeChance;
            attributeValueList = new List<float>();
            for(int i =0;i< _refObj.attributeValueList.Count;i++)
            {
                attributeValueList.Add(_refObj.attributeValueList[i]);
            }
        }
    }
}
