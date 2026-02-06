using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.RefData
{
    public class TrialRewardEffectObj : _AEffectObjBase
    {
        public int score;
        public long rewardPoolId;
        protected override void OnDeserialize(string _str)
        {
            string[] strArr = _str.Split(':');
            if (strArr == null || strArr.Length < 2)
                return;
            score = SCCommon.ParseInt(strArr[0]);
            rewardPoolId = SCCommon.ParseLong(strArr[1]);
        }

        protected override string OnSerialise()
        {
            string str = score + ":" + rewardPoolId;
            return str;
        }
    }
}
