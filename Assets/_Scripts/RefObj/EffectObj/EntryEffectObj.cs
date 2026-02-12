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
        public float attributeValue;
        protected override void OnDeserialize(string _str)
        {
            string[] strArr = _str.Split(':');
            if (strArr == null || strArr.Length < 3)
                return;
            triggerPointType = (EAttributeTriggerPointType)SCCommon.ParseEnum(strArr[0], typeof(EAttributeTriggerPointType));
            attributeType = (EAttributeType)SCCommon.ParseEnum(strArr[1],typeof(EAttributeType));
            attributeValue = SCCommon.ParseFloat(strArr[2]);
        }

        protected override string OnSerialise()
        {
            string str = attributeType + ":" + attributeValue;
            return str;
        }
    }
}
