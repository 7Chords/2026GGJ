using GameCore;
using GameCore.RefData;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Helpers
{
    public static class BattleBootyHelper
    {
        public const int DefaultOfferCount = 3;

        public static List<PartInfo> RollBootyOffers(
            EnemyRefObj enemyRef,
            int offerCount = DefaultOfferCount,
            HashSet<long> excludePartLevelIds = null)
        {
            if (enemyRef?.bootyList == null || offerCount <= 0)
                return new List<PartInfo>();

            return RandomSelectBooty(enemyRef.bootyList, offerCount, excludePartLevelIds);
        }

        public static List<List<PartInfo>> RollDistinctBootyOfferGroups(
            EnemyRefObj enemyRef,
            int groupCount,
            int offerPerGroup = DefaultOfferCount)
        {
            var groups = new List<List<PartInfo>>();
            if (enemyRef == null || groupCount <= 0)
                return groups;

            var usedPartLevelIds = new HashSet<long>();
            for (int i = 0; i < groupCount; i++)
            {
                List<PartInfo> offers = RollBootyOffers(enemyRef, offerPerGroup, usedPartLevelIds);
                groups.Add(offers);

                for (int j = 0; j < offers.Count; j++)
                {
                    long partLevelId = GetPartLevelId(offers[j]);
                    if (partLevelId != 0)
                        usedPartLevelIds.Add(partLevelId);
                }
            }

            return groups;
        }

        public static bool HasBootyOffers(EnemyRefObj enemyRef)
        {
            if (enemyRef?.bootyList == null || enemyRef.bootyList.Count == 0)
                return false;

            for (int i = 0; i < enemyRef.bootyList.Count; i++)
            {
                if (IsValidBooty(enemyRef.bootyList[i]))
                    return true;
            }

            return false;
        }

        public static long GetPartLevelId(PartInfo partInfo)
        {
            if (partInfo?.levelRefObj != null)
                return partInfo.levelRefObj.id;

            if (partInfo?.partRefObj == null)
                return 0;

            PartLevelRefObj levelRefObj = SCRefDataMgr.instance.partLevelRefList.refDataList.Find(x =>
                x.partId == partInfo.partRefObj.id && x.partLevel == partInfo.partLevel);
            return levelRefObj?.id ?? 0;
        }

        public static List<PartInfo> RandomSelectBooty(
            List<BootyEffectObj> sourceList,
            int count,
            HashSet<long> excludePartLevelIds = null)
        {
            var resultList = new List<PartInfo>();
            if (sourceList == null || sourceList.Count == 0 || count <= 0)
                return resultList;

            List<BootyEffectObj> fullPool = buildValidPool(sourceList);
            if (fullPool.Count == 0)
                return resultList;

            List<BootyEffectObj> drawPool = resolveDrawPool(fullPool, excludePartLevelIds, count);
            bool withReplacement = drawPool.Count < count;

            if (!withReplacement)
            {
                var tempList = new List<BootyEffectObj>(drawPool);
                for (int i = 0; i < count; i++)
                {
                    BootyEffectObj picked = weightedPickOne(tempList);
                    if (picked == null)
                        break;

                    AddBootyToResult(picked, resultList);
                    tempList.Remove(picked);
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    BootyEffectObj picked = weightedPickOne(drawPool);
                    if (picked == null)
                        break;

                    AddBootyToResult(picked, resultList);
                }
            }

            return resultList;
        }

        private static List<BootyEffectObj> buildValidPool(List<BootyEffectObj> sourceList)
        {
            var fullPool = new List<BootyEffectObj>();
            for (int i = 0; i < sourceList.Count; i++)
            {
                BootyEffectObj booty = sourceList[i];
                if (IsValidBooty(booty))
                    fullPool.Add(booty);
            }

            return fullPool;
        }

        private static List<BootyEffectObj> resolveDrawPool(
            List<BootyEffectObj> fullPool,
            HashSet<long> excludePartLevelIds,
            int needCount)
        {
            if (excludePartLevelIds == null || excludePartLevelIds.Count == 0)
                return fullPool;

            var distinctPool = new List<BootyEffectObj>();
            for (int i = 0; i < fullPool.Count; i++)
            {
                if (!excludePartLevelIds.Contains(fullPool[i].partLevelId))
                    distinctPool.Add(fullPool[i]);
            }

            // Prefer unused partLevelIds across option groups; fall back to full bootyList when not enough.
            return distinctPool.Count >= needCount ? distinctPool : fullPool;
        }

        private static BootyEffectObj weightedPickOne(List<BootyEffectObj> pool)
        {
            if (pool == null || pool.Count == 0)
                return null;
            if (pool.Count == 1)
                return pool[0];

            float totalChance = 0f;
            for (int i = 0; i < pool.Count; i++)
                totalChance += Mathf.Max(0f, pool[i].dropChance);

            if (totalChance <= 0f)
                return pool[Random.Range(0, pool.Count)];

            float randomValue = Random.Range(0f, totalChance);
            float currentChance = 0f;
            for (int i = 0; i < pool.Count; i++)
            {
                currentChance += Mathf.Max(0f, pool[i].dropChance);
                if (randomValue <= currentChance)
                    return pool[i];
            }

            return pool[pool.Count - 1];
        }

        private static bool IsValidBooty(BootyEffectObj booty)
        {
            if (booty == null)
                return false;

            PartLevelRefObj partLevelRefObj = SCRefDataMgr.instance.partLevelRefList.refDataList
                .Find(x => x.id == booty.partLevelId);
            if (partLevelRefObj == null)
                return false;

            PartRefObj partRefObj = SCRefDataMgr.instance.partRefList.refDataList
                .Find(x => x.id == partLevelRefObj.partId);
            return partRefObj != null;
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
