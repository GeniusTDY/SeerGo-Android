using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Systems;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 水火草技能循环
    /// </summary>
    public class LoopTypeSkillDamageBuffEffect : IBuffEffectHandler
    {
        // 定义属性常量
        private const int Grass = 1;
        private const int Water = 2;
        private const int Fire = 3;
        // 核心状态变量：记录下一次攻击期望的属性，初始为水(2)
        private static int _expectedElement = Water;

        public override bool CanHandle(string buffType)
        {
            return buffType == "LOOP_TYPE_SKILL_DAMAGE";
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            return -10001f;
        }

        public static bool isContainLoopSkill(IBuffable character)
        {
            return character.ActiveBuffs.ContainsKey(30014);
        }

        /// <summary>
        /// 检查是否水火草技能循环
        /// </summary>
        public static bool IsLoopSkill(IBuffable character)
        {
            if (RoundContext.Current.PlayerSkill != null &&
            RoundContext.Current.PlayerSkill.Type == _expectedElement)
            {
                // 满足所有条件时，更新状态机并返回true
                _expectedElement = GetNextElementInCycle(_expectedElement);
                return true;
            }

            return false;
        }

        // 根据当前状态，返回循环中的下一个属性
        private static int GetNextElementInCycle(int currentElement)
        {
            switch (currentElement)
            {
                case Water: return Fire;   // 水之后是火
                case Fire: return Grass;    // 火之后是草
                case Grass: return Water;   // 草之后回到水
                default: return Water;      // 默认情况，理论上不会发生
            }
        }

    }
}
