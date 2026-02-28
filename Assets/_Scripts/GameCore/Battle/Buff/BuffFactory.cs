using GameCore.RefData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

                    }
                    break;
                case EBuffType.BURN:
                    {

                    }
                    break;
            }

            return null;
        }
    }
}
