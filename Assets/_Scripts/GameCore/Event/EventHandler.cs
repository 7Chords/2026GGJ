using GameCore.RefData;
using System.Collections;
using System.Collections.Generic;
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
                    {

                    }
                    break;
                case EEventType.BLOOD_2_PART_MIDDLE:
                    {

                    }
                    break;
                case EEventType.BLOOD_2_PART_LOW:
                    {

                    }
                    break;
                case EEventType.PART_2_PART:
                    break;
                case EEventType.TREASURE_COIN:
                    {
                        AudioMgr.instance.PlaySfx("sfx_money");
                        EventGetMoneyRefObj getMoneyRefObj = SCRefDataMgr.instance.eventGetMoneyRefList.refDataList.
                            Find(x => x.floor == GameModel.instance.playerInfo.playerFloor);
                        if (getMoneyRefObj == null)
                            return;
                        GameModel.instance.playerInfo.playerMoney += getMoneyRefObj.money;
                        GameCommon.ShowPopTip("获得金钱×" + getMoneyRefObj.money,Vector2.zero);
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
                        GameCommon.ShowPopTip("获得" + partRefObj.partName, Vector2.zero);
                    }
                    break;
                case EEventType.TRAP_BATTLE:
                    break;
            }
        }
    }

}

