using SeerGoGame.Buffs;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Systems
{
    /// <summary>
    /// 恢复HP道具效果
    /// </summary>
    public class HealItemEffect : IItemEffectHandler
    {
        public override bool CanHandle(string itemType)
        {
            return itemType == "healHP" || itemType == "heal_full" || itemType == "healPP";
        }

        private readonly Dictionary<string, Func<ItemConfig, PetData, bool>> _effectHandlers;

        public HealItemEffect()
        {
            _effectHandlers = new Dictionary<string, Func<ItemConfig, PetData, bool>>
            {
                ["heal_full"] = ApplyHpHeal,
                ["healHP"] = ApplyHpHeal,
                ["healPP"] = ApplyPpHeal,
            };
        }

        public override bool ApplyEffect(ItemConfig itemConfig, PetData targetPet)
        {
            if (itemConfig == null)
            {
                SeerGoLogger.LogError("<color=red>[HealItemEffect] itemConfig 为空</color>");
                return false;
            }

            if (targetPet == null)
            {
                SeerGoLogger.LogError("<color=red>[HealItemEffect] targetPet 为空</color>");
                return false;
            }

            // 查找对应的处理方法
            if (_effectHandlers.TryGetValue(itemConfig.type, out var handler))
            {
                return handler(itemConfig, targetPet);
            }
            else
            {
                SeerGoLogger.Log($"<color=yellow>[HealItemEffect] 未知的道具类型: {itemConfig.type}</color>");
                return false;
            }
        }

        private bool ApplyHpHeal(ItemConfig itemConfig, PetData targetPet)
        {

          /*  // 计算治疗量
            int healAmount = itemConfig.type == "heal_full"
                ? targetPet.CurrentStatus.MaxHp - targetPet.CurrentStatus.Hp  // 完全恢复
                : (int)itemConfig.effectValue;  // 固定恢复量

            // 应用治疗
            int hpBefore = targetPet.CurrentStatus.Hp;
            targetPet.CurrentStatus.Hp = Mathf.Min(
                targetPet.CurrentStatus.Hp + healAmount,
                targetPet.CurrentStatus.MaxHp
            );

            int actualHeal = targetPet.CurrentStatus.Hp - hpBefore;

            // 记录日志
            SeerGoLogger.Log($"<color=cyan>[HealItemEffect] {targetPet.Basic.Name} 恢复了 {actualHeal} HP (当前 {targetPet.CurrentStatus.Hp}/{targetPet.CurrentStatus.MaxHp})</color>");

            return actualHeal >= 0;*/
          return true;
        }

        private bool ApplyPpHeal(ItemConfig itemConfig, PetData targetPet)
        {
            targetPet.Skills.NormalSkill.ForEach( skill => {
                skill.currentPP = Mathf.Min(skill.currentPP + (int)itemConfig.effectValue, skill.maxPP);
            });

            return true;
        }

        public override bool CanUse(ItemConfig itemConfig, PetData targetPet)
        {
            // 已死亡或HP已满时无法使用
          /*  if (targetPet.CurrentStatus.Hp <= 0)
            {
                return false;
            }*/
            
          /*  if (targetPet.CurrentStatus.Hp >= targetPet.CurrentStatus.MaxHp)
            {
                return false;
            }*/
            
            return true;
        }

        public override string GetUseMessage(ItemConfig itemConfig, PetData targetPet)
        {
            if (itemConfig.type == "heal_full")
            {
                return $"{targetPet.Basic.Name} 的HP已完全恢复";
            }
            else
            {
                return $"{targetPet.Basic.Name} 恢复了HP";
            }
        }
    }
}
