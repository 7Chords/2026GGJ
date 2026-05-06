using GameCore.Battle;
using GameCore.Data;
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

        /// <summary> 敌人脸部预设布局回合（每场战斗从 0 开始，每 DealNextTurn +1；超出预设条数时由数据库逻辑钳制） </summary>
        public int enemyFaceLayoutTurnIndex;

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
            SCMsgCenter.SendMsg(SCMsgConst.PLAYER_HURT, _amount);
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
            SCMsgCenter.SendMsg(SCMsgConst.ENEMY_HURT, _amount);
            EnemyPassiveController.OnEnemyBodyDamageApplied(_amount);
            if (curEnemyInfo.currentHealth == 0)
                BattleManager.instance.TerminateBattle(true);
        }

        public void PartTakeDamage(PartInfo _partInfo, PartInfo _senderInfo, int _amount)
        {
            _amount += BuffCombatModifiers.GetPreyExtraDamage(_partInfo);
            if (_amount <= 0)
                return;
            int hpBefore = _partInfo.currentHealth;
            int damageToPart = Mathf.Min(_amount, hpBefore);
            int overflowToBody = _amount - damageToPart;
            _partInfo.currentHealth = Mathf.Clamp(_partInfo.currentHealth - _amount, 0, _partInfo.maxHealth);
            SCMsgCenter.SendMsg(SCMsgConst.PART_HURT, _partInfo, damageToPart);
            _partInfo.TriggerGetHitLogic(_senderInfo, damageToPart);
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
            if (overflowToBody > 0)
            {
                if (_partInfo.isEnemyPart)
                    EnemyTakeDamage(overflowToBody);
                else
                    PlayerTakeDamage(overflowToBody);
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
            for (int i = 0; i < playerInfo.busyPartInfoList.Count; i++)
            {
                playerInfo.busyPartInfoList[i].ResetToBag();
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
            if (eventRefObj == null || eventRefObj.eventList == null || eventRefObj.eventList.Count == 0)
            {
                rollEventId = 0;
                return;
            }

            var pool = eventRefObj.eventList;
            int hp = playerInfo.currentHealth;
            int minHpForBlood2Part = GameConst.EVENT_BLOOD_2_PART_ROLL_MIN_CURRENT_HEALTH;

            var candidates = new List<long>();
            foreach (long dialogueId in pool)
            {
                EventDialogueRefObj dialogue = SCRefDataMgr.instance.eventDialogueRefList.refDataList.Find(x => x.id == dialogueId);
                if (dialogue == null)
                {
                    candidates.Add(dialogueId);
                    continue;
                }
                if (!IsBloodForPartEventType(dialogue.eventType) || hp >= minHpForBlood2Part)
                    candidates.Add(dialogueId);
            }

            if (candidates.Count == 0)
                rollEventId = pool[Random.Range(0, pool.Count)];
            else
                rollEventId = candidates[Random.Range(0, candidates.Count)];
        }

        private static bool IsBloodForPartEventType(EEventType _type)
        {
            return _type == EEventType.BLOOD_2_PART_HIGH
                || _type == EEventType.BLOOD_2_PART_MIDDLE
                || _type == EEventType.BLOOD_2_PART_LOW;
        }

        public void RollBattleOrder()
        {
            float randomNum = UnityEngine.Random.value;
            curTurnOwner = randomNum < 0.5f ? ETurnOwnerType.PLAYER : ETurnOwnerType.ENEMY;
        }

        private MapRefObj GetCurrentMapRow()
        {
            int floor = playerInfo != null ? playerInfo.playerFloor : 1;
            var list = SCRefDataMgr.instance?.mapRefList?.refDataList;
            if (list == null || list.Count == 0)
                return null;
            var row = list.Find(m => m != null && m.floor == floor);
            if (row != null)
                return row;
            // Fallback: keep game playable if map table is missing the exact floor row.
            return list.FindLast(m => m != null) ?? list[0];
        }

        public int GetPlayerMaxHandCards()
        {
            int v = GetCurrentMapRow()?.playerMaxHandCards ?? 0;
            return v > 0 ? v : GameConst.BUSY_CARD_MAX_COUNT;
        }

        public int GetPlayerMaxDrawCards()
        {
            int v = GetCurrentMapRow()?.playerMaxDrawCards ?? 0;
            return v > 0 ? v : GameConst.DRAW_CARD_COUNT_PER_TURN;
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
            PlayerDrawParts(GetPlayerMaxDrawCards());
            if (_isBoss)
                GenerateRandomEnemy(_id);
            else if (_id != -1)
                GenerateRandomEnemy(_id);
            else
                GenerateRandomEnemy();

            SCMsgCenter.SendMsg(SCMsgConst.NEW_GANE_START);
        }

        /// <summary>
        /// Buff types stripped from all in-combat parts (face, hand, deck) after both sides finish their queues once.
        /// Extend this list when adding more round-scoped battle buffs.
        /// </summary>
        private static readonly EBuffType[] BuffTypesClearedAfterFullBattleRound =
        {
            EBuffType.STRONG,
            EBuffType.PREY,
        };

        /// <summary>
        /// Clears every type in <see cref="BuffTypesClearedAfterFullBattleRound"/> for player and enemy battle lists.
        /// </summary>
        public void ClearBuffsAfterFullBattleRound()
        {
            ClearBuffTypesOnPartList(playerInfo?.battlePartInfoList, BuffTypesClearedAfterFullBattleRound);
            ClearBuffTypesOnPartList(playerInfo?.busyPartInfoList, BuffTypesClearedAfterFullBattleRound);
            ClearBuffTypesOnPartList(playerInfo?.deckPartInfoList, BuffTypesClearedAfterFullBattleRound);
            if (curEnemyInfo == null) return;
            ClearBuffTypesOnPartList(curEnemyInfo.battlePartInfoList, BuffTypesClearedAfterFullBattleRound);
            ClearBuffTypesOnPartList(curEnemyInfo.busyPartInfoList, BuffTypesClearedAfterFullBattleRound);
            ClearBuffTypesOnPartList(curEnemyInfo.deckPartInfoList, BuffTypesClearedAfterFullBattleRound);
        }

        private static void ClearBuffTypesOnPartList(List<PartInfo> parts, EBuffType[] types)
        {
            if (parts == null || types == null || types.Length == 0) return;
            for (int i = 0; i < parts.Count; i++)
            {
                var p = parts[i];
                if (p?.buffLogic == null) continue;
                for (int t = 0; t < types.Length; t++)
                    p.buffLogic.ClearBuff(types[t]);
            }
        }

        public void DealNextTurn()
        {
            PartDeckHelper.RecycleBusyToDeck(playerInfo.deckPartInfoList, playerInfo.busyPartInfoList);
            PartDeckHelper.RecycleBattleToBusy(playerInfo.battlePartInfoList, playerInfo.busyPartInfoList);
            int maxHand = GetPlayerMaxHandCards();
            int maxDraw = GetPlayerMaxDrawCards();
            int playerDrawCnt = Mathf.Min(maxDraw, maxHand - playerInfo.busyPartInfoList.Count);
            PlayerDrawParts(playerDrawCnt);
            foreach (var info in playerFaceGridInfoList) info.SetEmpty();

            PartDeckHelper.RecycleBusyToDeck(curEnemyInfo.deckPartInfoList, curEnemyInfo.busyPartInfoList);
            PartDeckHelper.RecycleBattleToBusy(curEnemyInfo.battlePartInfoList, curEnemyInfo.busyPartInfoList);
            // 合并手牌+脸上全部回牌堆，便于预设按回合从牌堆精确取牌
            EnemyLayoutPresetApplicator.MergeAllEnemyPartsIntoDeck(curEnemyInfo);
            if (enemyFaceGridInfoList != null)
            {
                foreach (var info in enemyFaceGridInfoList) info.SetEmpty();
            }

            RollBattleOrder();

            var encounterPreset = curEnemyInfo != null && curEnemyInfo.enemyRefObj != null
                ? ResourcesHelper.LoadAsset<EnemyLayoutPreset>(curEnemyInfo.enemyRefObj.layoutPresetName)
                : null;

            bool usePreset = encounterPreset != null;
            if (usePreset)
            {
                enemyFaceLayoutTurnIndex++;
                bool enemyActsFirst = curTurnOwner == ETurnOwnerType.ENEMY;
                EnemyTurnFaceLayout turnLayout = EnemyLayoutPresetApplicator.ResolveTurnFaceLayout(
                    encounterPreset, enemyFaceLayoutTurnIndex, enemyActsFirst);
                EnemyLayoutPresetApplicator.PrepareBusyFromTurnLayoutBestEffort(curEnemyInfo, turnLayout, out var resolvedSlots);
                EnemyLayoutPresetApplicator.ApplyTurnLayoutToFace(curEnemyInfo, enemyFaceGridInfoList, resolvedSlots);
            }
            else
            {
                SCDebugHelper.LogError(
                    $"[Enemy] id={curEnemyInfo.enemyRefObj.id} 缺少有效 layoutPresetName，回合布局无法应用。战斗中敌人应始终使用预设。");
            }
        }

        public void PlayerDrawParts(int _count)
        {
            if (playerInfo == null) return;
            PartDeckHelper.DrawParts(playerInfo.deckPartInfoList, playerInfo.busyPartInfoList, _count, GetPlayerMaxHandCards());
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
                // 受击类：展示「对方脸上、效果范围内、可对自己出伤的嘴巴」；真实结算仍由 ctx.senderPart 决定攻击来源
                case EAttributeType.REFLECT:
                case EAttributeType.SEND_BLEED_BY_GET_HIT:
                case EAttributeType.SEND_ALL_FAT_BY_GET_HIT:
                case EAttributeType.SEND_MOLD_2_BY_GET_HIT:
                    return FilterPartsMouthOnly(CollectPartsInEffectArea(_caster, enemyGrid));

                case EAttributeType.REAL_ATTACK:
                case EAttributeType.GET_COIN:
                case EAttributeType.GET_COIN_BY_ATTACK:
                case EAttributeType.ATTACK_BY_COIN:
                case EAttributeType.USE_HEAT_2_ATTACK_AGAIN:
                    return new List<PartInfo>();
            }

            List<PartInfo> raw;
            switch (_entryInfo.attributeType)
            {
                case EAttributeType.CLEAR_DEFULL:
                case EAttributeType.TRIGGER_MORE:
                case EAttributeType.DAMAGE_MULTIPILER:
                case EAttributeType.HEAL_ALL_PART:
                case EAttributeType.HEAL_WEAK_PART:
                case EAttributeType.TRIGGER_CHANCE_UP:
                case EAttributeType.CLEAR_BAD_SKIN:
                case EAttributeType.CLEAR_ALL_HATE_AND_MOUTH_GET_STRONG:
                case EAttributeType.SELF_GET_BUFF:
                case EAttributeType.SELF_MOUTH_GET_BUFF:
                case EAttributeType.SELF_BUFF_MULTIPLIER:
                case EAttributeType.CLEAR_SELF_BLEED_AND_HEAL_SELF:
                case EAttributeType.INCREASE_ADD_BURN:
                case EAttributeType.TRIGGER_MAX_MASS_EFFECT:
                case EAttributeType.CHANGE_BREEDING_MASS_2_OTHER:
                    raw = CollectPartsInEffectArea(_caster, allyGrid);
                    break;

                case EAttributeType.ATTACK:
                case EAttributeType.ENEMY_GET_BUFF:
                case EAttributeType.ENEMY_MOUTH_GET_BUFF:
                case EAttributeType.ENEMY_BUFF_MULTIPLIER:
                case EAttributeType.PART_LOSE_TURN:
                case EAttributeType.CLEAR_ENEMY_BLEED_AND_HEAL_PART:
                case EAttributeType.ATTACK_BY_ENEMY_BLEED:
                case EAttributeType.CHANGE_FAT_2_BURN:
                case EAttributeType.SPREAD_BURN:
                    raw = CollectPartsInEffectArea(_caster, enemyGrid);
                    break;

                default:
                    return new List<PartInfo>();
            }

            return ApplyEntryTargetFilters(_entryInfo.attributeType, _entryInfo, raw, false);
        }

        /// <summary>
        /// 根据词条类型返回效果会作用到的部位列表预览（与 GetEntryRealTargetPartList 同一套范围/过滤）。
        /// </summary>
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
                case EAttributeType.REFLECT:
                case EAttributeType.SEND_BLEED_BY_GET_HIT:
                case EAttributeType.SEND_ALL_FAT_BY_GET_HIT:
                case EAttributeType.SEND_MOLD_2_BY_GET_HIT:
                    return FilterPartsMouthOnly(CollectPartsInEffectArea(_caster, enemyGrid));

                case EAttributeType.REAL_ATTACK:
                case EAttributeType.GET_COIN:
                case EAttributeType.GET_COIN_BY_ATTACK:
                case EAttributeType.ATTACK_BY_COIN:
                case EAttributeType.USE_HEAT_2_ATTACK_AGAIN:
                    return new List<PartInfo>();
            }

            List<PartInfo> raw;
            switch (_entryInfo.attributeType)
            {
                case EAttributeType.CLEAR_DEFULL:
                case EAttributeType.TRIGGER_MORE:
                case EAttributeType.HEAL_ALL_PART:
                case EAttributeType.HEAL_WEAK_PART:
                case EAttributeType.TRIGGER_CHANCE_UP:
                case EAttributeType.CLEAR_BAD_SKIN:
                case EAttributeType.CLEAR_ALL_HATE_AND_MOUTH_GET_STRONG:
                case EAttributeType.SELF_GET_BUFF:
                case EAttributeType.SELF_MOUTH_GET_BUFF:
                case EAttributeType.SELF_BUFF_MULTIPLIER:
                case EAttributeType.CLEAR_SELF_BLEED_AND_HEAL_SELF:
                case EAttributeType.DAMAGE_MULTIPILER:
                case EAttributeType.INCREASE_ADD_BURN:
                case EAttributeType.TRIGGER_MAX_MASS_EFFECT:
                case EAttributeType.CHANGE_BREEDING_MASS_2_OTHER:
                    raw = CollectPartsInEffectArea(_caster, allyGrid);
                    break;

                case EAttributeType.ATTACK:
                case EAttributeType.ENEMY_GET_BUFF:
                case EAttributeType.ENEMY_MOUTH_GET_BUFF:
                case EAttributeType.ENEMY_BUFF_MULTIPLIER:
                case EAttributeType.PART_LOSE_TURN:
                case EAttributeType.CLEAR_ENEMY_BLEED_AND_HEAL_PART:
                case EAttributeType.ATTACK_BY_ENEMY_BLEED:
                case EAttributeType.CHANGE_FAT_2_BURN:
                case EAttributeType.SPREAD_BURN:
                    raw = CollectPartsInEffectArea(_caster, enemyGrid);
                    break;

                default:
                    return new List<PartInfo>();
            }

            return ApplyEntryTargetFilters(_entryInfo.attributeType, _entryInfo, raw, true);
        }

        /// <summary>
        /// 按词条真实逻辑缩小「效果范围内的部位」列表，使预览/提示与结算一致。
        /// </summary>
        private static List<PartInfo> ApplyEntryTargetFilters(EAttributeType _attr, EntryInfo _entry, List<PartInfo> _raw, bool _preview)
        {
            if (_raw == null)
                _raw = new List<PartInfo>();

            switch (_attr)
            {
                case EAttributeType.ENEMY_MOUTH_GET_BUFF:
                case EAttributeType.SELF_MOUTH_GET_BUFF:
                    return FilterPartsMouthOnly(_raw);

                case EAttributeType.DAMAGE_MULTIPILER:
                    return FilterPartsDamageMultiplierTargets(_raw);

                case EAttributeType.INCREASE_ADD_BURN:
                    return FilterPartsIncreaseAddBurnTargets(_raw);

                case EAttributeType.CHANGE_FAT_2_BURN:
                    return FilterPartsWithBuffType(_raw, EBuffType.FAT);

                // 结算需完整列表以比较最高燃烧层；预览只高亮会被扣层的部位
                case EAttributeType.SPREAD_BURN:
                    if (_preview)
                        return FilterPartsSpreadBurnVictims(_raw);
                    return _raw;

                case EAttributeType.ATTACK_BY_ENEMY_BLEED:
                case EAttributeType.CLEAR_ENEMY_BLEED_AND_HEAL_PART:
                case EAttributeType.CLEAR_SELF_BLEED_AND_HEAL_SELF:
                    return FilterPartsWithBuffType(_raw, EBuffType.BLEED);

                case EAttributeType.HEAL_WEAK_PART:
                    return FilterPartsHealWeakestOnly(_raw);

                case EAttributeType.SELF_BUFF_MULTIPLIER:
                case EAttributeType.ENEMY_BUFF_MULTIPLIER:
                    if (_entry?.attributeValueList == null || _entry.attributeValueList.Count < 1)
                        return _raw;
                    return FilterPartsWithBuffId(_raw, (long)_entry.attributeValueList[0]);

                default:
                    return _raw;
            }
        }

        private static List<PartInfo> FilterPartsMouthOnly(List<PartInfo> _parts)
        {
            var r = new List<PartInfo>();
            if (_parts == null) return r;
            for (int i = 0; i < _parts.Count; i++)
            {
                var p = _parts[i];
                if (p?.partRefObj != null && p.partRefObj.partType == EPartType.MOUTH)
                    r.Add(p);
            }
            return r;
        }

        private static List<PartInfo> FilterPartsDamageMultiplierTargets(List<PartInfo> _parts)
        {
            var r = new List<PartInfo>();
            if (_parts == null) return r;
            for (int i = 0; i < _parts.Count; i++)
            {
                var p = _parts[i];
                if (p?.partRefObj == null || p.partRefObj.partType != EPartType.MOUTH)
                    continue;
                if (p.entryInfoList == null) continue;
                for (int j = 0; j < p.entryInfoList.Count; j++)
                {
                    var e = p.entryInfoList[j];
                    if (e == null) continue;
                    if (e.attributeType == EAttributeType.ATTACK
                        || e.attributeType == EAttributeType.REAL_ATTACK
                        || e.attributeType == EAttributeType.ATTACK_BY_ENEMY_BLEED)
                    {
                        r.Add(p);
                        break;
                    }
                }
            }
            return r;
        }

        private static List<PartInfo> FilterPartsIncreaseAddBurnTargets(List<PartInfo> _parts)
        {
            var r = new List<PartInfo>();
            if (_parts == null) return r;
            for (int i = 0; i < _parts.Count; i++)
            {
                var p = _parts[i];
                if (p?.entryInfoList == null) continue;
                for (int j = 0; j < p.entryInfoList.Count; j++)
                {
                    var e = p.entryInfoList[j];
                    if (e == null) continue;
                    if (e.attributeType == EAttributeType.CHANGE_FAT_2_BURN || e.attributeType == EAttributeType.SPREAD_BURN)
                    {
                        r.Add(p);
                        break;
                    }
                }
            }
            return r;
        }

        private static List<PartInfo> FilterPartsWithBuffType(List<PartInfo> _parts, EBuffType _buffType)
        {
            var r = new List<PartInfo>();
            if (_parts == null) return r;
            for (int i = 0; i < _parts.Count; i++)
            {
                var p = _parts[i];
                if (p?.GetBuff(_buffType) != null)
                    r.Add(p);
            }
            return r;
        }

        private static List<PartInfo> FilterPartsWithBuffId(List<PartInfo> _parts, long _buffId)
        {
            var r = new List<PartInfo>();
            if (_parts == null) return r;
            for (int i = 0; i < _parts.Count; i++)
            {
                var p = _parts[i];
                if (p?.GetBuff(_buffId) != null)
                    r.Add(p);
            }
            return r;
        }

        /// <summary> 与 SpreadBurnEffectHandler 一致：有燃烧层数最高者作为源，其余带燃烧的部位为被均衡对象。 </summary>
        private static List<PartInfo> FilterPartsSpreadBurnVictims(List<PartInfo> _parts)
        {
            var r = new List<PartInfo>();
            if (_parts == null || _parts.Count == 0) return r;
            int maxLayer = 0;
            for (int i = 0; i < _parts.Count; i++)
            {
                var b = _parts[i]?.GetBuff(EBuffType.BURN);
                if (b != null)
                    maxLayer = Mathf.Max(maxLayer, b.buffLayer);
            }
            if (maxLayer <= 0) return r;
            PartInfo maxPart = null;
            for (int i = 0; i < _parts.Count; i++)
            {
                var p = _parts[i];
                var b = p?.GetBuff(EBuffType.BURN);
                if (b != null && b.buffLayer == maxLayer)
                {
                    maxPart = p;
                    break;
                }
            }
            if (maxPart == null) return r;
            for (int i = 0; i < _parts.Count; i++)
            {
                var p = _parts[i];
                if (p == null || p == maxPart) continue;
                if (p.GetBuff(EBuffType.BURN) != null)
                    r.Add(p);
            }
            return r;
        }

        private static List<PartInfo> FilterPartsHealWeakestOnly(List<PartInfo> _parts)
        {
            var r = new List<PartInfo>();
            if (_parts == null || _parts.Count == 0) return r;
            PartInfo pick = null;
            float bestRatio = float.MaxValue;
            for (int i = 0; i < _parts.Count; i++)
            {
                var p = _parts[i];
                if (p == null || p.maxHealth <= 0) continue;
                float ratio = (float)p.currentHealth / p.maxHealth;
                if (pick == null || ratio < bestRatio - 1e-5f
                    || (Mathf.Approximately(ratio, bestRatio) && p.currentHealth < pick.currentHealth))
                {
                    bestRatio = ratio;
                    pick = p;
                }
            }
            if (pick != null)
                r.Add(pick);
            return r;
        }

        /// <summary>
        /// 遍历施法部位当前效果格，收集指定脸图上占据格子的部位（去重）。
        /// </summary>
        public static List<PartInfo> CollectPartsInEffectArea(PartInfo _caster, List<FaceGridInfo> _gridInfoList)
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
