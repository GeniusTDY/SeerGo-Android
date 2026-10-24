using QFramework;
using SeerGoGame.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

// 技能模型接口
namespace SeerGoGame.Interfaces
{
    public interface ISkillModel : IModel
    {
        Dictionary<string, SkillData> SkillDictionary { get; }
        Dictionary<string, SkillSideEffectInfoData> SkillSideEffectInfoDictionary { get; }
        Dictionary<int, SkillEffectInfoData> SkillEffectInfoDict { get; }
        Task LoadSkillData();
        Task LoadSkillSideEffectInfoData();
        Task LoadSkillEffectInfoConfigAsync();

        Dictionary<int, MoveSideEffect> SideEffects { get; }
        Dictionary<int, EffectInfo> EffectInfos { get; }
    }
}