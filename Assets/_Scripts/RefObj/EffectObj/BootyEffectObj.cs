using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SCFrame;

namespace GameCore.RefData
{
    public class BootyEffectObj : _AEffectObjBase
    {
        public long partLevelId;
        public float dropChance;
        protected override void OnDeserialize(string _str)
        {
            string[] strArr = _str.Split(':');
            if (strArr == null || strArr.Length < 2)
                return;
            partLevelId = SCCommon.ParseInt(strArr[0]);
            dropChance = SCCommon.ParseFloat(strArr[1]);

        }

        protected override string OnSerialise()
        {
            string str = partLevelId + ":" + dropChance;
            return str;
        }
    }
}
