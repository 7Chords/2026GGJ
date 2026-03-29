using GameCore.Helpers;
using GameCore.RefData;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// PART_2_PART: remove one bag part, add a weighted random part from event_part_2_part (same floor; prefer row matching sacrifice partLevel).
    /// </summary>
    public static class EventPartExchangeHelper
    {
        public static bool TryExecute(PartInfo sacrifice)
        {
            if (sacrifice == null || sacrifice.partRefObj == null)
                return false;
            var bag = GameModel.instance.playerInfo.bagPartInfoList;
            if (bag == null || !bag.Contains(sacrifice))
                return false;

            int floor = GameModel.instance.playerInfo.playerFloor;
            var all = SCRefDataMgr.instance.eventPart2PartRefList.refDataList;
            if (all == null || all.Count == 0)
            {
                GameCommon.ShowPopTip("No part exchange table data.", Vector2.zero);
                return false;
            }

            List<EventPart2PartRefObj> configs =
                all.FindAll(x => x.floor == floor && x.partLevel == sacrifice.partLevel);
            if (configs == null || configs.Count == 0)
                configs = all.FindAll(x => x.floor == floor);
            if (configs == null || configs.Count == 0)
            {
                GameCommon.ShowPopTip("No part exchange config for this floor.", Vector2.zero);
                return false;
            }

            EventPart2PartRefObj cfg = configs[Random.Range(0, configs.Count)];
            if (cfg.partList == null || cfg.partList.Count == 0)
            {
                GameCommon.ShowPopTip("Part exchange pool is empty.", Vector2.zero);
                return false;
            }

            if (!WeightedBootyPickHelper.TryPickOne(cfg.partList, out var booty) || booty == null)
            {
                GameCommon.ShowPopTip("Part exchange weight pick failed.", Vector2.zero);
                return false;
            }

            PartLevelRefObj levelRefObj =
                SCRefDataMgr.instance.partLevelRefList.refDataList.Find(x => x.id == booty.partLevelId);
            if (levelRefObj == null)
                return false;
            PartRefObj partRefObj = SCRefDataMgr.instance.partRefList.refDataList.Find(x => x.id == levelRefObj.partId);
            if (partRefObj == null)
                return false;

            bag.Remove(sacrifice);
            bag.Add(new PartInfo(partRefObj, false, levelRefObj.partLevel));
            GameCommon.ShowPopTip("获得" + partRefObj.partName, Vector2.zero);
            return true;
        }
    }
}
