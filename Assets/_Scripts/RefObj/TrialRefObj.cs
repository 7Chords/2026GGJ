using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.RefData
{
    public class TrialRefObj : SCRefDataCore
    {
        public TrialRefObj()
        {

        }
        public TrialRefObj(string _assetPath, string _sheetName) : base(_assetPath, _sheetName)
        {

        }

        public long id;
        public int floor;
        public List<TrialRewardEffectObj> rewardList;
        protected override void _parseFromString()
        {
            id = getLong("id");
            floor = getInt("floor");
            rewardList = getList<TrialRewardEffectObj>("rewardList");
        }

        public static string assetPath => "RefData/ExportTxt";
        public static string sheetName => "trial";
    }
}
