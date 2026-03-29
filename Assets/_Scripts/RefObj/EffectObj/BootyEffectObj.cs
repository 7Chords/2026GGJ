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
            if (string.IsNullOrEmpty(_str))
                return;
            string[] strArr = _str.Split(':');
            if (strArr == null || strArr.Length == 0)
                return;
            partLevelId = SCCommon.ParseLong(strArr[0].Trim());
            if (strArr.Length >= 2)
                dropChance = SCCommon.ParseFloat(strArr[1]);
            else
                dropChance = 1f;
        }

        protected override string OnSerialise()
        {
            string str = partLevelId + ":" + dropChance;
            return str;
        }
    }
}
