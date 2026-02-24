using SCFrame;
using System.Collections.Generic;

namespace GameCore.Battle
{
    public class BuffLogic
    {
        public List<BuffInfo> buffList = new List<BuffInfo>();

        /// <summary>
        /// buff的效果周期和生命周期计时(回合制）
        /// </summary>
        public void BuffTurnTick()
        {
            List<BuffInfo> deleteBuffList = new List<BuffInfo>();
            foreach (var buffInfo in buffList)
            {
                buffInfo.buffLayer--;

                if (buffInfo.buffLayer == 0)
                {
                    deleteBuffList.Add(buffInfo);
                }
                else
                {
                    SCDebugHelper.LogWarning(buffInfo.buffRefObj.buffName + ":" + buffInfo.buffLayer);
                }
            }

            foreach (var buffInfo in deleteBuffList)
            {
                RemoveBuff(buffInfo);
            }
        }

        /// <summary>
        /// 添加buff
        /// </summary>
        /// <param name="_buffInfo"></param>
        public void AddBuff(BuffInfo _buffInfo)
        {
            if (_buffInfo == null) return;
            BuffInfo findBuffInfo = findBuff(_buffInfo.buffRefObj.id);

            if (findBuffInfo != null)
            {
                findBuffInfo.buffLayer += findBuffInfo.buffLayer;
                SCMsgCenter.SendMsg(SCMsgConst.PART_BUFF_UPDATE, _buffInfo);
            }
            else
            {
                buffList.Add(_buffInfo);
                SCMsgCenter.SendMsg(SCMsgConst.PART_BUFF_ADD, _buffInfo);
            }
        }

        /// <summary>
        /// 移除buff
        /// </summary>
        /// <param name="_buffInfo"></param>
        public void RemoveBuff(BuffInfo _buffInfo)
        {
            if (!buffList.Contains(_buffInfo))
                return;

            buffList.Remove(_buffInfo);

            SCMsgCenter.SendMsg(SCMsgConst.PART_BUFF_REMOVE, _buffInfo);

        }

        public void ClearAllBuffs()
        {
            if (buffList == null)
                return;
            List<BuffInfo> deleteInfoList = new List<BuffInfo>();
            foreach (BuffInfo buffInfo in buffList)
            {
                deleteInfoList.Add(buffInfo);
            }
            buffList.Clear();
            foreach (BuffInfo buffInfo in deleteInfoList)
            {
                SCMsgCenter.SendMsg(SCMsgConst.PART_BUFF_REMOVE, buffInfo);
            }
        }

        /// <summary>
        /// 查找列表中的buff
        /// </summary>
        /// <param name="_buffDataID"></param>
        /// <returns></returns>
        private BuffInfo findBuff(long _buffDataID)
        {
            foreach (var buffInfo in buffList)
            {
                if (buffInfo.buffRefObj.id == _buffDataID)
                {
                    return buffInfo;
                }
            }

            return default;
        }



        public void TriggerPartAwakeBuff()
        {
            foreach (var buffInfo in buffList)
            {
                if (buffInfo == null)
                    continue;
                buffInfo.onPartAwake?.Invoke();
            }
        }


    }
}
