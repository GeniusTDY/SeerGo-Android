using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using Random = UnityEngine.Random;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 特性1022-1025：受到普通攻击时使对方异常状态
    /// Args: "状态ID 概率"
    /// 状态ID: 10=麻痹, 12=烧伤, 14=冻伤, 15=害怕
    /// </summary>
    public class NewSeEffect_CauseStatusOnHit : INewSeEffectHandler
    {
        private int _seId;
        private int _statusId;
        private string _statusName;

        public override int SeId => _seId;

        public NewSeEffect_CauseStatusOnHit(int seId, int statusId)
        {
            _seId = seId;
            _statusId = statusId;
            _statusName = GetStatusName(statusId);
        }

        private string GetStatusName(int statusId)
        {
            switch (statusId)
            {
                case 10: return "麻痹";
                case 12: return "烧伤";
                case 14: return "冻伤";
                case 15: return "害怕";
                default: return "异常";
            }
        }

        public override void OnAfterApplyDamage(NewSeConfig seConfig, IBuffable caster, IBuffable target,
            SkillData skill, string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            // Args格式: "状态ID 概率"
            // 受到攻击时使对方异常状态
            if (args == null || args.Length < 2) return;

            if (!int.TryParse(args[1], out int probability)) return;

            // 只对普通攻击有效
            if (skill.Category != 1) return;

            // caster是被攻击方（触发特性的那个）
            // target是攻击方
            // 检查目标（攻击方）是否已有该状态
            // if (target.ActiveBuffs.ContainsKey(_statusId)) return;

            if (Random.Range(1, 101) <= probability)
            {
                // 被攻击方对攻击方施加异常状态
                buffSystem.ApplyBuff(caster, target, _statusId);
                SeerGoLogger.Log($"<color=green>[特性-{_statusName}] 使对方陷入{_statusName}状态</color>");
            }
        }
    }
}
