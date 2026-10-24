using QFramework;
using SeerGoGame.Interfaces;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SeerGoGame.Models
{
    // 类型关系模型实现
    public class TypeRelationModel : AbstractModel, ITypeRelationModel
    {
        public Dictionary<string, Dictionary<string, float>> TypeRelations { get; private set; }

        protected override void OnInit()
        {
            TypeRelations = new Dictionary<string, Dictionary<string, float>>();
        }

        public async Task LoaTypeRelationConfigAsync()
        {
            string path = ResourcePathBuilder.GetPetsInfoDataPath();
            string fullPath = $"{path}/typesRelation";

            var result = await JsonResourceLoadManager.LoadConfigAsync<TypeRelationRoot>(fullPath);

            if (result.Success && result.Data?.Root != null)
            {
                foreach (var relation in result.Data.Root.Relation)
                {
                    string attackerType = relation.type;
                    if (!TypeRelations.ContainsKey(attackerType))
                    {
                        TypeRelations[attackerType] = new Dictionary<string, float>();
                    }

                    foreach (var opponent in relation.Opponent)
                    {
                        TypeRelations[attackerType][opponent.type] = opponent.multiple;
                    }
                }

            }
        }



        // 获取伤害倍数
        public float GetDamageMultiplier(string attackerType, string defenderType)
        {
            if (TypeRelations.TryGetValue(attackerType, out var defenderRelations))
            {
                if (defenderRelations.TryGetValue(defenderType, out var multiplier))
                {
                      return multiplier;
                }
            }

            // 默认返回1.0（无加成）
            return 1.0f;
        }
    }
}