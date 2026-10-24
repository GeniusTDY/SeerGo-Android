using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using Random = UnityEngine.Random;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 特性1026-1029：受到特殊攻击时使对方能力下降
    /// Args: "能力ID 概率"
    /// 能力ID: 0=攻击, 2=特攻, 3=特防, 4=速度
    /// </summary>
    public class NewSeEffect_StatDownOnHit : INewSeEffectHandler
    {
        private int _seId;
        private int _statId;
        private string _statName;

        public override int SeId => _seId;

        public NewSeEffect_StatDownOnHit(int seId, int statId)
        {
            _seId = seId;
            _statId = statId;
            _statName = GetStatName(statId);
        }

        private string GetStatName(int statId)
        {
            switch (statId)
            {
                case 0: return "攻击";
                case 2: return "特攻";
                case 3: return "特防";
                case 4: return "速度";
                default: return "能力";
            }
        }

        public override void OnAfterApplyDamage(NewSeConfig seConfig, IBuffable caster, IBuffable target,
            SkillData skill, string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            // Args格式: "能力ID 概率"
            // 受到特殊攻击时使对方能力下降
            if (args == null || args.Length < 2) return;

            if (!int.TryParse(args[1], out int probability)) return;

            // 只对特殊攻击有效
            if (skill.Category != 2) return;

            // caster是被攻击方（触发特性的那个）
            // target是攻击方
            if (Random.Range(1, 101) <= probability)
            {
                // 被攻击方对攻击方施加能力下降
                buffSystem.ApplyBuff(caster, target, _statId + 20000);
                SeerGoLogger.Log($"<color=green>[特性-{_statName}下降] 使对方{_statName}降低1个等级</color>");
            }
        }
    }
}
