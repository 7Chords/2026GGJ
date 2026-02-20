using System.Collections.Generic;

namespace GameCore.Battle
{
    /// <summary>
    /// 战斗上下文：抽象当前战斗的数据与操作，便于扩展（如多队伍、观战、回放）和测试。
    /// </summary>
    public interface IBattleContext
    {
        /// <summary> 当前回合归属 </summary>
        ETurnOwnerType turnOwner { get; }

        /// <summary> 设置回合归属（内部用） </summary>
        void SetTurnOwner(ETurnOwnerType _owner);

        /// <summary> 玩家方当前参战部位列表（只读） </summary>
        IReadOnlyList<PartInfo> playerBattleParts { get; }

        /// <summary> 敌方当前参战部位列表（只读） </summary>
        IReadOnlyList<PartInfo> enemyBattleParts { get; }

        /// <summary> 对玩家造成伤害；若玩家死亡会请求结束战斗 </summary>
        void ApplyDamageToPlayer(int _amount);

        /// <summary> 对玩家治疗 </summary>
        void ApplyHealToPlayer(int _amount);

        /// <summary> 对当前敌人造成伤害；若敌人死亡会请求结束战斗 </summary>
        void ApplyDamageToEnemy(int _amount);

        /// <summary> 对当前敌人治疗 </summary>
        void ApplyHealToEnemy(int _amount);

        /// <summary> 对部位造成伤害；若部位死亡会从参战列表移除并通知 </summary>
        void ApplyDamageToPart(PartInfo _part, PartInfo _sender, int _amount);

        /// <summary> 对部位治疗 </summary>
        void ApplyHealToPart(PartInfo _part, int _amount);

        /// <summary> 从玩家战斗队列中移除部位（如死亡、跳过回合等） </summary>
        void RemovePlayerPartFromBattle(PartInfo _part);

        /// <summary> 从敌人战斗队列中移除部位 </summary>
        void RemoveEnemyPartFromBattle(PartInfo _part);

        /// <summary> 请求在指定部位后插入执行（再执行一次等） </summary>
        void InsertPartAfterInQueue(bool _isPlayer, PartInfo _afterPart, PartInfo _part);

        /// <summary> 在队列指定索引插入执行 </summary>
        void InsertPartAtInQueue(bool _isPlayer, int _index, PartInfo _part);

        /// <summary> 获取当前执行队列中某部位的索引 </summary>
        int GetPartIndexInQueue(PartInfo _part, bool _isPlayer);

        /// <summary> 请求结束战斗（胜负已分） </summary>
        void RequestTerminateBattle(bool _isPlayerWin);
    }
}
