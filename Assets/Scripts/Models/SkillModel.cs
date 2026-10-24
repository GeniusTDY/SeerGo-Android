// 技能模型实现
using QFramework;
using SeerGoGame.Helper;
using SeerGoGame.Interfaces;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SeerGoGame.Models
{
    public class SkillModel : AbstractModel, ISkillModel
    {
        public Dictionary<string, SkillData> SkillDictionary { get; private set; }

        public Dictionary<string, SkillSideEffectInfoData> SkillSideEffectInfoDictionary { get; private set; }

        public Dictionary<int, SkillEffectInfoData> SkillEffectInfoDict { get; private set; }

        public Dictionary<int, MoveSideEffect> SideEffects { get; private set; }
        public Dictionary<int, EffectInfo> EffectInfos { get; private set; }

        protected override void OnInit()
        {
            SkillDictionary = new Dictionary<string, SkillData>();
            SkillSideEffectInfoDictionary = new Dictionary<string, SkillSideEffectInfoData>();
            SkillEffectInfoDict = new Dictionary<int, SkillEffectInfoData>();

            SideEffects = new Dictionary<int, MoveSideEffect>();
            EffectInfos = new Dictionary<int, EffectInfo>();

            var sideEffectConfig = ConfigHelper.LoadJsonConfig<MoveSideEffectRoot>("Config/moves_sideeffects");
            if (sideEffectConfig != null && sideEffectConfig.sideeffects != null)
            {
                foreach (var effect in sideEffectConfig.sideeffects)
                    SideEffects[effect.ID] = effect;
            }

            var effectInfoConfig = ConfigHelper.LoadJsonConfig<EffectInfoRoot>("Config/EffectInfo");
            if (effectInfoConfig != null && effectInfoConfig.root?.Effect != null)
            {
                foreach (var effect in effectInfoConfig.root.Effect)
                    EffectInfos[effect.id] = effect;
            }
        }

        public async Task LoadSkillData()
        {
            string path = ResourcePathBuilder.GetPetsInfoDataPath();
            var result = await JsonResourceLoadManager.LoadConfigAsync<MovesDataRoot>($"{path}/skill");

            if (result.Success && result.Data?.moves != null)
            {
                foreach (var skillData in result.Data.moves)
                {
                    if (!SkillDictionary.ContainsKey(skillData.ID))
                        SkillDictionary[skillData.ID] = skillData;
                    else
                        SeerGoLogger.LogWarning($"重复的技能ID: {skillData.ID}");
                }
            }
        }

        public async Task LoadSkillSideEffectInfoData()
        {
            // 旧版保留，新版使用 LoadSkillEffectInfoConfigAsync
        }

        /// <summary>
        /// 直接从 SkillEffectInfo.xlsx 加载技能效果描述（只加载一次，后续调用跳过）
        /// </summary>
        public async Task LoadSkillEffectInfoConfigAsync()
        {
            if (SkillEffectInfoDict.Count > 0) return; // 已加载，跳过

            await Task.Yield(); // 让出主线程

            var list = XlsxSkillEffectLoader.LoadFromXlsx();
            if (list == null) return;

            foreach (var infoData in list)
            {
                if (!SkillEffectInfoDict.ContainsKey(infoData.id))
                    SkillEffectInfoDict[infoData.id] = infoData;
            }

            list.Clear();
            list = null;

            SeerGoLogger.Log($"技能效果介绍加载完成，共 {SkillEffectInfoDict.Count} 条");
        }
    }
}