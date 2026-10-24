using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using System.Collections.Generic;

namespace SeerGoGame.Systems
{
    /// <summary>
    /// 单属性学习力重置道具效果
    /// 道具608-613分别对应 攻击/防御/特攻/特防/速度/体力
    /// 通过effectArgs指定要重置的属性名
    /// </summary>
    public class EVStatResetItemEffect : IItemEffectHandler
    {
        private static readonly Dictionary<string, string> StatDisplayNames = new()
        {
            ["HP"] = "体力",
            ["ATK"] = "攻击",
            ["DEF"] = "防御",
            ["SPATK"] = "特攻",
            ["SPDEF"] = "特防",
            ["SPD"] = "速度",
        };

        public override bool CanHandle(string itemType)
        {
            return itemType == "resetEVStat";
        }

        public override bool ApplyEffect(ItemConfig itemConfig, PetData targetPet)
        {
            string statName = itemConfig.effectArgs;
            if (string.IsNullOrEmpty(statName))
                return false;

            int currentEV = GetStatEV(targetPet.Talent.EV, statName);
            if (currentEV <= 0)
                return false;

            // 重置指定属性学习力
            SetStatEV(targetPet.Talent.EV, statName, 0);

            // 重新计算属性
            IPetAttributeCalcSystem petAttributeCalcSystem = GetArchitecture().GetSystem<IPetAttributeCalcSystem>();
            petAttributeCalcSystem?.CalculateAllStats(targetPet);

            return true;
        }

        public override bool CanUse(ItemConfig itemConfig, PetData targetPet)
        {
            string statName = itemConfig.effectArgs;
            if (string.IsNullOrEmpty(statName))
                return false;

            return GetStatEV(targetPet.Talent.EV, statName) > 0;
        }

        public override string GetUseMessage(ItemConfig itemConfig, PetData targetPet)
        {
            string statName = itemConfig.effectArgs;
            string displayName = StatDisplayNames.TryGetValue(statName, out string name) ? name : statName;
            return $"{targetPet.Basic.Name}的{displayName}学习力已重置";
        }

        private int GetStatEV(PetEV evData, string statName)
        {
            return statName switch
            {
                "HP" => evData.Hp,
                "ATK" => evData.Atk,
                "DEF" => evData.Def,
                "SPATK" => evData.SpAtk,
                "SPDEF" => evData.SpDef,
                "SPD" => evData.Spd,
                _ => 0,
            };
        }

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
