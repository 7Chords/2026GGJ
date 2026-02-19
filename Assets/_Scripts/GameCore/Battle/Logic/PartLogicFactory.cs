using GameCore.Battle;
using GameCore.RefData;
using System;
using System.Collections.Generic;

namespace GameCore.Battle
{
    public static class PartLogicFactory
    {
        public static PartLogic CreateLogic(long _id , PartInfo _partInfo)
        {
            if (_id < 0 || _partInfo == null)
                return null;

            PartLogic logicObj = new PartLogic(_partInfo);
            EntryInfo entryInfo = null;
            for (int i =0;i< _partInfo.entryInfoList.Count;i++)
            {
                entryInfo = _partInfo.entryInfoList[i];
                if (entryInfo == null)
                    continue;
                switch (entryInfo.triggerPointType)
                {
                    case EAttributeTriggerPointType.ACTIVE:
                        {
                            logicObj.RegisterPartActiveAction(GetActionByAttributeType(_partInfo, entryInfo));
                        }
                        break;
                    case EAttributeTriggerPointType.GET_HIT:
                        {
                            logicObj.RegisterPartGetHitAction(GetActionByAttributeType(_partInfo, entryInfo));
                        }
                        break;
                    case EAttributeTriggerPointType.DIE:
                        {
                            logicObj.RegisterPartDieAction(GetActionByAttributeType(_partInfo, entryInfo));

                        }
                        break;
                }
            }
            return logicObj;
        }


        public static void RecreateActiveLogic(PartLogic _partLogic, PartInfo _partInfo)
        {
            if (_partLogic == null || _partInfo == null)
                return;
            _partLogic.ClearOnPartActiveAction();
            EntryInfo entryInfo = null;
            for (int i = 0; i < _partInfo.entryInfoList.Count; i++)
            {
                entryInfo = _partInfo.entryInfoList[i];
                if (entryInfo == null)
                    continue;
                switch (entryInfo.triggerPointType)
                {
                    case EAttributeTriggerPointType.ACTIVE:
                        {
                            _partLogic.RegisterPartActiveAction(GetActionByAttributeType(_partInfo, entryInfo));
                        }
                        break;
                }
            }
        }
        public static void RecreateGetHitLogic(PartLogic _partLogic, PartInfo _receiverPartInfo, PartInfo _senderPartInfo,int _damage)
        {
            if (_partLogic == null || _receiverPartInfo == null)
                return;
            _partLogic.ClearOnPartGetHitAction();
            EntryInfo entryInfo = null;
            for (int i = 0; i < _receiverPartInfo.entryInfoList.Count; i++)
            {
                entryInfo = _receiverPartInfo.entryInfoList[i];
                if (entryInfo == null)
                    continue;
                switch (entryInfo.triggerPointType)
                {
                    case EAttributeTriggerPointType.GET_HIT:
                        {
                            _partLogic.RegisterPartGetHitAction(GetActionByAttributeType(_receiverPartInfo, entryInfo, _senderPartInfo,_damage));
                        }
                        break;
                }
            }
        }
        public static Action GetActionByAttributeType(PartInfo _info, EntryInfo _entryInfo, PartInfo _senderPartInfo = null, int _damage = 0)
        {
            if (_info == null)
                return null;

            // 优先使用注册表，便于扩展新效果
            var handler = PartEffectHandlerRegistry.Get(_entryInfo.attributeType);
            if (handler != null)
            {
                var ctx = _senderPartInfo != null ? PartEffectContext.GetHit(_senderPartInfo, _damage) : PartEffectContext.Active;
                var info = _info;
                var entry = _entryInfo;
                return () => handler.Execute(info, entry, ctx);
            }

            switch (_entryInfo.attributeType)
            {
                case EAttributeType.ATTACK:
                    {
                        return () =>
                        {
                            PartLogicHandler.DealAttack(_info,_entryInfo);
                        };
                    }
                case EAttributeType.REFLECT:
                    {
                        return () =>
                        {
                            PartLogicHandler.DealReflect(_info, _entryInfo, _senderPartInfo, _damage);
                        };
                    }
                case EAttributeType.TRIGGER_MORE:
                    {
                        return () =>
                        {
                            PartLogicHandler.DealTriggerMore(_info, _entryInfo);
                        };
                    }
                case EAttributeType.DAMAGE_MULTIPILER:
                    {
                        return () =>
                        {
                            PartLogicHandler.DealAttackMultiplier(_info, _entryInfo);
                        };
                    }
                case EAttributeType.HIT_CHANCE_UP:
                    {

                    }
                    break;
                case EAttributeType.HIT_CHANCE_DOWN:
                    {

                    }
                    break;
                case EAttributeType.TRIGGER_CHANCE_UP:
                    {

                    }
                    break;
                case EAttributeType.HEAL_PART:
                    {
                        return () =>
                        {
                            PartLogicHandler.DealHealPart(_info, _entryInfo);
                        };
                    }
                case EAttributeType.CLEAR_DEFULL:
                    {

                    }
                    break;
                case EAttributeType.CLEAR_BAD_SKIN:
                    {

                    }
                    break;
                case EAttributeType.REAL_ATTACK:
                    {
                        return () =>
                        {
                            PartLogicHandler.DealRealAttack(_info, _entryInfo);
                        };
                    }
                case EAttributeType.PART_LOSE_TURN:
                    {
                        return () =>
                        {
                            PartLogicHandler.DealPartLoseTurn(_info, _entryInfo);
                        };
                    }
            }



            return null;
        }
    }
}
