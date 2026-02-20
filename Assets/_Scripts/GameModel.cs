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
            for (int i = 0; i < playerRefObj.initPartList.Count; i++)
            {
                partEffectObj = playerRefObj.initPartList[i];
                if (partEffectObj == null)
                    continue;
                for(int j =0;j< partEffectObj.partAmount;j++)
                {
                    partRefObj = SCRefDataMgr.instance.partRefList.refDataList.Find(x => x.id == partEffectObj.partId);
                    if (partRefObj == null)
                        continue;
                    info = new PartInfo(partRefObj,false);
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

        public void RollBattleOrder()
        {
            float randomNum = RandomUtility.GetRandomGenerator(EModuleType.COMBAT).Next(1, 100) / 100f;
            curTurnOwner = randomNum < 0.5f ? ETurnOwnerType.PLAYER : ETurnOwnerType.ENEMY;
        }

        public void GenerateNewBattle()
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

    }

}
