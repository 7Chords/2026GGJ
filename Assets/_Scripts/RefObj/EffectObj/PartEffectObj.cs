using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.RefData
{
    public class PartEffectObj : _AEffectObjBase
    {
        public long partId;
        public int partAmount;
        protected override void OnDeserialize(string _str)
        {
            string[] strArr = _str.Split(':');
            if (strArr == null || strArr.Length < 2)
                return;
            partId = SCCommon.ParseInt(strArr[0]);
            partAmount = SCCommon.ParseInt(strArr[1]);

        }

        protected override string OnSerialise()
        {
            string str = partId + ":" + partAmount;
            return str;
        }
    }
}
