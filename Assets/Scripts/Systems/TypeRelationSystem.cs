using QFramework;
using SeerGoGame.Interfaces;
// 类型关系系统实现
namespace SeerGoGame.Systems
{ 
    public class TypeRelationSystem : AbstractSystem, ITypeRelationSystem
    {
        private ITypeRelationModel _typeRelationModel;

        protected override void OnInit()
        {
            _typeRelationModel = this.GetModel<ITypeRelationModel>();
        }

        // 获取伤害倍数
        public float GetDamageMultiplier(string attackerType, string defenderType)
        {
            return _typeRelationModel.GetDamageMultiplier(attackerType, defenderType);
        }
    }

}