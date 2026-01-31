using UnityEngine;

namespace GameCore.Logic.Parts
{
    public class NoseLogic : BasePartLogic
    {
        public override void OnTurnStart()
        {
            Debug.Log("鼻子感知到了气味 (Turn Start)");
        }
    }

    public class MouthLogic : BasePartLogic
    {
        public override void OnTurnStart()
        {
            Debug.Log("嘴巴张开了");
        }
        
        public override void OnTakeDamage(ref float damage)
        {
            Debug.Log("嘴巴受到攻击，伤害减免 10% !");
            damage *= 0.9f;
        }
    }

    public class EyeLogic : BasePartLogic
    {
        public override void OnTurnStart()
        {
            Debug.Log("眼球转动");
        }
        public override void OnPartBroken()
        {
            Debug.Log("眼睛被打瞎了！命中率下降！");
        }
    }
}
