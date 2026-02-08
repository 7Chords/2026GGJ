using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Logic
{
    public abstract class BasePartLogic
    {
        protected PartInfo partInfo;

        public virtual void Initialize(PartInfo _info)
        {
            partInfo = _info;
        }

        public virtual void OnGetHit(float damage)
        {
             GameCore.GameCommon.ShowDamageFloatText((int)damage, partInfo.GetAnchorTransformEvent?.Invoke());
        }

        /// <summary>
        /// 部位破坏时的逻辑
        /// </summary>
        public virtual void OnPartBroken() { }

        /// <summary>
        /// 回合开始
        /// </summary>
        public virtual void OnTurnStart() { }

        public virtual void OnPartAction() { }
    }
}
