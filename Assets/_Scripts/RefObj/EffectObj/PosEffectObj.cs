using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.RefData
{
    public class PosEffectObj : _AEffectObjBase
    {
        public int x;
        public int y;
        //public EGridPosType gridPosType;
        protected override void OnDeserialize(string _str)
        {
            string[] strArr = _str.Split(':');
            if (strArr == null || strArr.Length < 2)
                return;
            x = SCCommon.ParseInt(strArr[0]);
            y = SCCommon.ParseInt(strArr[1]);
            //gridPosType = (EGridPosType)SCCommon.ParseEnum(strArr[2], typeof(EGridPosType));
        }

        protected override string OnSerialise()
        {
            string str = x + ":" + y;
            return str;
        }
    }
}
