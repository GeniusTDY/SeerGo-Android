using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=195: 无视对手双防能力提升状态
    /// argsNum=0
    /// 在伤害计算前将对手防御/特防的正向buff暂时清零，计算后恢复
    /// </summary>
    public class SideEffect_195 : ISkillSideEffectHandler
    {
        public override int EffectId => 195;

        private float _savedDefValue = float.MinValue;
        private float _savedSpdefValue = float.MinValue;

        public override void OnBeforeDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (target == null) return;

            _savedDefValue = float.MinValue;
            _savedSpdefValue = float.MinValue;

            // 无视防御正向提升
            if (target.ActiveBuffs.TryGetValue(10001, out var defBuff) && defBuff.EffectValue > 0)
            {
                _savedDefValue = defBuff.EffectValue;
                defBuff.EffectValue = 0;
                SeerGoLogger.Log("[无视防御] 已忽略对手防御正向提升效果");
            }

            // 无视特防正向提升
            if (target.ActiveBuffs.TryGetValue(10003, out var spdefBuff) && spdefBuff.EffectValue > 0)
            {
                _savedSpdefValue = spdefBuff.EffectValue;
                spdefBuff.EffectValue = 0;
                SeerGoLogger.Log("[无视防御] 已忽略对手特防正向提升效果");
            }
        }

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            // 恢复被清零的防御buff值
            if (_savedDefValue > 0 && target != null)
            {
                if (target.ActiveBuffs.TryGetValue(10001, out var defBuff))
                    defBuff.EffectValue = _savedDefValue;
            }

            if (_savedSpdefValue > 0 && target != null)
            {
                if (target.ActiveBuffs.TryGetValue(10003, out var spdefBuff))
                    spdefBuff.EffectValue = _savedSpdefValue;
            }

            _savedDefValue = float.MinValue;
            _savedSpdefValue = float.MinValue;
        }
    }
}
