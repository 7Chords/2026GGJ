using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.RefData
{
    public class MapRefObj : SCRefDataCore
    {
        public long id;
        public int floor;
        public string mapName;
        public string mapCfgName;
        protected override void _parseFromString()
        {
            id = getLong("id");
            floor = getInt("floor");
            mapName = getString("mapName");
            mapCfgName = getString("mapCfgName");
        }
        public static string assetPath => "RefData/ExportTxt";
        public static string sheetName => "map";

    }
}
