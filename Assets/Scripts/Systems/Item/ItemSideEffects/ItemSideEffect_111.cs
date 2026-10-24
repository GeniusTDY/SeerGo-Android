using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using System.Collections.Generic;

namespace SeerGoGame.Systems
{
    /// <summary>
    /// 道具效果 111: 重置单项学习力（EV）
    /// SideEffectArg: "属性名" （如 "ATK"）
    /// </summary>
    public class ItemSideEffect_111 : IItemSideEffectHandler
    {
        public override int EffectId => 111;
        public override int ArgsNum => 1;

        private static readonly Dictionary<string, string> StatDisplayNames = new()
        {
            ["HP"] = "体力", ["ATK"] = "攻击", ["DEF"] = "防御",
            ["SPATK"] = "特攻", ["SPDEF"] = "特防", ["SPD"] = "速度",
        };

        public override bool Apply(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            string statName = args[0];
            if (string.IsNullOrEmpty(statName)) return false;
            if (GetStatEV(targetPet.Talent.EV, statName) <= 0) return false;

            SetStatEV(targetPet.Talent.EV, statName, 0);

            IPetAttributeCalcSystem petAttributeCalcSystem = GetArchitecture().GetSystem<IPetAttributeCalcSystem>();
            petAttributeCalcSystem?.CalculateAllStats(targetPet);

            return true;
        }

        public override bool CanUse(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            return GetStatEV(targetPet.Talent.EV, args[0]) > 0;
        }

        public override string GetMessage(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            string displayName = StatDisplayNames.TryGetValue(args[0], out string name) ? name : args[0];
            return $"{targetPet.Basic.Name}的{displayName}学习力已重置";
        }

        private int GetStatEV(PetEV evData, string statName) => statName switch
        {
            "HP" => evData.Hp, "ATK" => evData.Atk, "DEF" => evData.Def,
            "SPATK" => evData.SpAtk, "SPDEF" => evData.SpDef, "SPD" => evData.Spd,
            _ => 0,
        };

        private void SetStatEV(PetEV evData, string statName, int value)
        {
            switch (statName)
            {
                case "HP": evData.Hp = value; break;
                case "ATK": evData.Atk = value; break;
                case "DEF": evData.Def = value; break;
                case "SPATK": evData.SpAtk = value; break;
                case "SPDEF": evData.SpDef = value; break;
                case "SPD": evData.Spd = value; break;
            }
        }
    }
}
