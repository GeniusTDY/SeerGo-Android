using QFramework;
using SeerGoGame.Models;

// 技能系统接口
namespace SeerGoGame.Interfaces
{
    public interface ISkillSystem : ISystem
    {
        SkillData GetSkillById(string id);
        SkillSideEffectInfoData GetSkillSideEffectInfoById(string id);
        SkillEffectInfoData GetSkillEffectInfoById(string id);
    }
}