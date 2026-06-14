using GameCore;
using GameCore.RefData;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Helpers
{
    public static class BattleBootyHelper
    {
        public const int DefaultOfferCount = 3;

        public static List<PartInfo> RollBootyOffers(EnemyRefObj enemyRef, int offerCount = DefaultOfferCount)
        {
            if (enemyRef?.bootyList == null || offerCount <= 0)
                return new List<PartInfo>();

            return RandomSelectBooty(enemyRef.bootyList, offerCount);
        }

        public static bool HasBootyOffers(EnemyRefObj enemyRef)
        {
            if (enemyRef?.bootyList == null || enemyRef.bootyList.Count == 0)
                return false;

            for (int i = 0; i < enemyRef.bootyList.Count; i++)
            {
                BootyEffectObj booty = enemyRef.bootyList[i];
                if (booty == null)
                    continue;

                PartLevelRefObj partLevelRefObj = SCRefDataMgr.instance.partLevelRefList.refDataList
                    .Find(x => x.id == booty.partLevelId);
                if (partLevelRefObj == null)
                    continue;

                PartRefObj partRefObj = SCRefDataMgr.instance.partRefList.refDataList
                    .Find(x => x.id == partLevelRefObj.partId);
                if (partRefObj != null)
                    return true;
            }

            return false;
        }

        public static List<PartInfo> RandomSelectBooty(List<BootyEffectObj> sourceList, int count)
        {
            var resultList = new List<PartInfo>();
            if (sourceList == null || sourceList.Count == 0 || count <= 0)
                return resultList;

            var tempList = new List<BootyEffectObj>(sourceList);
            int actualCount = Mathf.Min(count, tempList.Count);

            for (int i = 0; i < actualCount; i++)
            {
                if (tempList.Count == 0)
                    break;

                float totalChance = 0f;
                for (int j = 0; j < tempList.Count; j++)
                    totalChance += Mathf.Max(0f, tempList[j].dropChance);

                if (totalChance <= 0f)
                {
                    int randomIndex = Random.Range(0, tempList.Count);
                    AddBootyToResult(tempList[randomIndex], resultList);
                    tempList.RemoveAt(randomIndex);
                    continue;
                }

                float randomValue = Random.Range(0f, totalChance);
                float currentChance = 0f;
                int selectedIndex = -1;

                for (int j = 0; j < tempList.Count; j++)
                {
                    currentChance += Mathf.Max(0f, tempList[j].dropChance);
                    if (randomValue <= currentChance)
                    {
                        selectedIndex = j;
                        break;
                    }
                }

                if (selectedIndex < 0)
                    continue;

                AddBootyToResult(tempList[selectedIndex], resultList);
                tempList.RemoveAt(selectedIndex);
            }

            return resultList;
        }

        public static void AddBootyToResult(BootyEffectObj booty, List<PartInfo> resultList)
        {
            if (booty == null || resultList == null)
                return;

            PartLevelRefObj partLevelRefObj = SCRefDataMgr.instance.partLevelRefList.refDataList
                .Find(x => x.id == booty.partLevelId);
            if (partLevelRefObj == null)
                return;

            PartRefObj partRefObj = SCRefDataMgr.instance.partRefList.refDataList
                .Find(x => x.id == partLevelRefObj.partId);
            if (partRefObj == null)
                return;

            resultList.Add(new PartInfo(partRefObj, false, partLevelRefObj.partLevel));
        }
    }
}
