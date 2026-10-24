using QFramework;
using SeerGoGame.Interfaces;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;

// 战斗效果模型实现
namespace SeerGoGame.Models
{
    public class BattleEffectModel : AbstractModel, IBattleEffectModel
    {
        public Dictionary<int, BattleEffectData> BattleEffectDictionary { get; private set; }

        protected override void OnInit()
        {
            BattleEffectDictionary = new Dictionary<int, BattleEffectData>();

        }

        // 加载战斗效果数据
        public async Task LoadBattleEffectInfoConfigAsync()
        {
            string path = ResourcePathBuilder.GetPetsInfoDataPath();
            string fullPath = $"{path}/battle_effects";

            var result = await JsonResourceLoadManager.LoadConfigAsync<BattleEffectJsonRoot>(fullPath);

            if (result.Success && result.Data?.BattleEffects != null)
            {
                foreach (var battleEffect in result.Data.BattleEffects.BattleEffect)
                {
                    // 添加到战斗效果字典（按类型）
                    if (!BattleEffectDictionary.ContainsKey(battleEffect.Type))
                    {
                        BattleEffectDictionary[battleEffect.Type] = battleEffect;
                    }
                    else
                    {
                        SeerGoLogger.LogWarning($"重复的战斗效果类型: {battleEffect.Type}");
                    }
                }
            }
        }

        // 根据类型获取战斗效果
        public BattleEffectData GetBattleEffectByType(int type)
        {
            if (BattleEffectDictionary.TryGetValue(type, out BattleEffectData battleEffect))
            {
                return battleEffect;
            }
            return null;
        }

    }
}