using System.Collections.Generic;

namespace GameCore.Battle
{
    /// <summary>
    /// 战斗上下文：抽象当前战斗的数据与操作，便于扩展（如多队伍、观战、回放）和测试。
    /// </summary>
    public interface IBattleContext
    {
        /// <summary> 当前回合归属 </summary>
        ETurnOwnerType TurnOwner { get; }

        /// <summary> 设置回合归属（内部用） </summary>
        void SetTurnOwner(ETurnOwnerType owner);

        /// <summary> 玩家方当前参战部位列表（只读） </summary>
        IReadOnlyList<PartInfo> PlayerBattleParts { get; }

        /// <summary> 敌方当前参战部位列表（只读） </summary>
        IReadOnlyList<PartInfo> EnemyBattleParts { get; }

        /// <summary> 对玩家造成伤害；若玩家死亡会请求结束战斗 </summary>
        void ApplyDamageToPlayer(int amount);

        /// <summary> 对玩家治疗 </summary>
        void ApplyHealToPlayer(int amount);

        /// <summary> 对当前敌人造成伤害；若敌人死亡会请求结束战斗 </summary>
        void ApplyDamageToEnemy(int amount);

        /// <summary> 对当前敌人治疗 </summary>
        void ApplyHealToEnemy(int amount);

        /// <summary> 对部位造成伤害；若部位死亡会从参战列表移除并通知 </summary>
        void ApplyDamageToPart(PartInfo part, PartInfo sender, int amount);

        /// <summary> 对部位治疗 </summary>
        void ApplyHealToPart(PartInfo part, int amount);

        /// <summary> 从玩家战斗队列中移除部位（如死亡、跳过回合等） </summary>
        void RemovePlayerPartFromBattle(PartInfo part);

        /// <summary> 从敌人战斗队列中移除部位 </summary>
        void RemoveEnemyPartFromBattle(PartInfo part);

        /// <summary> 请求在指定部位后插入执行（再执行一次等） </summary>
        void InsertPartAfterInQueue(bool isPlayer, PartInfo afterPart, PartInfo part);

        /// <summary> 在队列指定索引插入执行 </summary>
        void InsertPartAtInQueue(bool isPlayer, int index, PartInfo part);

        /// <summary> 获取当前执行队列中某部位的索引 </summary>
        int GetPartIndexInQueue(PartInfo part, bool isPlayer);

        /// <summary> 请求结束战斗（胜负已分） </summary>
        void RequestTerminateBattle(bool isPlayerWin);
    }
}
