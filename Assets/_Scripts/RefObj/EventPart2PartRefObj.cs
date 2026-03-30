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
        public int partLevel;
        public List<long> partList;
        protected override void _parseFromString()
        {
            id = getLong("id");
            floor = getInt("floor");
            partLevel = getInt("partLevel");
            partList = getList<long>("partList");
        }
        public static string assetPath => "RefData/ExportTxt";
        public static string sheetName => "event_part_2_part";
    }
}
