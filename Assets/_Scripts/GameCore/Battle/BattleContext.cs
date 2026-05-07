using SCFrame;
using System.Collections.Generic;
using GameCore.UI;
using GameCore.RefData;
using UnityEngine;

namespace GameCore.Battle
{
    /// <summary>
    /// 默认战斗上下文实现，桥接 GameModel 与 BattleManager。
    /// </summary>
    public class BattleContext : IBattleContext
    {
        public static IBattleContext current { get; set; }

        public ETurnOwnerType turnOwner => GameModel.instance.curTurnOwner;

        public void SetTurnOwner(ETurnOwnerType _owner) => GameModel.instance.curTurnOwner = _owner;

        public IReadOnlyList<PartInfo> playerBattleParts => GameModel.instance.playerInfo.battlePartInfoList;
        public IReadOnlyList<PartInfo> enemyBattleParts => GameModel.instance.curEnemyInfo?.battlePartInfoList;

        public void ApplyDamageToPlayer(int _amount)
        {
            if (_amount <= 0) return;
            GameModel.instance.playerInfo.currentHealth = UnityEngine.Mathf.Clamp(
                GameModel.instance.playerInfo.currentHealth - _amount, 0, GameModel.instance.playerInfo.maxHealth);
            SCMsgCenter.SendMsg(SCMsgConst.PLAYER_HURT, _amount);
            if (GameModel.instance.playerInfo.currentHealth == 0)
                RequestTerminateBattle(false);
        }

        public void ApplyHealToPlayer(int _amount)
        {
            if (_amount <= 0) return;
            GameModel.instance.playerInfo.currentHealth = UnityEngine.Mathf.Clamp(
                GameModel.instance.playerInfo.currentHealth + _amount, 0, GameModel.instance.playerInfo.maxHealth);
            SCMsgCenter.SendMsg(SCMsgConst.PLAYER_HEAL);
        }

        public void ApplyDamageToEnemy(int _amount)
        {
            if (_amount <= 0) return;
            var enemy = GameModel.instance.curEnemyInfo;
            if (enemy == null) return;
            enemy.currentHealth = UnityEngine.Mathf.Clamp(enemy.currentHealth - _amount, 0, enemy.maxHealth);
            SCMsgCenter.SendMsg(SCMsgConst.ENEMY_HURT, _amount);
            EnemyPassiveController.OnEnemyBodyDamageApplied(_amount);
            if (enemy.currentHealth == 0)
            {
                AudioMgr.instance.PlaySfx("sfx_enemy_dead");
                RequestTerminateBattle(true);
            }
        }

        public void ApplyHealToEnemy(int _amount)
        {
            if (_amount <= 0) return;
            var enemy = GameModel.instance.curEnemyInfo;
            if (enemy == null) return;
            enemy.currentHealth = UnityEngine.Mathf.Clamp(enemy.currentHealth + _amount, 0, enemy.maxHealth);
            SCMsgCenter.SendMsg(SCMsgConst.ENEMY_HEAL);
        }

        public void ApplyDamageToPart(PartInfo _part, PartInfo _sender, int _amount)
        {
            _amount = EnemyPassiveController.AdjustEnemyOutgoingDamageToPlayerPart(_part, _sender, _amount);
            _amount += BuffCombatModifiers.GetPreyExtraDamage(_part);
            bool isCultureMedium = _part?.partRefObj != null && _part.partRefObj.id == 101031;
            if (GermMassDamageAbsorption.PartHasSendMoldGetHitEntry(_part))
            {
                int before = _amount;
                GermMassDamageAbsorption.AbsorbDamageThroughGerms(_part, ref _amount);

                // 培养基规则：伤害先消耗菌团；若菌团不够吸收（即“溢出”），剩余伤害直接溢出到本体，
                // 而不是扣培养基本身的血量（培养基血量用于展示/承载，不作为护盾生命值）。
                if (isCultureMedium)
                {
                    int absorbed = Mathf.Max(0, before - _amount);
                    int overflowToBodyFromGerms = Mathf.Max(0, _amount);
                    if (absorbed > 0)
                    {
                        SCMsgCenter.SendMsg(SCMsgConst.PART_HURT, _part, absorbed);
                        _part.TriggerGetHitLogic(_sender, absorbed);
                    }
                    if (overflowToBodyFromGerms > 0)
                    {
                        if (_part.isEnemyPart)
                            ApplyDamageToEnemy(overflowToBodyFromGerms);
                        else
                            ApplyDamageToPlayer(overflowToBodyFromGerms);
                    }
                    return;
                }
            }
            if (_amount <= 0) return;
            int hpBefore = _part.currentHealth;
            int damageToPart = UnityEngine.Mathf.Min(_amount, hpBefore);
            int overflowToBody = _amount - damageToPart;
            _part.currentHealth = UnityEngine.Mathf.Clamp(_part.currentHealth - _amount, 0, _part.maxHealth);
            SCMsgCenter.SendMsg(SCMsgConst.PART_HURT, _part, damageToPart);
            _part.TriggerGetHitLogic(_sender, damageToPart);
            if (_part.currentHealth == 0)
            {
                SCMsgCenter.SendMsg(SCMsgConst.PART_DIE, _part);
                if (_part.isEnemyPart)
                {
                    GameModel.instance.curEnemyInfo?.battlePartInfoList.Remove(_part);
                    BattleManager.instance.RemovePartFromList(false, _part);
                    SCMsgCenter.SendMsg(SCMsgConst.BATTLE_ENEMY_PART_ORDER_CHG);
                }
                else
                {
                    GameModel.instance.playerInfo?.battlePartInfoList.Remove(_part);
                    BattleManager.instance.RemovePartFromList(true, _part);
                    SCMsgCenter.SendMsg(SCMsgConst.BATTLE_PLAYER_PART_ORDER_CHG);
                }
            }
            if (overflowToBody > 0)
            {
                if (_part.isEnemyPart)
                    ApplyDamageToEnemy(overflowToBody);
                else
                    ApplyDamageToPlayer(overflowToBody);
            }
        }

        public void ForceKillPart(PartInfo _part)
        {
            if (_part == null) return;
            if (_part.currentHealth <= 0) return;

            int hpBefore = _part.currentHealth;
            _part.currentHealth = 0;

            // Treat as “rule death”: show part hurt once, but do not overflow to body and do not run GET_HIT hooks.
            SCMsgCenter.SendMsg(SCMsgConst.PART_HURT, _part, hpBefore);
            SCMsgCenter.SendMsg(SCMsgConst.PART_DIE, _part);

            if (_part.isEnemyPart)
            {
                GameModel.instance.curEnemyInfo?.battlePartInfoList.Remove(_part);
                BattleManager.instance.RemovePartFromList(false, _part);
                SCMsgCenter.SendMsg(SCMsgConst.BATTLE_ENEMY_PART_ORDER_CHG);
            }
            else
            {
                GameModel.instance.playerInfo?.battlePartInfoList.Remove(_part);
                BattleManager.instance.RemovePartFromList(true, _part);
                SCMsgCenter.SendMsg(SCMsgConst.BATTLE_PLAYER_PART_ORDER_CHG);
            }
        }

        public void ApplyHealToPart(PartInfo _part, int _amount)
        {
            if (_amount <= 0) return;
            _part.currentHealth = UnityEngine.Mathf.Clamp(_part.currentHealth + _amount, 0, _part.maxHealth);
            SCMsgCenter.SendMsg(SCMsgConst.PART_HEAL, _part, _amount);
        }

        public void RemovePlayerPartFromBattle(PartInfo _part)
        {
            if (_part == null) return;
            BattleManager.instance.RemovePartFromList(true, _part);
        }

        public void RemoveEnemyPartFromBattle(PartInfo _part)
        {
            if (_part == null) return;
            BattleManager.instance.RemovePartFromList(false, _part);
        }

        public void InsertPartAfterInQueue(bool _isPlayer, PartInfo _afterPart, PartInfo _part)
            => BattleManager.instance.InsertPartAfterTarget(_isPlayer, _afterPart, _part);

        public void InsertPartAtInQueue(bool _isPlayer, int _index, PartInfo _part)
            => BattleManager.instance.InsertPartAt(_isPlayer, _index, _part);

        public int GetPartIndexInQueue(PartInfo _part, bool _isPlayer)
            => BattleManager.instance.GetIndexOfPartInfo(_part, _isPlayer);

        public void RequestTerminateBattle(bool _isPlayerWin)
            => BattleManager.instance.TerminateBattle(_isPlayerWin);

        public void ApplyBuffToPart(PartInfo _part, PartInfo _sender, long _buffId, int _buffLayer)
        {
            BuffRefObj buffRefObj = SCRefDataMgr.instance.buffRefList.refDataList.Find(x => x.id == _buffId);
            if (buffRefObj == null)
                return;
            BuffInfo buffInfo = BuffFactory.CreateBuffInfo(buffRefObj, _buffLayer, _sender,_part);
            _part.AddBuff(buffInfo);
        }

        public void ApplyBuffMultiplierToPart(PartInfo _part, PartInfo _sender, long _buffId, int _multiplier)
        {
            BuffRefObj buffRefObj = SCRefDataMgr.instance.buffRefList.refDataList.Find(x => x.id == _buffId);
            if (buffRefObj == null)
                return;
            BuffInfo findBuffInfo = _part.GetBuff(_buffId);
            if (findBuffInfo == null)
                return;
            int buffLayer = findBuffInfo.buffLayer * (_multiplier - 1);
            BuffInfo buffInfo = BuffFactory.CreateBuffInfo(buffRefObj, buffLayer, _sender, _part);
            _part.AddBuff(buffInfo);
        }

        public void ApplyReduceBuffLayerToPart(PartInfo _part, long _buffId, int _reduceLayer)
        {
            BuffRefObj buffRefObj = SCRefDataMgr.instance.buffRefList.refDataList.Find(x => x.id == _buffId);
            if (buffRefObj == null)
                return;
            _part.ReduceBuffLayer(_buffId, _reduceLayer);
        }

        public void ApplyReduceAllBuffLayerToPart(PartInfo _part, int _reduceLayer)
        {
            _part.ReduceAllBuffLayer(_reduceLayer);
        }
    }
}
