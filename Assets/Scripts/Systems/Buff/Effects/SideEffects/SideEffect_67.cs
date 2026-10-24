using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using System;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=67: 当次攻击击败对方时减少对方下次出战精灵的最大体力1/{0}
    /// </summary>
    public class SideEffect_67 : ISkillSideEffectHandler
    {
        public override int EffectId => 67;

        public override void OnAfterBuffTurnEnd(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !float.TryParse(args[0], out float multiple))
            {
                return;
            }

            if (caster is PlayerBattleController playerCaster)
            {
                MonsterBattleController monsterBattle = target as MonsterBattleController;

                if (monsterBattle.GetCurrentMonsterData().CurrentStatus.Hp <= 0)
                {
                    buffSystem.ApplyBuff(caster, target, 20022, new Dictionary<string, object>
                    {
                        { "SwitchCount", 1 }, // 切换后生效
                        { "EffectValue", multiple },
                        { "Keep", true }
                    });
                }
            }
            else if (caster is MonsterBattleController monsterCaster)
            {
                PlayerBattleController playerBattle = target as PlayerBattleController;

                if (playerBattle.GetCurrentPetData().CurrentStatus.Hp <= 0)
                {
                    buffSystem.ApplyBuff(caster, target, 20022, new Dictionary<string, object>
                    {
                        { "SwitchCount", 1 }, // 切换后生效
                        { "EffectValue", multiple },
                        { "Keep", true }
                    });
                }
            }
        }
    }
}
