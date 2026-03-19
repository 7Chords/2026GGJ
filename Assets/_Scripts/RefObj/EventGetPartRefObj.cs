using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.RefData
{
    public class EventGetPartRefObj : SCRefDataCore
    {
        public EventGetPartRefObj()
        {

        }
        public EventGetPartRefObj(string _assetPath, string _sheetName) : base(_assetPath, _sheetName)
        {

        }
        public long id;
        public int floor;
        public List<long> partList;
        protected override void _parseFromString()
        {
            id = getLong("id");
            floor = getInt("floor");
            partList = getList<long>("partList");
        }
        public static string assetPath => "RefData/ExportTxt";
        public static string sheetName => "event_get_part";
    }
}
