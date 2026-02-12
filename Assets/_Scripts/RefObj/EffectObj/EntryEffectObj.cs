using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.RefData
{
    public class EntryEffectObj : _AEffectObjBase
    {
        public EAttributeTriggerPointType triggerPointType;
        public EAttributeType attributeType;
        public float attributeChance;

        public float attributeValue;
        protected override void OnDeserialize(string _str)
        {
            string[] strArr = _str.Split(':');
            if (strArr == null || strArr.Length < 4)
                return;
            triggerPointType = (EAttributeTriggerPointType)SCCommon.ParseEnum(strArr[0], typeof(EAttributeTriggerPointType));
            attributeType = (EAttributeType)SCCommon.ParseEnum(strArr[1],typeof(EAttributeType));
            attributeChance = SCCommon.ParseFloat(strArr[2]);
            attributeValue = SCCommon.ParseFloat(strArr[3]);
        }

        protected override string OnSerialise()
        {
            string str = attributeType + ":" + attributeValue;
            return str;
        }
    }
}
