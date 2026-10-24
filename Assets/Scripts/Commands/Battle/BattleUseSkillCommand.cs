using QFramework;
using SeerGoGame.Events;
using SeerGoGame.Models;

namespace SeerGoGame.Commands
{
    public class BattleUseSkillCommand : AbstractCommand
    {
        private SkillData _skillData;
        private string _role;
        private string _action;
        private int _petId;
        private int _skillIndex; // 技能在技能列表中的索引

        public BattleUseSkillCommand(string role, int petId, string action, SkillData skillData = null, int skillIndex = -1) {
            _skillData = skillData;
            _role = role;
            _action = action;
            _petId = petId;
            _skillIndex = skillIndex;
        }

        protected override void OnExecute()
        {
            // 玩家使用精灵技能event
            this.SendEvent(new BattleUseSkillEvent { 
                _skillData = _skillData, 
                _role = _role, 
                _action = _action, 
                _petId = _petId,
                _skillIndex = _skillIndex
            });
        }

    }
}