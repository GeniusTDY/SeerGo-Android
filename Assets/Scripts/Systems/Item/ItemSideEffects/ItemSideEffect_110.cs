using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Systems
{
    /// <summary>
    /// 道具效果 110: 增加学习力（EV）
    /// SideEffectArg: "属性名 增加值" （如 "SPATK 255"）
    /// 单项属性上限255，总上限510
    /// </summary>
    public class ItemSideEffect_110 : IItemSideEffectHandler
    {
        public override int EffectId => 110;
        public override int ArgsNum => 2;

        private static readonly Dictionary<string, string> StatDisplayNames = new()
        {
            ["HP"] = "生命", ["ATK"] = "攻击", ["DEF"] = "防御",
            ["SPATK"] = "特攻", ["SPDEF"] = "特防", ["SPD"] = "速度",
        };

        public override bool Apply(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            string statName = args[0];
            if (string.IsNullOrEmpty(statName)) return false;

            if (!int.TryParse(args[1], out int addValue) || addValue <= 0) return false;

            int actualAdd = CalculateEVToAdd(targetPet.Talent.EV, statName, addValue);
            if (actualAdd <= 0) return false;

            AddEVToStat(targetPet.Talent.EV, statName, actualAdd);

            IPetAttributeCalcSystem petAttributeCalcSystem = GetArchitecture().GetSystem<IPetAttributeCalcSystem>();
            petAttributeCalcSystem?.CalculateAllStats(targetPet);

            return true;
        }

        public override bool CanUse(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            string statName = args[0];
            if (string.IsNullOrEmpty(statName)) return false;
            if (CalculateTotalEV(targetPet.Talent.EV) >= 510) return false;
            if (GetStatEV(targetPet.Talent.EV, statName) >= 255) return false;
            return true;
        }

        public override string GetMessage(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            string displayName = StatDisplayNames.TryGetValue(args[0], out string name) ? name : args[0];
            return $"{targetPet.Basic.Name}的{displayName}学习力增加了";
        }

        private int CalculateEVToAdd(PetEV evData, string statName, int addValue)
        {
            int remainingTotal = 510 - CalculateTotalEV(evData);
            if (remainingTotal <= 0) return 0;
            int remainingStat = 255 - GetStatEV(evData, statName);
            if (remainingStat <= 0) return 0;
            return Mathf.Min(addValue, remainingTotal, remainingStat);
        }

        private int CalculateTotalEV(PetEV evData) =>
            evData.Hp + evData.Atk + evData.Def + evData.SpAtk + evData.SpDef + evData.Spd;

        private int GetStatEV(PetEV evData, string statName) => statName switch
        {
            "HP" => evData.Hp, "ATK" => evData.Atk, "DEF" => evData.Def,
            "SPATK" => evData.SpAtk, "SPDEF" => evData.SpDef, "SPD" => evData.Spd,
            _ => 0,
        };

        private void AddEVToStat(PetEV evData, string statName, int evToAdd)
        {
            switch (statName)
            {
                case "HP": evData.Hp = Mathf.Min(255, evData.Hp + evToAdd); break;
                case "ATK": evData.Atk = Mathf.Min(255, evData.Atk + evToAdd); break;
                case "DEF": evData.Def = Mathf.Min(255, evData.Def + evToAdd); break;
                case "SPATK": evData.SpAtk = Mathf.Min(255, evData.SpAtk + evToAdd); break;
                case "SPDEF": evData.SpDef = Mathf.Min(255, evData.SpDef + evToAdd); break;
                case "SPD": evData.Spd = Mathf.Min(255, evData.Spd + evToAdd); break;
            }
        }
    }
}
