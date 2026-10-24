using QFramework;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;

// 技能系统实现
namespace SeerGoGame.Systems
{
    public class SkillSystem : AbstractSystem, ISkillSystem
    {
        private ISkillModel _skillModel;

        protected override void OnInit()
        {
            _skillModel = this.GetModel<ISkillModel>();
        }
      
        // 根据ID获取技能
        public SkillData GetSkillById(string id)
        {
            if (_skillModel.SkillDictionary.TryGetValue(id, out SkillData skill))
            {
                return skill;
            }
            return null;
        }

        public SkillSideEffectInfoData GetSkillSideEffectInfoById(string id)
        {
            if (_skillModel.SkillSideEffectInfoDictionary.TryGetValue(id, out SkillSideEffectInfoData effectInfoData))
            {
                return effectInfoData;
            }
            return null;
        }

        /// <summary>
        /// 根据技能ID获取技能效果描述（新版，从 SkillEffectInfo 加载）
        /// </summary>
        public SkillEffectInfoData GetSkillEffectInfoById(string id)
        {
            if (int.TryParse(id, out int skillId) && _skillModel.SkillEffectInfoDict.TryGetValue(skillId, out SkillEffectInfoData infoData))
            {
                return infoData;
            }
            return null;
        }
    }
}