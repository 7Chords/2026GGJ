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
        /// <summary>Each entry: part_level id and weight, same format as enemy bootyList (id:weight;). Plain id defaults weight 1.</summary>
        public List<BootyEffectObj> partList;
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
            partList = getList<BootyEffectObj>("partList");
        }
        public static string assetPath => "RefData/ExportTxt";
        public static string sheetName => "event_blood_2_part";
    }
}
