using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.RefData
{
    public class EventBlood2PartRefObj : SCRefDataCore
    {
        public long id;
        public int floor;
        public EEventType eventType;
        public int blood;
        public List<long> partList;
        public EventBlood2PartRefObj()
        {
        }
        public EventBlood2PartRefObj(string _assetPath, string _sheetName) : base(_assetPath, _sheetName)
        {

        }
        protected override void _parseFromString()
        {
            id = getLong("id");
            floor = getInt("floor");
            eventType = (EEventType)getEnum("eventType", typeof(EEventType));
            blood = getInt("blood");
            partList = getList<long>("partList");
        }
        public static string assetPath => "RefData/ExportTxt";
        public static string sheetName => "event_blood_2_part";
    }
}
