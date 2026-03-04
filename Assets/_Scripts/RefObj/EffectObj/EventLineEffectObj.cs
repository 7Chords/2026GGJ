using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.RefData
{
    public class EventLineEffectObj : _AEffectObjBase
    {
        public List<long> eventList;
        protected override void OnDeserialize(string _str)
        {
            string[] strArr = _str.Split(':');
            if (strArr == null || strArr.Length < 1)
                return;
            eventList = new List<long>();
            for(int i = 0; i < strArr.Length; i++)
            {
                eventList.Add(SCCommon.ParseLong(strArr[i]));
            }
        }

        protected override string OnSerialise()
        {
            return "eventline";
        }
    }
}

