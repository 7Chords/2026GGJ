using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.RefData
{
    public class EventRefObj : SCRefDataCore
    {
        public long id;
        public int floor;
        public List<long> eventList;

        protected override void _parseFromString()
        {
            id = getLong("id");
            floor = getInt("floor");
            eventList = getList<long>("eventList");
        }
        public static string assetPath => "RefData/ExportTxt";
        public static string sheetName => "event";
    }
}
