using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.RefData
{
    public class TrialRewardRefObj : SCRefDataCore
    {
        public long id;
        public List<long> partList;
        protected override void _parseFromString()
        {
            id = getLong("id");
            partList = getList<long>("partList");
        }
        public static string assetPath => "RefData/ExportTxt";
        public static string sheetName => "trial_reward";
    }
}
