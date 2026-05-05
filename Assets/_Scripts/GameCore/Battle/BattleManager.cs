using DG.Tweening;
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
        private SequenceRunner _m_runner;
        public override void OnInitialize()
        {
            playerExcuteInfoList = new List<PartInfo>();
            enemyExcuteInfoList = new List<PartInfo>();
            _m_runner = new SequenceRunner();
        }

        public override void OnDiscard()
        {
            _cancelToken?.Cancel();
            BattleContext.current = null;
            _m_runner?.Kill();
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
                        TriggerTurnOverForSide(true);
                        ChangeTurnOwner();
                        StartExecuteParts(false, enemyExcuteInfoList,
                            ()=> 
                            {
                                TriggerTurnOverForSide(false);
                                OnBattleRoundFinish();
                            });
                    });
                }
                else
                {
                    StartExecuteParts(false, enemyExcuteInfoList, () =>
                    {
                        TriggerTurnOverForSide(false);
                        ChangeTurnOwner();
                        StartExecuteParts(true, playerExcuteInfoList, ()=> 
                        {
                            TriggerTurnOverForSide(true);
                            OnBattleRoundFinish();
                        });
                    });
                }
             });
        }

        private void TriggerTotalTurnOverForAllBattleParts()
        {
            TriggerTotalTurnOverOnList(playerExcuteInfoList);
            TriggerTotalTurnOverOnList(enemyExcuteInfoList);
        }

        static void TriggerTotalTurnOverOnList(List<PartInfo> list)
        {
            if (list == null) return;
            for (int i = 0; i < list.Count; i++)
            {
                var part = list[i];
                if (part == null) continue;
                if (!part.HasBuff(EAttributeTriggerPointType.TOTAL_TURN_OVER)) continue;
                part.TriggerBuff(EAttributeTriggerPointType.TOTAL_TURN_OVER);
            }
        }

        private void TriggerTurnOverForSide(bool _isPlayer)
        {
            // 回合结束触发：玩家回合结束触发玩家部位，敌人回合结束触发敌人部位
            var list = _isPlayer ? playerExcuteInfoList : enemyExcuteInfoList;
            if (list == null) return;
            for (int i = 0; i < list.Count; i++)
            {
                var part = list[i];
                if (part == null) continue;
                if (!part.HasBuff(EAttributeTriggerPointType.TURN_OVER)) continue;
                part.TriggerBuff(EAttributeTriggerPointType.TURN_OVER);
            }
        }

        private void OnBattleRoundFinish()
        {
            TriggerTotalTurnOverForAllBattleParts();
            GameModel.instance.ClearBuffsAfterFullBattleRound();
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
            if (!_isPlayer)
                EnemyPassiveController.OnEnemyPhaseStart();
            var list = _isPlayer ? playerExcuteInfoList : enemyExcuteInfoList;
            var queue = _isPlayer ? _playerQueue : _enemyQueue;

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

            SCMsgCenter.SendMsg(SCMsgConst.PART_POSITIVE_BUFF_GAIN, _part);

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
 
            _m_runner?.Kill();
            _m_runner = new SequenceRunner();
            _m_runner.AddTask(GameConst.DELAY_START_TIME, () =>
            {
                if (_cancelToken != null && _cancelToken.isCancelled) return;
                SCMsgCenter.SendMsg(SCMsgConst.PART_ACTIVE_START, _part);
            });
            if (_part.HasBuff(EAttributeTriggerPointType.ACTIVE))
            {
                _m_runner.AddTask(GameConst.DELAY_ACTIVE_BUFF_TIME, () =>
                {
                    if (_cancelToken != null && _cancelToken.isCancelled) return;
                    _part.TriggerBuff(EAttributeTriggerPointType.ACTIVE);
                });
            }
            _m_runner.AddTask(GameConst.DELAY_EFFECT_TIME, () =>
            {
                if (_cancelToken != null && _cancelToken.isCancelled) return;
                MouthAttackCoordinator.ResetPendingForNewActivation();
                MouthAttackCoordinator.BindResume(() => SchedulePartActivationEnd(_isPlayer, _part));
                _part.TriggerActiveLogic();
                if (!MouthAttackCoordinator.PendingMouthAttack)
                {
                    MouthAttackCoordinator.CancelResume();
                    SchedulePartActivationEnd(_isPlayer, _part);
                }
            });
        }

        private void SchedulePartActivationEnd(bool _isPlayer, PartInfo _part)
        {
            SCTimeCaller.instance.CallDealy(GameConst.DELAY_END_TIME, () =>
            {
                if (_cancelToken != null && _cancelToken.isCancelled) return;
                if (_part != null && _part.currentHealth > 0)
                {
                    _part.partLogic?.OnPartActionOver();
                    if (_part.HasBuff(EAttributeTriggerPointType.ACTION_OVER))
                        _part.TriggerBuff(EAttributeTriggerPointType.ACTION_OVER);
                }
                SCMsgCenter.SendMsg(SCMsgConst.PART_ACTIVE_END, _part);
                ExecuteNext(_isPlayer);
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

        public void TerminateBattle(bool _isPlayerWin)
        {
            _cancelToken?.Cancel();
            BattleContext.current = null;

            void FinishTerminateBattleUiAndState()
            {
                if (_isPlayerWin)
                {
                    GameModel.instance.CaptureEnemyWinSnapshot();
                    AudioMgr.instance.PlaySfx("sfx_money");
                    UICoreMgr.instance.AddNode(new UINodeBattleWin(SCUIShowType.ADDITION));
                    GameModel.instance.SetAllPlayerPart2Bag();
                    GameModel.instance.SetEnemyEmpty();
                }
                else
                {
                    GameModel.instance.ClearEnemyWinSnapshot();
                    GameModel.instance.playerInfo.ClearPendingMapMove();
                    UICoreMgr.instance.AddNode(new UINodeLose(SCUIShowType.FULL));
                    GameModel.instance.SetAllPlayerPart2Bag();
                    GameModel.instance.SetEnemyEmpty();
                }

                _playerQueue.Start(null, null);
                _enemyQueue.Start(null, null);
                playerExcuteInfoList.Clear();
                enemyExcuteInfoList.Clear();
            }

            if (UIPanelBattle.Current != null &&
                UIPanelBattle.Current.TryRunDefeatFaceEffectThen(_isPlayerWin, FinishTerminateBattleUiAndState))
                return;

            FinishTerminateBattleUiAndState();
        }
    }
}
