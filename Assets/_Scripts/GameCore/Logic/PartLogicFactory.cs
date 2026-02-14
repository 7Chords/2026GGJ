using GameCore.RefData;
using System;
using System.Collections.Generic;

namespace GameCore.Logic
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

        public static Action GetActionByAttributeType(PartInfo _info,EntryInfo _entryInfo)
        {
            if (_info == null)
                return null;
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

                        return null;
                    }
                case EAttributeType.TRIGGER_MORE:
                    {
                        return () =>
                        {
                            PartLogicHandler.DealTriggerMore(_info, _entryInfo);
                        };
                    }
                case EAttributeType.ATTACK_MORE:
                    {

                    }
                    break;
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
                    break;
                case EAttributeType.PART_LOSE_TURN:
                    {

                    }
                    break;
            }



            return null;
        }
    }
}
