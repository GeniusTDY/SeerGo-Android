using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using Random = UnityEngine.Random;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 特性1030-1033：受到任何攻击时有几率使自身能力提升
    /// Args: "能力ID 概率"
    /// 能力ID: 0=攻击, 1=防御, 2=特攻, 4=速度
    /// </summary>
    public class NewSeEffect_StatUpOnHit : INewSeEffectHandler
    {
        private int _seId;
        private int _statId;
        private string _statName;

        public override int SeId => _seId;

        public NewSeEffect_StatUpOnHit(int seId, int statId)
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
                case 1: return "防御";
                case 2: return "特攻";
                case 4: return "速度";
                default: return "能力";
            }
        }

        public override void OnAfterApplyDamage(NewSeConfig seConfig, IBuffable caster, IBuffable target,
            SkillData skill, string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            // Args格式: "能力ID 概率"
            // 受到攻击时使自身能力提升
            if (args == null || args.Length < 2) return;

            if (!int.TryParse(args[1], out int probability)) return;

            if (Random.Range(1, 101) <= probability)
            {
                // caster是被攻击方（触发特性的那个），对自己施加能力提升
                buffSystem.ApplyBuff(caster, caster, _statId + 10000);
                SeerGoLogger.Log($"<color=green>[特性-{_statName}提升] 自身{_statName}提升1个等级</color>");
            }
        }
    }
}
