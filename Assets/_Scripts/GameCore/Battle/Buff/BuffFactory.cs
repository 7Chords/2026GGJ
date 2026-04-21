using GameCore.RefData;
using SCFrame;

namespace GameCore.Battle
{
    public static class BuffFactory
    {
        public static BuffInfo CreateBuffInfo(BuffRefObj _buffRefObj, int _layer, PartInfo _creator, PartInfo _owner)
        {
            BuffInfo buffInfo = new BuffInfo(_buffRefObj, _layer, _creator, _owner);
            ProcessBuffInfo(buffInfo);
            return buffInfo;
        }

        public static BuffRefObj GetBuffRefObjByType(EBuffType _buffType)
        {
            // 约定：同一 EBuffType 在表里只有一条
            return SCRefDataMgr.instance.buffRefList.refDataList.Find(x => x.buffType == _buffType);
        }

        public static BuffInfo CreateBuffInfoByType(EBuffType _buffType, int _layer, PartInfo _creator, PartInfo _owner)
        {
            var refObj = GetBuffRefObjByType(_buffType);
            if (refObj == null) return null;
            return CreateBuffInfo(refObj, _layer, _creator, _owner);
        }

        public static BuffInfo ProcessBuffInfo(BuffInfo _buffInfo)
        {

            switch (_buffInfo.buffType)
            {
                case EBuffType.BLEED:
                    {
                        _buffInfo.onPartActive += () =>
                        {
                            int buffValue = (int)_buffInfo.buffValue;
                            var battleCtx = BattleContext.current;
                            battleCtx.ApplyDamageToPart(_buffInfo.owner, _buffInfo.owner, buffValue);
                        };
                    }
                    break;
                case EBuffType.FAT:
                    {
                        // 油脂：本身无效果（仅作为与燃烧交互的资源）
                    }
                    break;
                case EBuffType.BURN:
                    {
                        // 燃烧：当前行动方回合结束时触发（由 BattleManager 在一方行动结束时触发 TURN_OVER）
                        _buffInfo.onTurnOver += () =>
                        {
                            int buffValue = (int)_buffInfo.buffValue;
                            int buffLayer = _buffInfo.buffLayer;
                            var battleCtx = BattleContext.current;
                            battleCtx.ApplyDamageToPart(_buffInfo.owner, _buffInfo.owner, buffValue * buffLayer);
                        };
                    }
                    break;
                case EBuffType.STRONG:
                    {
                        // 强壮：仅增加嘴巴攻击力，由 BuffCombatModifiers + 攻击结算处理
                    }
                    break;
                case EBuffType.PREY:
                    {
                        // 猎物：受到的伤害增加，由 BuffCombatModifiers + ApplyDamageToPart 处理
                    }
                    break;
                case EBuffType.HATE:
                    {
                        // 怨恨：无战斗效果（在特定情况下被消耗）
                    }
                    break;
                case EBuffType.MOLD:
                    {
                        _buffInfo.onTotalTurnOver += () => GermMassBuffEffects.RunMoldTotalTurnOver(_buffInfo);
                    }
                    break;
                case EBuffType.BREEDING_MASS:
                    {
                        _buffInfo.onPartActionOver += () => GermMassBuffEffects.RunBreedingMassAfterPartAction(_buffInfo);
                    }
                    break;
                case EBuffType.HEAL_MASS:
                    {
                        _buffInfo.onPartGetEffect += () => GermMassBuffEffects.RunHealMassEffect(_buffInfo);
                    }
                    break;
                case EBuffType.ATTACK_MASS:
                    {
                        _buffInfo.onPartGetEffect += () => GermMassBuffEffects.RunAttackMassEffect(_buffInfo);
                    }
                    break;
            }

            return _buffInfo;
        }
    }
}
