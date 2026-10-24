using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 衰弱效果处理器：每层降低造成伤害的20%
    /// </summary>
    public class WeakBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType)
        {
            return buffType == "WEAKEN";
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            if (buff.Config.type != statType)
                return -10001f;

            // 每层衰弱降低伤害20%
            // EffectValue 表示层数
            float multiplier = 1f - (buff.EffectValue * 0.2f);
            return Mathf.Max(multiplier, 0.1f); // 最低10%
        }

        public override void OnTurnEnd(IBuffable character, BattleEntityController opponent, BuffInstance buff)
        {
            // 衰弱无需每回合处理，通过 GetStatMultiplier 生效
        }
    }
}
