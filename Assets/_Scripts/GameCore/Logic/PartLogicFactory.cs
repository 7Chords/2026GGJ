using GameCore.RefData;
using System;
using System.Collections.Generic;

namespace GameCore.Logic
{
    public static class PartLogicFactory
    {
        //key 部位id  value 部件逻辑
        private static Dictionary<long, PartLogic> _m_logicTypeMap;

        public static void Initialize()
        {
            _m_logicTypeMap = new Dictionary<long, PartLogic>();
        }

        private static void RegisterLogic(long _id, PartLogic _logicObj)
        {
            if (!_m_logicTypeMap.ContainsKey(_id))
            {
                _m_logicTypeMap.Add(_id, _logicObj);
            }
        }

        public static PartLogic CreateLogic(long _id , PartInfo _partInfo)
        {
            if (_m_logicTypeMap == null)
                Initialize();

            if (_id < 0 || _partInfo == null)
                return null;

            if (_m_logicTypeMap.TryGetValue(_id, out PartLogic logicObj))
            {
                return logicObj;
            }
            logicObj = new PartLogic(_partInfo);
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
            RegisterLogic(_id, logicObj);
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

                    }
                    break;
                case EAttributeType.TRIGGER_MORE:
                    {

                    }
                    break;
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

                    }
                    break;
                case EAttributeType.CLEAR_DEFULL:
                    {

                    }
                    break;
                case EAttributeType.CLEAR_BAD_SKIN:
                    {

                    }
                    break;
                case EAttributeType.PENETRATE:
                    {

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
