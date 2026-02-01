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

        /// <summary>
        /// 头部受伤加倍等逻辑
        /// </summary>
        /// <param name="damage"></param>
        public virtual void OnTakeDamage(ref float damage) { }

        public virtual void OnDamageTaken(float damage)
        {
             GameCore.GameCommon.ShowDamageFloatText((int)damage, partInfo.GetScreenPosEvent?.Invoke() ?? Vector2.zero);
        }

        /// <summary>
        /// 部位破坏时的逻辑
        /// </summary>
        public virtual void OnPartBroken() { }

        /// <summary>
        /// 回合开始
        /// </summary>
        public virtual void OnTurnStart() { }
    }
}
