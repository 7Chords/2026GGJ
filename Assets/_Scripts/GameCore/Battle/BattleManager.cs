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
            BattleContext.current = null;
        }

        public void StartBattle()
        {
            playerExcuteInfoList = new List<PartInfo>(GameModel.instance.playerInfo.battlePartInfoList);
            enemyExcuteInfoList = new List<PartInfo>(GameModel.instance.curEnemyInfo.battlePartInfoList);
            if (_cancelToken == null) _cancelToken = new BattleCancelToken();
            _cancelToken.Reset();

            SCTimeCaller.instance.CallDealy(0.5f, () =>
            {
                if (_cancelToken.isCancelled) return;
                BattleContext.current = new BattleContext();

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
                if (_cancelToken != null && _cancelToken.isCancelled) return;
                BattleContext.current = null;
                GameModel.instance.DealNextTurn();
                UICoreMgr.instance.AddNode(new UINodeMaskCombine(SCUIShowType.FULL));
                UICoreMgr.instance.AddNode(new UINodeBattleOrder(SCUIShowType.ADDITION));
            });
        }

        #region 队列相关方法

        public void StartExecuteParts(bool _isPlayer, List<PartInfo> _parts, Action _onFinish = null)
        {
            var list = _isPlayer ? playerExcuteInfoList : enemyExcuteInfoList;
            var queue = _isPlayer ? _playerQueue : _enemyQueue;

            //list.Clear();
            //if (_parts != null)
            //{
            //    foreach (var part in _parts)
            //        if (part != null) list.Add(part);
            //}
            queue.Start(list, _onFinish);
            ExecuteNext(_isPlayer);
        }

        public void InsertPartAt(bool _isPlayer, int _index, PartInfo _part)
        {
            if (_part == null) return;
            var queue = _isPlayer ? _playerQueue : _enemyQueue;
            if (_index < queue.currentIndex) return;

            queue.InsertAt(_index, _part);
            var list = _isPlayer ? playerExcuteInfoList : enemyExcuteInfoList;
            int index = Mathf.Clamp(_index, 0, list.Count);
            list.Insert(index, _part);

            if (!queue.isExecuting)
                ExecuteNext(_isPlayer);
        }

        public void AddPartToLast(bool _isPlayer, PartInfo _part)
        {
            var list = _isPlayer ? playerExcuteInfoList : enemyExcuteInfoList;
            InsertPartAt(_isPlayer, list.Count, _part);
        }

        public void InsertPartAfterCurrent(bool _isPlayer, PartInfo _part)
        {
            var queue = _isPlayer ? _playerQueue : _enemyQueue;
            InsertPartAt(_isPlayer, queue.currentIndex + 1, _part);
        }

        public void InsertPartAfterTarget(bool _isPlayer, PartInfo _targetPart, PartInfo _newPart)
        {
            var queue = _isPlayer ? _playerQueue : _enemyQueue;
            int index = queue.IndexOf(_targetPart);
            var list = _isPlayer ? playerExcuteInfoList : enemyExcuteInfoList;
            if (index < 0)
            {
                AddPartToLast(_isPlayer, _newPart);
                return;
            }
            InsertPartAt(_isPlayer, index + 1, _newPart);
        }

        private void ExecuteNext(bool _isPlayer)
        {
            var queue = _isPlayer ? _playerQueue : _enemyQueue;
            PartInfo part = queue.MoveNext();
            if (part == null)
                return;
            RunOnePartSequence(_isPlayer, part);
        }

        private void RunOnePartSequence(bool _isPlayer, PartInfo _part)
        {
            const float delayStart = 0.75f;
            const float delayEffect = 0.75f;
            const float delayEnd = 1f;

            SCTimeCaller.instance.CallDealy(delayStart, () =>
            {
                if (_cancelToken != null && _cancelToken.isCancelled) return;
                SCMsgCenter.SendMsg(SCMsgConst.PART_ACTIVE_START, _part);
                SCTimeCaller.instance.CallDealy(delayEffect, () =>
                {
                    if (_cancelToken != null && _cancelToken.isCancelled) return;
                    _part.TriggerActiveLogic();
                    SCTimeCaller.instance.CallDealy(delayEnd, () =>
                    {
                        if (_cancelToken != null && _cancelToken.isCancelled) return;
                        SCMsgCenter.SendMsg(SCMsgConst.PART_ACTIVE_END, _part);
                        ExecuteNext(_isPlayer);
                    });
                });
            });
        }

        public bool RemovePartFromList(bool _isPlayer, PartInfo _part)
        {
            if (_part == null) return false;
            var queue = _isPlayer ? _playerQueue : _enemyQueue;
            var list = _isPlayer ? playerExcuteInfoList : enemyExcuteInfoList;
            bool fromQueue = queue.Remove(_part);
            bool fromList = list.Remove(_part);
            return fromQueue || fromList;
        }
        public bool RemovePartAt(bool _isPlayer, int _index)
        {
            var list = _isPlayer ? playerExcuteInfoList : enemyExcuteInfoList;
            if (_index < 0 || _index >= list.Count) return false;
            PartInfo part = list[_index];
            list.RemoveAt(_index);
            var queue = _isPlayer ? _playerQueue : _enemyQueue;
            queue.Remove(part);
            return true;
        }

        public int GetIndexOfPartInfo(PartInfo _info, bool _isPlayer)
        {
            var queue = _isPlayer ? _playerQueue : _enemyQueue;
            return queue.IndexOf(_info);
        }

        #endregion

        public void ChangeTurnOwner()
        {
            GameModel.instance.curTurnOwner = GameModel.instance.curTurnOwner == ETurnOwnerType.PLAYER
                ? ETurnOwnerType.ENEMY
                : ETurnOwnerType.PLAYER;
        }

        public void FinishBattle()
        {
            OnBattleRoundFinish();
        }
        public void TerminateBattle(bool _isPlayerWin)
        {
            _cancelToken?.Cancel();
            BattleContext.current = null;
            _playerQueue.Start(null, null);
            _enemyQueue.Start(null, null);
            playerExcuteInfoList.Clear();
            enemyExcuteInfoList.Clear();
            if (_isPlayerWin)
            {
                UICoreMgr.instance.AddNode(new UINodeBattleWin(SCUIShowType.ADDITION));
                GameModel.instance.SetAllPlayerPart2Bag();
                GameModel.instance.SetEnemyEmpty();
            }
            else
            {
                UICoreMgr.instance.AddNode(new UINodeLose(SCUIShowType.FULL));
                GameModel.instance.SetAllPlayerPart2Bag();
                GameModel.instance.SetEnemyEmpty();
            }
        }
    }
}
