using SCFrame;
using System.Collections.Generic;
using GameCore.UI;

namespace GameCore.Battle
{
    /// <summary>
    /// 默认战斗上下文实现，桥接 GameModel 与 BattleManager。
    /// </summary>
    public class BattleContext : IBattleContext
    {
        public static IBattleContext Current { get; set; }

        public ETurnOwnerType TurnOwner => GameModel.instance.curTurnOwner;

        public void SetTurnOwner(ETurnOwnerType owner) => GameModel.instance.curTurnOwner = owner;

        public IReadOnlyList<PartInfo> PlayerBattleParts => GameModel.instance.playerInfo.battlePartInfoList;
        public IReadOnlyList<PartInfo> EnemyBattleParts => GameModel.instance.curEnemyInfo?.battlePartInfoList;

        public void ApplyDamageToPlayer(int amount)
        {
            if (amount <= 0) return;
            GameModel.instance.playerInfo.currentHealth = UnityEngine.Mathf.Clamp(
                GameModel.instance.playerInfo.currentHealth - amount, 0, GameModel.instance.playerInfo.maxHealth);
            SCMsgCenter.SendMsg(SCMsgConst.PLAYER_HURT);
            if (GameModel.instance.playerInfo.currentHealth == 0)
                RequestTerminateBattle(false);
        }

        public void ApplyHealToPlayer(int amount)
        {
            if (amount <= 0) return;
            GameModel.instance.playerInfo.currentHealth = UnityEngine.Mathf.Clamp(
                GameModel.instance.playerInfo.currentHealth + amount, 0, GameModel.instance.playerInfo.maxHealth);
            SCMsgCenter.SendMsg(SCMsgConst.PLAYER_HEAL);
        }

        public void ApplyDamageToEnemy(int amount)
        {
            if (amount <= 0) return;
            var enemy = GameModel.instance.curEnemyInfo;
            if (enemy == null) return;
            enemy.currentHealth = UnityEngine.Mathf.Clamp(enemy.currentHealth - amount, 0, enemy.maxHealth);
            SCMsgCenter.SendMsg(SCMsgConst.ENEMY_HURT);
            if (enemy.currentHealth == 0)
                RequestTerminateBattle(true);
        }

        public void ApplyHealToEnemy(int amount)
        {
            if (amount <= 0) return;
            var enemy = GameModel.instance.curEnemyInfo;
            if (enemy == null) return;
            enemy.currentHealth = UnityEngine.Mathf.Clamp(enemy.currentHealth + amount, 0, enemy.maxHealth);
            SCMsgCenter.SendMsg(SCMsgConst.ENEMY_HEAL);
        }

        public void ApplyDamageToPart(PartInfo part, PartInfo sender, int amount)
        {
            if (amount <= 0) return;
            part.currentHealth = UnityEngine.Mathf.Clamp(part.currentHealth - amount, 0, part.maxHealth);
            SCMsgCenter.SendMsg(SCMsgConst.PART_HURT, part, amount);
            part.TriggerGetHitLogic(sender, amount);
            if (part.currentHealth == 0)
            {
                SCMsgCenter.SendMsg(SCMsgConst.PART_DIE, part);
                if (part.isEnemyPart)
                {
                    GameModel.instance.curEnemyInfo.battlePartInfoList.Remove(part);
                    BattleManager.instance.RemovePartFromList(false, part);
                    SCMsgCenter.SendMsg(SCMsgConst.BATTLE_ENEMY_PART_ORDER_CHG);
                }
                else
                {
                    GameModel.instance.playerInfo.battlePartInfoList.Remove(part);
                    BattleManager.instance.RemovePartFromList(true, part);
                    SCMsgCenter.SendMsg(SCMsgConst.BATTLE_PLAYER_PART_ORDER_CHG);
                }
            }
        }

        public void ApplyHealToPart(PartInfo part, int amount)
        {
            if (amount <= 0) return;
            part.currentHealth = UnityEngine.Mathf.Clamp(part.currentHealth + amount, 0, part.maxHealth);
            SCMsgCenter.SendMsg(SCMsgConst.PART_HEAL, part, amount);
        }

        public void RemovePlayerPartFromBattle(PartInfo part)
        {
            if (part == null) return;
            GameModel.instance.playerInfo.battlePartInfoList?.Remove(part);
            BattleManager.instance.RemovePartFromList(true, part);
            SCMsgCenter.SendMsg(SCMsgConst.BATTLE_PLAYER_PART_ORDER_CHG);
        }

        public void RemoveEnemyPartFromBattle(PartInfo part)
        {
            if (part == null) return;
            GameModel.instance.curEnemyInfo?.battlePartInfoList?.Remove(part);
            BattleManager.instance.RemovePartFromList(false, part);
            SCMsgCenter.SendMsg(SCMsgConst.BATTLE_ENEMY_PART_ORDER_CHG);
        }

        public void InsertPartAfterInQueue(bool isPlayer, PartInfo afterPart, PartInfo part)
            => BattleManager.instance.InsertPartAfterTarget(isPlayer, afterPart, part);

        public void InsertPartAtInQueue(bool isPlayer, int index, PartInfo part)
            => BattleManager.instance.InsertPartAt(isPlayer, index, part);

        public int GetPartIndexInQueue(PartInfo part, bool isPlayer)
            => BattleManager.instance.GetIndexOfPartInfo(part, isPlayer);

        public void RequestTerminateBattle(bool isPlayerWin)
            => BattleManager.instance.TerminateBattle(isPlayerWin);
    }
}
