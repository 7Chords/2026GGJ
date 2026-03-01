using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.RefData
{
    public class PartLevelEffectObj : _AEffectObjBase
    {
        public long partLevelId;
        public int partAmount;
        protected override void OnDeserialize(string _str)
        {
            string[] strArr = _str.Split(':');
            if (strArr == null || strArr.Length < 2)
                return;
            partLevelId = SCCommon.ParseLong(strArr[0]);
            partAmount = SCCommon.ParseInt(strArr[1]);
        }

        protected override string OnSerialise()
        {
            return partLevelId + ":" + partAmount;
        }
    }
}
