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
        public float attributeValue;

        public EntryInfo(EntryEffectObj _refObj)
        {
            if (_refObj == null)
                return;
            effectObj = _refObj;
            triggerPointType = _refObj.triggerPointType;
            attributeType = _refObj.attributeType;
            attributeChance = _refObj.attributeChance;
            attributeValue = _refObj.attributeValue;
        }
    }
}
