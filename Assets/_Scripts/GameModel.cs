using GameCore.Battle;
using GameCore.Helpers;
using GameCore.RefData;
using GameCore.UI;
using SCFrame;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// 游戏模型 放所有的运行时数据并提供数据处理的相关方法
    /// </summary>
    public partial class GameModel : Singleton<GameModel>
    {
        public long rollStoreId; //进入商店节点后roll到的商店id
        public long rollEventId;//进入事件节点后roll到的事件id

        public PlayerInfo playerInfo;
        public List<FaceGridInfo> playerFaceGridInfoList;//玩家当前脸部格子信息列表
        public List<GameObject> playerFaceGridGOList;//玩家当前脸部格子物体列表

        public EnemyInfo curEnemyInfo;
        public List<FaceGridInfo> enemyFaceGridInfoList;//敌人当前脸部格子信息列表

        public ETurnOwnerType curTurnOwner;//当前行动方
        public int curActivePartIndex;//当前行动的部位索引

        public override void OnInitialize()
        {
            //初始化数据从配表读取
            PlayerRefObj playerRefObj = SCRefDataMgr.instance.playerConfigRefObj;
            if (playerRefObj == null)
                return;
            playerInfo = new PlayerInfo(playerRefObj);


            PartEffectObj partEffectObj = null;
            PartInfo info = null;
            PartRefObj partRefObj = null;
            PartLevelRefObj levelRefObj = null;
            for (int i = 0; i < playerRefObj.initPartList.Count; i++)
            {
                partEffectObj = playerRefObj.initPartList[i];
                if (partEffectObj == null)
                    continue;
                for(int j =0;j< partEffectObj.partAmount;j++)
                {
                    levelRefObj = SCRefDataMgr.instance.partLevelRefList.refDataList.Find(x => x.id == partEffectObj.partId);
                    if (levelRefObj == null)
                        continue;
                    partRefObj = SCRefDataMgr.instance.partRefList.refDataList.Find(x => x.id == levelRefObj.partId);
                    if (partRefObj == null)
                        continue;
                    info = new PartInfo(partRefObj,false, levelRefObj.partLevel);
                    playerInfo.bagPartInfoList.Add(info);
                }
            }
            
        }

        public void PlayerHeal(int _amount)
        {
            if (_amount <= 0)
                return;
            playerInfo.currentHealth = Mathf.Clamp(playerInfo.currentHealth + _amount, 0, playerInfo.maxHealth);
            SCMsgCenter.SendMsg(SCMsgConst.PLAYER_HEAL);
        }

        public void PlayerTakeDamage(int _amount)
        {
            if (_amount <= 0)
                return;
            playerInfo.currentHealth = Mathf.Clamp(playerInfo.currentHealth - _amount, 0, playerInfo.maxHealth);
            SCMsgCenter.SendMsg(SCMsgConst.PLAYER_HURT);
            if(playerInfo.currentHealth == 0)
                BattleManager.instance.TerminateBattle(false);
        }

        public void EnemyHeal(int _amount)
        {
            if (_amount <= 0)
                return;
            curEnemyInfo.currentHealth = Mathf.Clamp(curEnemyInfo.currentHealth + _amount, 0, curEnemyInfo.maxHealth);
            SCMsgCenter.SendMsg(SCMsgConst.ENEMY_HEAL);

        }

        public void EnemyTakeDamage(int _amount)
        {
            if (_amount <= 0)
                return;
            curEnemyInfo.currentHealth = Mathf.Clamp(curEnemyInfo.currentHealth - _amount, 0, curEnemyInfo.maxHealth);
            SCMsgCenter.SendMsg(SCMsgConst.ENEMY_HURT);
            if (curEnemyInfo.currentHealth == 0)
                BattleManager.instance.TerminateBattle(true);
        }

        public void PartTakeDamage(PartInfo _partInfo, PartInfo _senderInfo, int _amount)
        {
            if (_amount <= 0)
                return;
            _partInfo.currentHealth = Mathf.Clamp(_partInfo.currentHealth - _amount, 0, _partInfo.maxHealth);
            SCMsgCenter.SendMsg(SCMsgConst.PART_HURT, _partInfo, _amount);
            _partInfo.TriggerGetHitLogic(_senderInfo, _amount);
            if (_partInfo.currentHealth == 0)
            {
                SCMsgCenter.SendMsg(SCMsgConst.PART_DIE,_partInfo);
                if (_partInfo.isEnemyPart)
                {
                    curEnemyInfo.battlePartInfoList.Remove(_partInfo);
                    BattleManager.instance.RemovePartFromList(false, _partInfo);
                    SCMsgCenter.SendMsg(SCMsgConst.BATTLE_ENEMY_PART_ORDER_CHG);

                }
                else
                {
                    playerInfo.battlePartInfoList.Remove(_partInfo);
                    BattleManager.instance.RemovePartFromList(true, _partInfo);
                    SCMsgCenter.SendMsg(SCMsgConst.BATTLE_PLAYER_PART_ORDER_CHG);

                }
            }
        }
        public void PartHeal(PartInfo _partInfo, int _amount)
        {
            if (_amount <= 0)
                return;
            _partInfo.currentHealth = Mathf.Clamp(_partInfo.currentHealth + _amount, 0, _partInfo.maxHealth);
            SCMsgCenter.SendMsg(SCMsgConst.PART_HEAL, _partInfo, _amount);

        }

        public List<Vector2Int> GetPlaceFaceOccupyPosList(GameObject _hitGridGO, Vector3 _mousePos, List<Vector2Int> _localGridList)
        {
            return FacePlacementHelper.GetPlaceFaceOccupyPosList(
                _hitGridGO, _mousePos, _localGridList,
                playerFaceGridInfoList, playerFaceGridGOList,
                SCGame.instance.gameCamera);
        }

        public List<Vector2Int> GetPlaceFaceEffectPosList(List<Vector2Int> _localEffectPosList, List<Vector2Int> _faceOccupyPosList, List<Vector2Int> _localOccupyPosList)
        {
            return FacePlacementHelper.GetPlaceFaceEffectPosList(_localEffectPosList, _faceOccupyPosList, _localOccupyPosList);
        }

        public bool CanPlacePart(GameObject _hitGridGO, Vector3 _mousePos, List<Vector2Int> _localGridList)
        {
            return FacePlacementHelper.CanPlacePart(_hitGridGO, _mousePos, _localGridList, playerFaceGridInfoList, playerFaceGridGOList, SCGame.instance.gameCamera);
        }

        public bool CanPlacePart(List<Vector2Int> _faceOccupyPosList)
        {
            return FacePlacementHelper.CanPlacePart(_faceOccupyPosList, playerFaceGridInfoList);
        }

        public void SetGridsEmpty(List<Vector2Int> _posList)
        {
            if (_posList == null)
                return;
            for(int i =0;i<_posList.Count;i++)
            {
                FaceGridInfo info = playerFaceGridInfoList.Find(x => x.pos == _posList[i]);
                if (info == null)
                    continue;
                info.SetEmpty();
            }
        }
        public void SetAllPlayerPart2Bag()
        {
            for (int i = 0; i < playerInfo.battlePartInfoList.Count; i++)
            {
                playerInfo.battlePartInfoList[i].ResetToBag();
            }
            for (int i = 0; i < playerInfo.deckPartInfoList.Count; i++)
            {
                playerInfo.deckPartInfoList[i].ResetToBag();
            }
        }

        public void SetEnemyEmpty()
        {
            curEnemyInfo = null;
        }

        public int GetPlayerBattleOrderByPartInfo(PartInfo _info)
        {
            return BattleOrderHelper.GetBattleOrderByPartInfo(playerInfo.battlePartInfoList, _info);
        }

        public int GetEnemyBattleOrderByPartInfo(PartInfo _info)
        {
            if (curEnemyInfo == null) return -1;
            return BattleOrderHelper.GetBattleOrderByPartInfo(curEnemyInfo.battlePartInfoList, _info);
        }

        public void SortEnemyBattleOrder()
        {
            if (curEnemyInfo != null)
                BattleOrderHelper.SortBattleOrder(curEnemyInfo.battlePartInfoList);
        }
        public void RollRandomShop()
        {
            List<StoreRefObj> storeRefList = SCRefDataMgr.instance.storeRefList.refDataList.Where(refObj => refObj.floor == playerInfo.playerFloor).ToList();
            long id = storeRefList[Random.Range(0, storeRefList.Count)].id;
            rollStoreId = id;
        }

        public void RollEventId()
        {
            EventRefObj eventRefObj = SCRefDataMgr.instance.eventRefList.refDataList.Find(refObj => refObj.floor == playerInfo.playerFloor);
            long id = eventRefObj.eventList[Random.Range(0, eventRefObj.eventList.Count)];
            rollEventId = id;
        }

        public void RollBattleOrder()
        {
            float randomNum = RandomUtility.GetRandomGenerator(EModuleType.COMBAT).Next(1, 100) / 100f;
            curTurnOwner = randomNum < 0.5f ? ETurnOwnerType.PLAYER : ETurnOwnerType.ENEMY;
        }

        public void GenerateNewBattle(bool _isBoss = false,long _id = -1)
        {
            playerInfo.ClearListForNewBattle();

            if (playerInfo.bagPartInfoList != null)
            {
                foreach (var part in playerInfo.bagPartInfoList)
                {
                    if (part.currentHealth > 0)
                    {
                        playerInfo.deckPartInfoList.Add(part);
                    }
                }
            }
            PlayerDrawParts(GameConst.DRAW_CARD_COUNT_PER_TURN);
            if(_isBoss)
                GenerateRandomEnemy(_id);
            else
                GenerateRandomEnemy();

            SCMsgCenter.SendMsg(SCMsgConst.NEW_GANE_START);
        }

        public void DealNextTurn()
        {
            PartDeckHelper.RecycleBusyToDeck(playerInfo.deckPartInfoList, playerInfo.busyPartInfoList);
            PartDeckHelper.RecycleBattleToBusy(playerInfo.battlePartInfoList, playerInfo.busyPartInfoList);
            int playerDrawCnt = Mathf.Min(GameConst.DRAW_CARD_COUNT_PER_TURN, GameConst.BUSY_CARD_MAX_COUNT - playerInfo.busyPartInfoList.Count);
            PlayerDrawParts(playerDrawCnt);
            foreach (var info in playerFaceGridInfoList) info.SetEmpty();

            PartDeckHelper.RecycleBusyToDeck(curEnemyInfo.deckPartInfoList, curEnemyInfo.busyPartInfoList);
            PartDeckHelper.RecycleBattleToBusy(curEnemyInfo.battlePartInfoList, curEnemyInfo.busyPartInfoList);
            int enemyDrawCnt = Mathf.Min(GameConst.DRAW_CARD_COUNT_PER_TURN, GameConst.BUSY_CARD_MAX_COUNT - curEnemyInfo.busyPartInfoList.Count);
            EnemyDrawParts(enemyDrawCnt);
            foreach (var info in enemyFaceGridInfoList) info.SetEmpty();

            EnemyLayoutGenerator.GenerateLayout(curEnemyInfo, enemyFaceGridInfoList);
            RollBattleOrder();
        }

        public void PlayerDrawParts(int _count)
        {
            if (playerInfo == null) return;
            PartDeckHelper.DrawParts(playerInfo.deckPartInfoList, playerInfo.busyPartInfoList, _count, GameConst.BUSY_CARD_MAX_COUNT);
        }

        public void EnemyDrawParts(int _count)
        {
            if (curEnemyInfo == null) return;
            PartDeckHelper.DrawParts(curEnemyInfo.deckPartInfoList, curEnemyInfo.busyPartInfoList, _count, GameConst.BUSY_CARD_MAX_COUNT);
        }

        /// <summary>
        /// 部位「效果范围」覆盖到的所有脸部部位（同时检查己方脸图与敌方脸图上的格子，去重）。
        /// 用于预览或不限定敌我词条时的范围展示。
        /// </summary>
        public List<PartInfo> GetPartPreviewTargetPartList(PartInfo _partInfo)
        {
            if (_partInfo == null)
                return null;
            if (_partInfo.curEffectFacePosList == null || _partInfo.curEffectFacePosList.Count == 0)
                return new List<PartInfo>();

            List<PartInfo> resList = new List<PartInfo>();
            for (int i =0;i<_partInfo.entryInfoList.Count;i++)
            {
                resList.AddRange(GetEntryPreviewTargetPartList(_partInfo, _partInfo.entryInfoList[i]));
            }
            return resList;
        }

        /// <summary>
        /// 根据词条类型返回该次效果会作用到的部位列表
        /// </summary>
        /// <param name="_caster">施放该词条的部位</param>
        /// <param name="_entryInfo">词条</param>
        /// <param name="_ctx">受击等上下文；反射/传递类词条需要 senderPart</param>
        public List<PartInfo> GetEntryRealTargetPartList(PartInfo _caster, EntryInfo _entryInfo, PartEffectContext _ctx = default)
        {
            if (_entryInfo == null || _caster == null || !_caster.isOnFace)
                return null;
            if (_caster.curEffectFacePosList == null || _caster.curEffectFacePosList.Count == 0)
                return new List<PartInfo>();

            bool sameSide = _caster.isEnemyPart;
            var allyGrid = sameSide ? enemyFaceGridInfoList : playerFaceGridInfoList;
            var enemyGrid = sameSide ? playerFaceGridInfoList : enemyFaceGridInfoList;

            switch (_entryInfo.attributeType)
            {
                // 作用己方脸上的部位（效果格与己方脸重叠）
                case EAttributeType.CLEAR_DEFULL:
                case EAttributeType.TRIGGER_MORE:
                case EAttributeType.DAMAGE_MULTIPILER:
                case EAttributeType.HEAL_ALL_PART:
                case EAttributeType.HEAL_WEAK_PART:
                case EAttributeType.TRIGGER_CHANCE_UP:
                case EAttributeType.CLEAR_BAD_SKIN:
                case EAttributeType.SELF_GET_BUFF:
                case EAttributeType.SELF_BUFF_MULTIPLIER:
                case EAttributeType.CLEAR_SELF_BLEED_AND_HEAL_SELF:
                case EAttributeType.INCREASE_ADD_BURN:
                    return CollectPartsInEffectArea(_caster, allyGrid);

                // 作用敌方脸上的部位（效果格与敌方脸重叠）
                case EAttributeType.ATTACK:
                case EAttributeType.ENEMY_GET_BUFF:
                case EAttributeType.ENEMY_BUFF_MULTIPLIER:
                case EAttributeType.PART_LOSE_TURN:
                case EAttributeType.CLEAR_ENEMY_BLEED_AND_HEAL_PART:
                case EAttributeType.ATTACK_BY_ENEMY_BLEED:
                case EAttributeType.CHANGE_FAT_2_BURN:
                case EAttributeType.SPREAD_BURN:
                    return CollectPartsInEffectArea(_caster, enemyGrid);

                // 仅针对攻击来源部位
                case EAttributeType.REFLECT:
                case EAttributeType.SEND_BLEED_BY_GET_HIT:
                case EAttributeType.SEND_ALL_FAT_BY_GET_HIT:
                    if (_ctx.senderPart == null)
                        return new List<PartInfo>();
                    return new List<PartInfo> { _ctx.senderPart };

                // 对本体生命造成伤害，无部位列表
                case EAttributeType.REAL_ATTACK:
                case EAttributeType.GET_COIN:
                case EAttributeType.GET_COIN_BY_ATTACK:
                case EAttributeType.ATTACK_BY_COIN:
                    return new List<PartInfo>();

                default:
                    return new List<PartInfo>();
            }
        }

        /// <summary>
        /// 根据词条类型返回效果会作用到的部位列表预览
        /// </summary>
        /// <param name="_caster">施放该词条的部位</param>
        /// <param name="_entryInfo">词条</param>
        /// <param name="_ctx">受击等上下文；反射/传递类词条需要 senderPart</param>
        public List<PartInfo> GetEntryPreviewTargetPartList(PartInfo _caster, EntryInfo _entryInfo)
        {
            if (_entryInfo == null || _caster == null)
                return null;
            if (_caster.curEffectFacePosList == null || _caster.curEffectFacePosList.Count == 0)
                return new List<PartInfo>();

            bool sameSide = _caster.isEnemyPart;
            var allyGrid = sameSide ? enemyFaceGridInfoList : playerFaceGridInfoList;
            var enemyGrid = sameSide ? playerFaceGridInfoList : enemyFaceGridInfoList;

            switch (_entryInfo.attributeType)
            {
                // 作用己方脸上的部位（效果格与己方脸重叠）
                case EAttributeType.CLEAR_DEFULL:
                case EAttributeType.TRIGGER_MORE:
                case EAttributeType.DAMAGE_MULTIPILER:
                case EAttributeType.HEAL_ALL_PART:
                case EAttributeType.HEAL_WEAK_PART:
                case EAttributeType.TRIGGER_CHANCE_UP:
                case EAttributeType.CLEAR_BAD_SKIN:
                case EAttributeType.SELF_GET_BUFF:
                case EAttributeType.SELF_BUFF_MULTIPLIER:
                case EAttributeType.CLEAR_SELF_BLEED_AND_HEAL_SELF:
                case EAttributeType.INCREASE_ADD_BURN:
                    return CollectPartsInEffectArea(_caster, allyGrid);

                // 作用敌方脸上的部位（效果格与敌方脸重叠）
                case EAttributeType.ATTACK:
                case EAttributeType.ENEMY_GET_BUFF:
                case EAttributeType.ENEMY_BUFF_MULTIPLIER:
                case EAttributeType.PART_LOSE_TURN:
                case EAttributeType.CLEAR_ENEMY_BLEED_AND_HEAL_PART:
                case EAttributeType.ATTACK_BY_ENEMY_BLEED:
                case EAttributeType.CHANGE_FAT_2_BURN:
                case EAttributeType.SPREAD_BURN:
                case EAttributeType.REFLECT:
                case EAttributeType.SEND_BLEED_BY_GET_HIT:
                case EAttributeType.SEND_ALL_FAT_BY_GET_HIT:
                    return CollectPartsInEffectArea(_caster, enemyGrid);

                // 对本体生命造成伤害，无部位列表
                case EAttributeType.REAL_ATTACK:
                case EAttributeType.GET_COIN:
                case EAttributeType.GET_COIN_BY_ATTACK:
                case EAttributeType.ATTACK_BY_COIN:
                    return new List<PartInfo>();

                default:
                    return new List<PartInfo>();
            }
        }

        /// <summary>
        /// 遍历施法部位当前效果格，收集指定脸图上占据格子的部位（去重）。
        /// </summary>
        private static List<PartInfo> CollectPartsInEffectArea(PartInfo _caster, List<FaceGridInfo> _gridInfoList)
        {
            var result = new List<PartInfo>();
            if (_caster?.curEffectFacePosList == null || _gridInfoList == null)
                return result;

            foreach (var pos in _caster.curEffectFacePosList)
            {
                var gridInfo = _gridInfoList.Find(x => x.pos == pos);
                if (gridInfo?.hasPart == true && gridInfo.ownerPart != null && !result.Contains(gridInfo.ownerPart))
                    result.Add(gridInfo.ownerPart);
            }
            return result;
        }
    }

}
