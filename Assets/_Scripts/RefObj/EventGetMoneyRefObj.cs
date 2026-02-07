using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.RefData
{
    public class EventGetMoneyRefObj : SCRefDataCore
    {
        public EventGetMoneyRefObj()
        {

        }
        public EventGetMoneyRefObj(string _assetPath, string _sheetName) : base(_assetPath, _sheetName)
        {

        }
        public long id;
        public int floor;
        public int money;
        protected override void _parseFromString()
        {
            id = getLong("id");
            floor = getInt("floor");
            money = getInt("money");
        }
        public static string assetPath => "RefData/ExportTxt";
        public static string sheetName => "event_get_money";
    }
}
