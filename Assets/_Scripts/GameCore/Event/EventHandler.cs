using GameCore.Helpers;
using GameCore.RefData;
using GameCore.UI;
using SCFrame;
using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameCore
{
    public static class EventHandler
    {
        public static void DealEvent(EEventType _eventType)
        {
            switch (_eventType)
            {
                case EEventType.NONE:
                    break;
                case EEventType.BLOOD_2_PART_HIGH:
                    ExecuteBlood2Part(EEventType.BLOOD_2_PART_HIGH);
                    break;
                case EEventType.BLOOD_2_PART_MIDDLE:
                    ExecuteBlood2Part(EEventType.BLOOD_2_PART_MIDDLE);
                    break;
                case EEventType.BLOOD_2_PART_LOW:
                    ExecuteBlood2Part(EEventType.BLOOD_2_PART_LOW);
                    break;
                case EEventType.PART_2_PART:
                    {
                        if (GameModel.instance.playerInfo.bagPartInfoList == null
                            || GameModel.instance.playerInfo.bagPartInfoList.Count == 0)
                        {
                            GameCommon.ShowPopTip("????", Vector2.zero);
                            return;
                        }
                        UICoreMgr.instance.AddNode(new UINodeEventPartExchange(SCUIShowType.ADDITION));
                    }
                    break;
                case EEventType.TREASURE_COIN:
                    {
                        AudioMgr.instance.PlaySfx("sfx_money");
                        EventGetMoneyRefObj getMoneyRefObj = SCRefDataMgr.instance.eventGetMoneyRefList.refDataList.
                            Find(x => x.floor == GameModel.instance.playerInfo.playerFloor);
                        if (getMoneyRefObj == null)
                            return;
                        GameModel.instance.playerInfo.playerMoney += getMoneyRefObj.money;
                        GameCommon.ShowPopTip("get" + getMoneyRefObj.money,Vector2.zero);
                    }
                    break;
                case EEventType.TREASURE_PART:
                    {
                        EventGetPartRefObj getPartRefObj = SCRefDataMgr.instance.eventGetPartRefList.refDataList.
                            Find(x => x.floor == GameModel.instance.playerInfo.playerFloor);
                        if (getPartRefObj == null)
                            return;
                        long levelId = getPartRefObj.partList[Random.Range(0, getPartRefObj.partList.Count)];
                        PartLevelRefObj levelRefObj = SCRefDataMgr.instance.partLevelRefList.refDataList.Find(x => x.id == levelId);
                        if (levelRefObj == null)
                            return;
                        PartRefObj partRefObj = SCRefDataMgr.instance.partRefList.refDataList.Find(x => x.id == levelRefObj.partId);
                        GameModel.instance.playerInfo.bagPartInfoList.Add(new PartInfo(partRefObj, false, levelRefObj.partLevel));
                        GameCommon.ShowPopTip("get" + partRefObj.partName, Vector2.zero);
                    }
                    break;
                case EEventType.TRAP_BATTLE:
                    {
                        int floor = GameModel.instance.playerInfo.playerFloor;
                        List<EnemyRefObj> pool = SCRefDataMgr.instance.enemyRefList.refDataList
                            .Where(e => e.floor == floor && e.battleType == EBattleType.EVENT && !e.isBoss)
                            .ToList();
                        if (pool == null || pool.Count == 0)
                        {
                            SCDebugHelper.LogWarning(
                                $"[TRAP_BATTLE] No enemy with battleType=EVENT on floor {floor}. Check enemy sheet.");
                            GameCommon.ShowPopTip(
                                "\u672c\u5c42\u672a\u914d\u7f6e EVENT \u7c7b\u578b\u654c\u4eba\uff08\u9677\u9631\u6218\u6597\uff09\u3002",
                                Vector2.zero);
                            return;
                        }
                        EnemyRefObj pick = pool[Random.Range(0, pool.Count)];
                        AudioMgr.instance.PlaySfx("sfx_click");
                        TVSwitchTransition.Run(() =>
                        {
                            UICoreMgr.instance.RemoveNode(nameof(UINodeEventPartExchange));
                            UICoreMgr.instance.RemoveNode(nameof(UINodeEvent));
                            GameModel.instance.RollBattleOrder();
                            UICoreMgr.instance.AddNode(new UINodeMaskCombine(SCUIShowType.FULL));
                            GameModel.instance.GenerateNewBattle(false, pick.id);
                            UICoreMgr.instance.AddNode(new UINodeGuideBattle(SCUIShowType.ADDITION));
                        });
                    }
                    break;
            }
        }

        private static void ExecuteBlood2Part(EEventType poolType)
        {
            EventBlood2PartRefObj blood2PartRefObj = SCRefDataMgr.instance.eventBlood2PartRefList.refDataList
                .Find(x => x.floor == GameModel.instance.playerInfo.playerFloor && x.eventType == poolType);
            if (blood2PartRefObj == null)
                return;
            if (blood2PartRefObj.partList == null || blood2PartRefObj.partList.Count == 0)
                return;
            if (!WeightedBootyPickHelper.TryPickOne(blood2PartRefObj.partList, out var booty) || booty == null)
                return;
            PartLevelRefObj levelRefObj =
                SCRefDataMgr.instance.partLevelRefList.refDataList.Find(x => x.id == booty.partLevelId);
            if (levelRefObj == null)
                return;
            PartRefObj partRefObj =
                SCRefDataMgr.instance.partRefList.refDataList.Find(x => x.id == levelRefObj.partId);
            if (partRefObj == null)
                return;
            GameModel.instance.playerInfo.bagPartInfoList.Add(new PartInfo(partRefObj, false, levelRefObj.partLevel));
            string tipPrefix = poolType == EEventType.BLOOD_2_PART_HIGH ? "???" : "get";
            GameCommon.ShowPopTip(tipPrefix + partRefObj.partName, Vector2.zero);
            GameModel.instance.PlayerTakeDamage(blood2PartRefObj.blood);
        }
    }

}

