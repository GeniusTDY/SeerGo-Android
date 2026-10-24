using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using UnityEngine;

namespace SeerGoGame.Systems
{
    /// <summary>
    /// 学习力道具效果（增加EV）
    /// 道具601-606分别对应 HP/ATK/DEF/SPATK/SPDEF/SPD
    /// 单项属性上限255，总上限510
    /// </summary>
    public class EVAddItemEffect : IItemEffectHandler
    {
        // 道具ID映射到属性名（当effectArgs为空时的备用映射）
        private static readonly System.Collections.Generic.Dictionary<int, string> IdStatMap = new()
        {
            [606] = "HP",
            [602] = "ATK",
            [604] = "DEF",
            [601] = "SPATK",
            [605] = "SPDEF",
            [603] = "SPD",
        };

        // 属性英文名到中文显示名映射
        private static readonly System.Collections.Generic.Dictionary<string, string> StatDisplayNames = new()
        {
            ["HP"] = "生命",
            ["ATK"] = "攻击",
            ["DEF"] = "防御",
            ["SPATK"] = "特攻",
            ["SPDEF"] = "特防",
            ["SPD"] = "速度",
        };

        public override bool CanHandle(string itemType)
        {
            return itemType == "addEV";
        }

        public override bool ApplyEffect(ItemConfig itemConfig, PetData targetPet)
        {
            string statName = GetStatName(itemConfig);
            if (string.IsNullOrEmpty(statName))
                return false;

            int addValue = (int)itemConfig.effectValue;
            int actualAdd = CalculateEVToAdd(targetPet.Talent.EV, statName, addValue);
            if (actualAdd <= 0)
                return false;

            // 增加学习力
            AddEVToStat(targetPet.Talent.EV, statName, actualAdd);

            // 重新计算属性
            IPetAttributeCalcSystem petAttributeCalcSystem = GetArchitecture().GetSystem<IPetAttributeCalcSystem>();
            petAttributeCalcSystem?.CalculateAllStats(targetPet);

            return true;
        }

        public override bool CanUse(ItemConfig itemConfig, PetData targetPet)
        {
            string statName = GetStatName(itemConfig);
            if (string.IsNullOrEmpty(statName))
                return false;

            // 总EV已达上限
            if (CalculateTotalEV(targetPet.Talent.EV) >= 510)
                return false;

            // 该单项已达上限
            if (GetStatEV(targetPet.Talent.EV, statName) >= 255)
                return false;

            return true;
        }

        public override string GetUseMessage(ItemConfig itemConfig, PetData targetPet)
        {
            string statName = GetStatName(itemConfig);
            string displayName = StatDisplayNames.TryGetValue(statName, out string name) ? name : statName;
            return $"{targetPet.Basic.Name}的{displayName}学习力增加了";
        }

        /// <summary>
        /// 获取道具对应的属性名：优先从effectArgs读取，否则根据道具ID映射
        /// </summary>
        private string GetStatName(ItemConfig itemConfig)
        {
            if (!string.IsNullOrEmpty(itemConfig.effectArgs))
                return itemConfig.effectArgs;

            if (IdStatMap.TryGetValue(itemConfig.id, out string statName))
                return statName;

            return null;
        }

        /// <summary>
        /// 计算实际能增加的EV值（考虑总上限510和单项上限255）
        /// </summary>
        private int CalculateEVToAdd(PetEV evData, string statName, int addValue)
        {
            int currentTotal = CalculateTotalEV(evData);
            int remainingTotal = 510 - currentTotal;
            if (remainingTotal <= 0)
                return 0;

            int currentStatEV = GetStatEV(evData, statName);
            int remainingStat = 255 - currentStatEV;
            if (remainingStat <= 0)
                return 0;

            return Mathf.Min(addValue, remainingTotal, remainingStat);
        }

        /// <summary>
        /// 计算当前总EV值
        /// </summary>
        private int CalculateTotalEV(PetEV evData)
        {
            return evData.Hp + evData.Atk + evData.Def +
                   evData.SpAtk + evData.SpDef + evData.Spd;
        }

        /// <summary>
        /// 获取指定属性的当前EV值
        /// </summary>
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

        /// <summary>
        /// 为指定属性增加EV值（上限255）
        /// </summary>
        private void AddEVToStat(PetEV evData, string statName, int evToAdd)
        {
            switch (statName)
            {
                case "HP":
                    evData.Hp = Mathf.Min(255, evData.Hp + evToAdd);
                    break;
                case "ATK":
                    evData.Atk = Mathf.Min(255, evData.Atk + evToAdd);
                    break;
                case "DEF":
                    evData.Def = Mathf.Min(255, evData.Def + evToAdd);
                    break;
                case "SPATK":
                    evData.SpAtk = Mathf.Min(255, evData.SpAtk + evToAdd);
                    break;
                case "SPDEF":
                    evData.SpDef = Mathf.Min(255, evData.SpDef + evToAdd);
                    break;
                case "SPD":
                    evData.Spd = Mathf.Min(255, evData.Spd + evToAdd);
                    break;
            }
        }
    }
}
