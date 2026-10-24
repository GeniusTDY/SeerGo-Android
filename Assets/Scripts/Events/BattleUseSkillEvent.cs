// 事件定义
using SeerGoGame.Models;

namespace SeerGoGame.Events
{
    public struct BattleUseSkillEvent
    {
        public SkillData _skillData;
        public string _role;// PLAYER   MONSTER
        public string _action;// SWITCH_PET  USE_SKILL
        public int _petId;
        public int _skillIndex; // 技能在技能列表中的索引，用于PP扣减
        
        public BattleUseSkillEvent(string role, int petId, string action, SkillData skillData = null, int skillIndex = -1)
        {
            _skillData = skillData;
            _role = role;
            _action = action;
            _petId = petId;
            _skillIndex = skillIndex;
        }
    }
}