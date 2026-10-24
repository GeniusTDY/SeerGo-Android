using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=1044: 吸取对手能力提升状态，吸取成功则下{0}回合造成的伤害翻倍
    /// argsNum=1, args[0]=伤害翻倍持续回合数
    /// </summary>
    public class SideEffect_1044 : ISkillSideEffectHandler
    {
        private static readonly int[] StatChangeBuffIds =
        {
            10000, // 攻击
            10001, // 防御
            10002, // 特攻
            10003, // 特防
            10004, // 速度
            10005, // 命中
        };

        public override int EffectId => 1044;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int damageDoubleDuration))
            {
                SeerGoLogger.LogError($"[SideEffect_1044] 参数解析失败");
                return;
            }

            if (!(caster is BattleEntityController casterController) || !(target is BattleEntityController targetController))
                return;

            bool stolen = false;

            foreach (var statId in StatChangeBuffIds)
            {
                if (target.ActiveBuffs.TryGetValue(statId, out var opponentBuff) && opponentBuff.EffectValue > 0)
                {
                    // 复制对手的正面能力提升buff到自身
                    var copiedBuff = new BuffInstance
                    {
                        Config = opponentBuff.Config,
                        RemainingTurns = opponentBuff.RemainingTurns,
                        EffectValue = opponentBuff.EffectValue,
                        MaxStackCount = opponentBuff.MaxStackCount,
                        Keep = opponentBuff.Keep,
                        SwitchCount = opponentBuff.SwitchCount,
                        TargetType = BuffTargetType.Self,
                        Args = opponentBuff.Args
                    };
                    caster.AddBuff(copiedBuff);
                    SeerGoLogger.Log($"[吸取] 复制对手{opponentBuff.Config.name} {opponentBuff.EffectValue:+#;-#;0} → 自身");

                    // 移除对手的该能力提升buff
                    target.RemoveBuff(statId);
                    SeerGoLogger.Log($"[吸取] 移除对手{opponentBuff.Config.name}提升状态");

                    stolen = true;
                }
            }

            // 吸取成功后，施加伤害翻倍buff
            if (stolen && damageDoubleDuration > 0)
            {
                buffSystem.ApplyBuff(caster, caster, 20045, new Dictionary<string, object>
                {
                    { "RemainingTurns", damageDoubleDuration.ToString() },
                    { "EffectValue", 2f }
                });

                SeerGoLogger.Log($"[SideEffect_1044] 吸取成功，施加{damageDoubleDuration}回合伤害翻倍buff");
            }
            else if (!stolen)
            {
                SeerGoLogger.Log("[SideEffect_1044] 对手无能力提升状态，未触发");
            }
        }
    }
}
