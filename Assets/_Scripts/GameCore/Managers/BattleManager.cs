using DG.Tweening;
using GameCore.UI;
using SCFrame;
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
        private List<PartInfo> _playerExecQueue = new List<PartInfo>();
        private List<PartInfo> _enemyExecQueue = new List<PartInfo>();

        private bool _isPlayerExecuting;
        private bool _isEnemyExecuting;

        private System.Action _onPlayerFinish;
        private System.Action _onEnemyFinish;

        // 当前执行到第几个
        private int _playerCurrentIndex = -1;
        private int _enemyCurrentIndex = -1;

        public override void OnInitialize()
        {
            playerExcuteInfoList = new List<PartInfo>();
            enemyExcuteInfoList = new List<PartInfo>();
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
            var queue = isPlayer ? _playerExecQueue : _enemyExecQueue;
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
                _onPlayerFinish = onFinish;
                _isPlayerExecuting = true;
                _playerCurrentIndex = -1;
            }
            else
            {
                _onEnemyFinish = onFinish;
                _isEnemyExecuting = true;
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

            var queue = isPlayer ? _playerExecQueue : _enemyExecQueue;
            index = Mathf.Clamp(index, 0, queue.Count);
            queue.Insert(index, part);

            // 如果没在执行，启动
            if (!(isPlayer ? _isPlayerExecuting : _isEnemyExecuting))
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
            var queue = isPlayer ? _playerExecQueue : _enemyExecQueue;
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
            var queue = isPlayer ? _playerExecQueue : _enemyExecQueue;
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
            var queue = isPlayer ? _playerExecQueue : _enemyExecQueue;
            ref int curIndex = ref (isPlayer ? ref _playerCurrentIndex : ref _enemyCurrentIndex);

            curIndex++;

            // 全部执行完
            if (curIndex >= queue.Count)
            {
                if (isPlayer)
                {
                    _isPlayerExecuting = false;
                    _onPlayerFinish?.Invoke();
                    _onPlayerFinish = null;
                }
                else
                {
                    _isEnemyExecuting = false;
                    _onEnemyFinish?.Invoke();
                    _onEnemyFinish = null;
                }
                return;
            }

            var part = queue[curIndex];
            SCTaskHelper.instance.CreateCoroutine(this,ExecuteOneRoutine(isPlayer, part));
        }

        private IEnumerator ExecuteOneRoutine(bool isPlayer, PartInfo part)
        {
            yield return new WaitForSeconds(0.5f);
            part.TriggerActiveLogic(EAttributeTriggerPointType.ACTIVE);
            ExecuteNext(isPlayer);
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
            GameModel.instance.DealNextTurn();
            UICoreMgr.instance.AddNode(new UINodeMaskCombine(SCFrame.UI.SCUIShowType.FULL));
        }

        //public void InsertExcuteInfo2List(PartInfo _info, int _idx, bool _isPlayer)
        //{
        //    if (_info == null) return;
        //    if (_isPlayer) playerExcuteInfoList.Insert(_idx, _info);
        //    else enemyExcuteInfoList.Insert(_idx, _info);
        //}

        //public void RemoveInfoFromList(PartInfo _info, bool _isPlayer)
        //{
        //    if (_info == null) return;
        //    if (_isPlayer)
        //    {
        //        if (playerExcuteInfoList.Contains(_info))
        //            playerExcuteInfoList.Remove(_info);
        //    }
        //    else
        //    {
        //        if (enemyExcuteInfoList.Contains(_info))
        //            enemyExcuteInfoList.Remove(_info);
        //    }
        //}

        public int GetIndexOfPartInfo(PartInfo _info, bool _isPlayer)
        {
            return _isPlayer ? playerExcuteInfoList.IndexOf(_info) : enemyExcuteInfoList.IndexOf(_info);
        }
    }
}