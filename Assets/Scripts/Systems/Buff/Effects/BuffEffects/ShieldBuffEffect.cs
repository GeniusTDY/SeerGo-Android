using SeerGoGame.Interfaces;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Linq;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 护盾效果处理器
    /// </summary>
    public class ShieldBuffEffect : IBuffEffectHandler
    {
        // 实现接口方法：判断是否能处理该类型Buff
        public override bool CanHandle(string buffType) => buffType == "SHIELD";

        public static int CalculateBlock(IBuffable character, int damage)
        {
            if (character == null || damage <= 0 || character.ActiveBuffs == null || character.ActiveBuffs.Count == 0)
                return damage;

            // int remainingDamage = damage;
            // 2. 找到第一个有效护盾
            var shieldBuff = character.ActiveBuffs.Values.FirstOrDefault(buff => buff.Config?.type == "SHIELD");
            if (shieldBuff == null) return damage;

            int currentShield = (int)shieldBuff.EffectValue;
            // 3. 分三种场景处理
            if (currentShield > damage)
            {
                // 场景1：护盾>伤害 → 更新护盾值，返回0
                shieldBuff.EffectValue = currentShield - damage;
                damage = 0;
            }
            else
            {
                // 场景2：护盾=伤害  /  场景3：护盾<伤害 → 抵扣后移除Buff，返回对应值
                damage -= currentShield;
                // 直接移除护盾Buff
                var shieldKey = character.ActiveBuffs.First(kvp => kvp.Value == shieldBuff).Key;
                character.ActiveBuffs.Remove(shieldKey);
            }
            // 4. 返回扣除后的伤害
            return damage;
        }

        /// <summary>
        /// 接口实现：返回属性乘数
        /// </summary>
        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            return -10001f;
        }
    }
}
