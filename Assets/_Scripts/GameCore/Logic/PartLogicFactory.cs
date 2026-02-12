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

        public static PartLogic CreateLogic(long _id)
        {
            if (_m_logicTypeMap == null)
                Initialize();

            if (_id < 0)
                return null;

            if (_m_logicTypeMap.TryGetValue(_id, out PartLogic logicObj))
            {
                return logicObj;
            }
            logicObj = new PartLogic();
            PartRefObj partRefObj = SCRefDataMgr.instance.partRefList.refDataList.Find(x => x.id == _id);
            if (partRefObj == null)
                return null;
            EntryEffectObj entryRefObj = null;
            for (int i =0;i< partRefObj.entryList.Count;i++)
            {
                entryRefObj = partRefObj.entryList[i];
                if (entryRefObj == null)
                    continue;
                switch (entryRefObj.triggerPointType)
                {
                    case EAttributeTriggerPointType.ACTIVE:
                        {
                            
                        }
                        break;
                    case EAttributeTriggerPointType.GET_HIT:
                        {

                        }
                        break;
                    case EAttributeTriggerPointType.DIE:
                        {

                        }
                        break;
                }
            }
            RegisterLogic(_id, logicObj);
            return logicObj;
        }

        public static Action GetActionByAttributeType(EAttributeType _attributeType)
        {
            return null;
        }
    }
}
