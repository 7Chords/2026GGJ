using DG.Tweening;
using GameCore.UI;
using SCFrame;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public class BattleManager : Singleton<BattleManager>
    {
        public List<PartInfo> playerExcuteInfoList;
        public List<PartInfo> enemyExcuteInfoList;

        private TweenContainer _m_tweenContainer;

        // 核心：真正支持任意插入的执行队列
        private List<PartInfo> _m_playerExecQueue;
        private List<PartInfo> _m_enemyExecQueue;

        private bool _m_isPlayerExecuting;
        private bool _m_isEnemyExecuting;

        private Action _m_onPlayerFinish;
        private Action _m_onEnemyFinish;

        // 当前执行到第几个
        private int _playerCurrentIndex = -1;
        private int _enemyCurrentIndex = -1;

        public override void OnInitialize()
        {
            playerExcuteInfoList = new List<PartInfo>();
            enemyExcuteInfoList = new List<PartInfo>();
            _m_playerExecQueue = new List<PartInfo>();
            _m_enemyExecQueue = new List<PartInfo>();
            _m_tweenContainer = new TweenContainer();
        }
        public override void OnDiscard()
        {
            SCTaskHelper.instance.KillAllCoroutines(this);
        }

        public void StartBattle()
        {
            playerExcuteInfoList = new List<PartInfo>(GameModel.instance.playerInfo.battlePartInfoList);
            enemyExcuteInfoList = new List<PartInfo>(GameModel.instance.curEnemyInfo.battlePartInfoList);

            SCTimeCaller.instance.CallDealy(0.5f, () =>
            {
                GameModel.instance.curTurnOwner = ETurnOwnerType.PLAYER;

                // 玩家 → 换回合 → 敌人 → 结束
                StartExecuteParts(true, playerExcuteInfoList, () =>
                {
                    ChangeTurnOwner();
                    StartExecuteParts(false, enemyExcuteInfoList, FinishBattle);
                });
            });
        }

        #region 核心：任意位置插入执行队列
        /// <summary>
        /// 开始执行一组部件
        /// </summary>
        public void StartExecuteParts(bool isPlayer, List<PartInfo> parts, System.Action onFinish = null)
        {
            var queue = isPlayer ? _m_playerExecQueue : _m_enemyExecQueue;
            queue.Clear();

            if (parts != null)
            {
                foreach (var part in parts)
                {
                    if (part != null)
                        queue.Add(part);
                }
            }

            if (isPlayer)
            {
                _m_onPlayerFinish = onFinish;
                _m_isPlayerExecuting = true;
                _playerCurrentIndex = -1;
            }
            else
            {
                _m_onEnemyFinish = onFinish;
                _m_isEnemyExecuting = true;
                _enemyCurrentIndex = -1;
            }

            ExecuteNext(isPlayer);
        }

        /// <summary>
        /// 【全能接口】插入到任意位置
        /// index = 0 → 最前面
        /// index = 2 → 插到第2个后面
        /// index = queue.Count → 最后面（等于Add）
        /// </summary>
        public void InsertPartAt(bool isPlayer, int index, PartInfo part)
        {
            if (part == null) return;

            var queue = isPlayer ? _m_playerExecQueue : _m_enemyExecQueue;
            index = Mathf.Clamp(index, 0, queue.Count);
            queue.Insert(index, part);

            // 如果没在执行，启动
            if (!(isPlayer ? _m_isPlayerExecuting : _m_isEnemyExecuting))
            {
                if (isPlayer) _playerCurrentIndex = -1;
                else _enemyCurrentIndex = -1;
                ExecuteNext(isPlayer);
            }
        }

        /// <summary>
        /// 插到最后（普通追加）
        /// </summary>
        public void AddPartToLast(bool isPlayer, PartInfo part)
        {
            var queue = isPlayer ? _m_playerExecQueue : _m_enemyExecQueue;
            InsertPartAt(isPlayer, queue.Count, part);
        }

        /// <summary>
        /// 插到当前正在执行的后面（实现“再执行一遍”）
        /// </summary>
        public void InsertPartAfterCurrent(bool isPlayer, PartInfo part)
        {
            int curIndex = isPlayer ? _playerCurrentIndex : _enemyCurrentIndex;
            InsertPartAt(isPlayer, curIndex + 1, part);
        }

        /// <summary>
        /// 插到某个目标部件的后面
        /// </summary>
        public void InsertPartAfterTarget(bool isPlayer, PartInfo targetPart, PartInfo newPart)
        {
            var queue = isPlayer ? _m_playerExecQueue : _m_enemyExecQueue;
            int index = queue.IndexOf(targetPart);
            if (index < 0)
            {
                AddPartToLast(isPlayer, newPart);
                return;
            }
            InsertPartAt(isPlayer, index + 1, newPart);
        }

        private void ExecuteNext(bool isPlayer)
        {
            var queue = isPlayer ? _m_playerExecQueue : _m_enemyExecQueue;
            ref int curIndex = ref (isPlayer ? ref _playerCurrentIndex : ref _enemyCurrentIndex);

            curIndex++;

            // 全部执行完
            if (curIndex >= queue.Count)
            {
                if (isPlayer)
                {
                    _m_isPlayerExecuting = false;
                    _m_onPlayerFinish?.Invoke();
                    _m_onPlayerFinish = null;
                }
                else
                {
                    _m_isEnemyExecuting = false;
                    _m_onEnemyFinish?.Invoke();
                    _m_onEnemyFinish = null;
                }
                return;
            }

            PartInfo part = queue[curIndex];
            SCTaskHelper.instance.CreateCoroutine(this,ExecuteOneRoutine(isPlayer, part));
        }

        private IEnumerator ExecuteOneRoutine(bool isPlayer, PartInfo part)
        {
            yield return new WaitForSeconds(1f);
            SCMsgCenter.SendMsg(SCMsgConst.PART_ACTIVE, part);
            part.TriggerActiveLogic(EAttributeTriggerPointType.ACTIVE);
            ExecuteNext(isPlayer);
        }

        /// <summary>
        /// 从队列中删除指定的部位（按引用删除）
        /// </summary>
        public bool RemovePartFromList(bool isPlayer, PartInfo part)
        {
            if (part == null) return false;

            var queue = isPlayer ? playerExcuteInfoList : enemyExcuteInfoList;
            bool removed = queue.Remove(part);

            // 如果删除的是还没执行到的，队列索引不用动，自动跳过
            return removed;
        }

        /// <summary>
        /// 根据索引删除队列中的部位
        /// </summary>
        public bool RemovePartAt(bool isPlayer, int index)
        {
            var queue = isPlayer ? playerExcuteInfoList : enemyExcuteInfoList;
            if (index < 0 || index >= queue.Count)
                return false;

            queue.RemoveAt(index);
            return true;
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
            SCTimeCaller.instance.CallDealy(1f, () =>
            {
                GameModel.instance.DealNextTurn();
                UICoreMgr.instance.AddNode(new UINodeMaskCombine(SCFrame.UI.SCUIShowType.FULL));
            });
        }
        public int GetIndexOfPartInfo(PartInfo _info, bool _isPlayer)
        {
            return _isPlayer ? playerExcuteInfoList.IndexOf(_info) : enemyExcuteInfoList.IndexOf(_info);
        }
    }
}