using SeerGoGame.Models;
using SeerGoGame.Systems;

namespace SeerGoGame.Interfaces
{
    /// <summary>
    /// Buff效果处理器接口
    /// 每种buff类型实现自己的效果逻辑
    /// </summary>
    public abstract class IBuffEffectHandler
    {
        /// <summary>
        /// 该处理器是否能处理指定类型的buff
        /// </summary>
        public abstract bool CanHandle(string buffType);
        /// <summary>
        /// 获取属性加成(攻击/防御/速度等)
        /// </summary>
        public abstract float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType);

        /// <summary>
        /// 处理回合开始时的buff效果
        /// </summary>
        public virtual void OnTurnStart(IBuffable character, BattleEntityController opponent, BuffInstance buff) { }

        /// <summary>
        /// 处理回合结束时的buff效果
        /// </summary>
        public virtual void OnTurnEnd(IBuffable character, BattleEntityController opponent, BuffInstance buff) { }




    }
}
