using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.RefData
{
    public class EventPart2PartRefObj : SCRefDataCore
    {
        public EventPart2PartRefObj()
        {

        }
        public EventPart2PartRefObj(string _assetPath, string _sheetName) : base(_assetPath, _sheetName)
        {

        }
        public long id;
        public int floor;
        public EQualityType qualityType;
        /// <summary>Each entry: part_level id and weight, same format as enemy bootyList (id:weight;). Plain id defaults weight 1.</summary>
        public List<BootyEffectObj> partList;
        protected override void _parseFromString()
        {
            id = getLong("id");
            floor = getInt("floor");
            qualityType = (EQualityType)getEnum("qualityType",typeof(EQualityType));
            partList = getList<BootyEffectObj>("partList");
        }
        public static string assetPath => "RefData/ExportTxt";
        public static string sheetName => "event_part_2_part";
    }
}
