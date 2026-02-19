using GameCore.Battle;
using GameCore.UI;
using SCFrame;
using SCFrame.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public class BattleManager : Singleton<BattleManager>
    {
        public List<PartInfo> playerExcuteInfoList;
        public List<PartInfo> enemyExcuteInfoList;

        private readonly BattlePartExecutionQueue _playerQueue = new BattlePartExecutionQueue();
        private readonly BattlePartExecutionQueue _enemyQueue = new BattlePartExecutionQueue();
        private BattleCancelToken _cancelToken;

        public override void OnInitialize()
        {
            playerExcuteInfoList = new List<PartInfo>();
            enemyExcuteInfoList = new List<PartInfo>();
        }

        public override void OnDiscard()
        {
            _cancelToken?.Cancel();
            BattleContext.Current = null;
        }

        public void StartBattle()
        {
            playerExcuteInfoList = new List<PartInfo>(GameModel.instance.playerInfo.battlePartInfoList);
            enemyExcuteInfoList = new List<PartInfo>(GameModel.instance.curEnemyInfo.battlePartInfoList);
            if (_cancelToken == null) _cancelToken = new BattleCancelToken();
            _cancelToken.Reset();

            SCTimeCaller.instance.CallDealy(0.5f, () =>
            {
                if (_cancelToken.IsCancelled) return;
                BattleContext.Current = new BattleContext();

                if (GameModel.instance.curTurnOwner == ETurnOwnerType.PLAYER)
                {
                    StartExecuteParts(true, playerExcuteInfoList, () =>
                    {
                        ChangeTurnOwner();
                        StartExecuteParts(false, enemyExcuteInfoList, OnBattleRoundFinish);
                    });
                }
                else
                {
                    StartExecuteParts(false, enemyExcuteInfoList, () =>
                    {
                        ChangeTurnOwner();
                        StartExecuteParts(true, playerExcuteInfoList, OnBattleRoundFinish);
                    });
                }
             });
        }

        private void OnBattleRoundFinish()
        {
            SCTimeCaller.instance.CallDealy(1f, () =>
            {
                if (_cancelToken != null && _cancelToken.IsCancelled) return;
                BattleContext.Current = null;
                GameModel.instance.DealNextTurn();
                UICoreMgr.instance.AddNode(new UINodeMaskCombine(SCUIShowType.FULL));
                UICoreMgr.instance.AddNode(new UINodeBattleOrder(SCUIShowType.ADDITION));
            });
        }

        #region ??????????????????????????

        /// <summary> ???????????? </summary>
        public void StartExecuteParts(bool isPlayer, List<PartInfo> parts, Action onFinish = null)
        {
            var list = isPlayer ? playerExcuteInfoList : enemyExcuteInfoList;
            var queue = isPlayer ? _playerQueue : _enemyQueue;

            //list.Clear();
            //if (parts != null)
            //{
            //    foreach (var part in parts)
            //        if (part != null) list.Add(part);
            //}
            queue.Start(list, onFinish);
            ExecuteNext(isPlayer);
        }

        /// <summary> ?????????????index = queue.Count ?????? </summary>
        public void InsertPartAt(bool isPlayer, int index, PartInfo part)
        {
            if (part == null) return;
            var queue = isPlayer ? _playerQueue : _enemyQueue;
            if (index < queue.CurrentIndex) return;

            queue.InsertAt(index, part);
            var list = isPlayer ? playerExcuteInfoList : enemyExcuteInfoList;
            index = Mathf.Clamp(index, 0, list.Count);
            list.Insert(index, part);

            if (!queue.IsExecuting)
                ExecuteNext(isPlayer);
        }

        public void AddPartToLast(bool isPlayer, PartInfo part)
        {
            var list = isPlayer ? playerExcuteInfoList : enemyExcuteInfoList;
            InsertPartAt(isPlayer, list.Count, part);
        }

        public void InsertPartAfterCurrent(bool isPlayer, PartInfo part)
        {
            var queue = isPlayer ? _playerQueue : _enemyQueue;
            InsertPartAt(isPlayer, queue.CurrentIndex + 1, part);
        }

        public void InsertPartAfterTarget(bool isPlayer, PartInfo targetPart, PartInfo newPart)
        {
            var queue = isPlayer ? _playerQueue : _enemyQueue;
            int index = queue.IndexOf(targetPart);
            var list = isPlayer ? playerExcuteInfoList : enemyExcuteInfoList;
            if (index < 0)
            {
                AddPartToLast(isPlayer, newPart);
                return;
            }
            InsertPartAt(isPlayer, index + 1, newPart);
        }

        private void ExecuteNext(bool isPlayer)
        {
            var queue = isPlayer ? _playerQueue : _enemyQueue;
            PartInfo part = queue.MoveNext();
            if (part == null)
                return;
            RunOnePartSequence(isPlayer, part);
        }

        /// <summary>
        /// ????¦Ë????????????????????§¿???????????????????????? _cancelToken??
        /// </summary>
        private void RunOnePartSequence(bool isPlayer, PartInfo part)
        {
            const float delayStart = 0.75f;
            const float delayEffect = 0.75f;
            const float delayEnd = 1f;

            SCTimeCaller.instance.CallDealy(delayStart, () =>
            {
                if (_cancelToken != null && _cancelToken.IsCancelled) return;
                SCMsgCenter.SendMsg(SCMsgConst.PART_ACTIVE_START, part);
                SCTimeCaller.instance.CallDealy(delayEffect, () =>
                {
                    if (_cancelToken != null && _cancelToken.IsCancelled) return;
                    part.TriggerActiveLogic();
                    SCTimeCaller.instance.CallDealy(delayEnd, () =>
                    {
                        if (_cancelToken != null && _cancelToken.IsCancelled) return;
                        SCMsgCenter.SendMsg(SCMsgConst.PART_ACTIVE_END, part);
                        ExecuteNext(isPlayer);
                    });
                });
            });
        }

        /// <summary> ?????????????????????????????????@???????? </summary>
        public bool RemovePartFromList(bool isPlayer, PartInfo part)
        {
            if (part == null) return false;
            var queue = isPlayer ? _playerQueue : _enemyQueue;
            var list = isPlayer ? playerExcuteInfoList : enemyExcuteInfoList;
            bool fromQueue = queue.Remove(part);
            bool fromList = list.Remove(part);
            return fromQueue || fromList;
        }

        /// <summary> ?????????????????????? </summary>
        public bool RemovePartAt(bool isPlayer, int index)
        {
            var list = isPlayer ? playerExcuteInfoList : enemyExcuteInfoList;
            if (index < 0 || index >= list.Count) return false;
            PartInfo part = list[index];
            list.RemoveAt(index);
            var queue = isPlayer ? _playerQueue : _enemyQueue;
            queue.Remove(part);
            return true;
        }

        public int GetIndexOfPartInfo(PartInfo info, bool isPlayer)
        {
            var queue = isPlayer ? _playerQueue : _enemyQueue;
            return queue.IndexOf(info);
        }

        #endregion

        public void ChangeTurnOwner()
        {
            GameModel.instance.curTurnOwner = GameModel.instance.curTurnOwner == ETurnOwnerType.PLAYER
                ? ETurnOwnerType.ENEMY
                : ETurnOwnerType.PLAYER;
        }

        /// <summary> ????????????????????????? </summary>
        public void FinishBattle()
        {
            OnBattleRoundFinish();
        }

        /// <summary> ??????????????????? </summary>
        public void TerminateBattle(bool isPlayerWin)
        {
            _cancelToken?.Cancel();
            BattleContext.Current = null;
            _playerQueue.Start(null, null);
            _enemyQueue.Start(null, null);
            playerExcuteInfoList.Clear();
            enemyExcuteInfoList.Clear();
            if (isPlayerWin)
                UICoreMgr.instance.AddNode(new UINodeBattleWin(SCUIShowType.ADDITION));
            else
                UICoreMgr.instance.AddNode(new UINodeLose(SCUIShowType.FULL));
        }
    }
}
