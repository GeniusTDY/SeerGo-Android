using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using Random = UnityEngine.Random;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 特性1021：回神 - 体力降低到1/8时有5%几率体力回满
    /// Args: "8 5" - 第一个参数是HP比例分母，第二个是概率
    /// </summary>
    public class NewSeEffect_1021 : INewSeEffectHandler
    {
        public override int SeId => 1021;

        public override void OnAfterApplyDamage(NewSeConfig seConfig, IBuffable caster, IBuffable target,
            SkillData skill, string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            // Args格式: "HP比例分母 概率"
            // 回神效果：在伤害应用后检查是否触发回血
            if (args == null || args.Length < 2) return;

            if (!int.TryParse(args[0], out int hpRatio)) return;
            if (!int.TryParse(args[1], out int probability)) return;

            // caster是被攻击方（触发回神特性的那个）
            // target是攻击方
            if (caster is PlayerBattleController playerTarget)
            {
                var petData = playerTarget.GetCurrentPetData();
                if (petData != null)
                {
                    int threshold = petData.CurrentStatus.MaxHp / hpRatio;
                    if (petData.CurrentStatus.Hp > 0 && petData.CurrentStatus.Hp <= threshold)
                    {
                        if (Random.Range(1, 101) <= probability)
                        {
                            int fullHp = petData.CurrentStatus.MaxHp;
                            int healAmount = fullHp - petData.CurrentStatus.Hp;
                            playerTarget.SetPlayerHp(-healAmount, petData);
                            SeerGoLogger.Log($"<color=green>[特性-回神] 玩家精灵触发回神效果，体力回满 ({petData.CurrentStatus.Hp} -> {fullHp})</color>");
                        }
                    }
                }
            }
            else if (caster is MonsterBattleController monsterTarget)
            {
                var petData = monsterTarget.GetCurrentMonsterData();
                if (petData != null)
                {
                    int threshold = petData.CurrentStatus.MaxHp / hpRatio;
                    if (petData.CurrentStatus.Hp > 0 && petData.CurrentStatus.Hp <= threshold)
                    {
                        if (Random.Range(1, 101) <= probability)
                        {
                            int fullHp = petData.CurrentStatus.MaxHp;
                            int healAmount = fullHp - petData.CurrentStatus.Hp;
                            monsterTarget.SetMonsterHp(-healAmount, petData);
                            SeerGoLogger.Log($"<color=green>[特性-回神] 敌方精灵触发回神效果，体力回满 ({petData.CurrentStatus.Hp} -> {fullHp})</color>");
                        }
                    }
                }
            }
        }
    }
}
