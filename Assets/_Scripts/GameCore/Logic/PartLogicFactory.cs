using GameCore.RefData;
using System.Collections.Generic;

namespace GameCore.Logic
{
    public static class PartLogicFactory
    {
        //key 部位id  value 部件逻辑
        private static Dictionary<long, BasePartLogic> _m_logicTypeMap;

        public static void Initialize()
        {
            _m_logicTypeMap = new Dictionary<long, BasePartLogic>();
        }

        private static void RegisterLogic(long _id, BasePartLogic _logicObj)
        {
            if (!_m_logicTypeMap.ContainsKey(_id))
            {
                _m_logicTypeMap.Add(_id, _logicObj);
            }
        }

        public static BasePartLogic CreateLogic(long _id)
        {
            if (_m_logicTypeMap == null)
                Initialize();

            if (_id < 0)
                return null;

            if (_m_logicTypeMap.TryGetValue(_id, out BasePartLogic logicObj))
            {
                return logicObj;
            }
            PartRefObj partRefObj = SCRefDataMgr.instance.partRefList.refDataList.Find(x => x.id == _id);
            if (partRefObj == null)
                return null;
            logicObj = null;
            switch (partRefObj.partName)
            {
                //todo
            }

            return logicObj;
        }
    }
}
