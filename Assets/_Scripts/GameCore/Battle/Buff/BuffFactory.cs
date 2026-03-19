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
            }

            return _buffInfo;
        }
    }
}
