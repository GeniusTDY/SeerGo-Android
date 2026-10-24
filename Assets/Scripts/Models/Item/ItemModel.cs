using QFramework;
using SeerGoGame.Helper;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

namespace SeerGoGame.Models
{
    /// <summary>
    /// 道具配置数据
    /// </summary>
    public class ItemConfig
    {
        public int id;                      // 道具ID
        public string name;                 // 道具名称
        public string description;          // 道具描述
        public string type;                 // 道具类型（exp/heal/buff/cure/revive等，保留用于显示分组）
        public float effectValue;             // 效果值（如恢复的HP量、增加的经验值）
        public string effectArgs;           // 效果参数（用于复杂效果，如buff ID）
        public int price;                   // 售价
        public string useScope;             // 使用范围（INBATTLE/OUTBATTLE/INBATTLE|OUTBATTLE）
        public string icon;                 // 图标路径
        public string purchaseMethod = "SEER_BEAN";       // 购买方式 SEER_BEAN GOLD_BEAN

        // ===== 新版 SideEffect 效果系统（参照技能副作用模式）=====
        /// <summary>
        /// 道具副作用ID列表（空格分隔，如 "102" 或 "102 103"）
        /// </summary>
        public string sideEffect;

        /// <summary>
        /// 道具副作用参数（空格分隔，如 "20" 或 "SPATK 255"）
        /// 支持多个副作用共用参数列表，按 argsNum 顺序切分
        /// </summary>
        public string sideEffectArg;

        /// <summary>
        /// 解析 SideEffect 字符串为 ID 列表
        /// </summary>
        public List<int> GetSideEffectIds()
        {
            var ids = new List<int>();
            if (!string.IsNullOrEmpty(sideEffect))
            {
                string[] idStrings = sideEffect.Split(' ');
                foreach (string idStr in idStrings)
                {
                    if (int.TryParse(idStr.Trim(), out int id))
                    {
                        ids.Add(id);
                    }
                }
            }
            return ids;
        }
    }

    /// <summary>
    /// 道具模型接口
    /// </summary>
    public interface IItemModel : IModel
    {
        Dictionary<int, ItemConfig> ItemConfigs { get; }

        /// <summary>
        /// 道具副作用元数据（ID → EffectInfo），记录每个 SideEffect 需要的参数个数等信息
        /// 配置来源：Config/ItemEffectInfo.json
        /// </summary>
        Dictionary<int, EffectInfo> ItemEffectInfos { get; }

        ItemConfig GetById(int id);
        List<ItemConfig> GetByType(string type);
        bool IsContainScope(ItemConfig item, string scope);
        ItemConfig GetByName(string name);
    }

    /// <summary>
    /// 道具模型实现
    /// </summary>
    public class ItemModel : AbstractModel, IItemModel
    {
        public Dictionary<int, ItemConfig> ItemConfigs { get; private set; } = new Dictionary<int, ItemConfig>();

        /// <summary>
        /// 道具副作用元数据字典：SideEffect ID → argsNum/info
        /// </summary>
        public Dictionary<int, EffectInfo> ItemEffectInfos { get; private set; } = new Dictionary<int, EffectInfo>();

        protected override void OnInit()
        {
            LoadItemConfigs();
            LoadItemEffectInfos();
        }

        /// <summary>
        /// 加载道具副作用元数据配置（ItemEffectInfo.json）
        /// 参照技能 EffectInfo 加载方式
        /// </summary>
        private void LoadItemEffectInfos()
        {
            var config = ConfigHelper.LoadJsonConfig<EffectInfoRoot>("Config/ItemEffectInfo");
            if (config != null && config.root?.Effect != null)
            {
                foreach (var effect in config.root.Effect)
                {
                    ItemEffectInfos[effect.id] = effect;
                }
                SeerGoLogger.Log($"<color=cyan>[ItemModel] 成功加载 {ItemEffectInfos.Count} 个道具副作用元数据</color>");
            }
            else
            {
                SeerGoLogger.LogWarning("<color=yellow>[ItemModel] 未找到道具副作用元数据配置 ItemEffectInfo.json</color>");
            }
        }

        private void LoadItemConfigs()
        {
            // 从XML加载道具配置
            var xmlPath = Resources.Load<TextAsset>("Config/item");

            try
            {
                var doc = XDocument.Parse(xmlPath.text);
                var items = doc.Descendants("item");

                foreach (var itemElement in items)
                {
                    var config = new ItemConfig
                    {
                        id = int.Parse(itemElement.Attribute("id")?.Value ?? "0"),
                        name = itemElement.Attribute("name")?.Value ?? "",
                        description = itemElement.Attribute("description")?.Value ?? "",
                        type = itemElement.Attribute("type")?.Value ?? "",
                        effectValue = float.Parse(itemElement.Attribute("effectValue")?.Value ?? "0"),
                        effectArgs = itemElement.Attribute("effectArgs")?.Value ?? "",
                        price = int.Parse(itemElement.Attribute("price")?.Value ?? "0"),
                        useScope = itemElement.Attribute("useScope")?.Value ?? "",
                        icon = itemElement.Attribute("icon")?.Value ?? "",
                        purchaseMethod = itemElement.Attribute("purchaseMethod")?.Value ?? "",
                        // 新 SideEffect 字段（按需读取，未配置时为空）
                        sideEffect = itemElement.Attribute("sideEffect")?.Value ?? "",
                        sideEffectArg = itemElement.Attribute("sideEffectArg")?.Value ?? ""
                    };

                    ItemConfigs[config.id] = config;
                }
                SeerGoLogger.Log($"<color=cyan>[ItemModel] 成功加载 {ItemConfigs.Count} 个道具配置</color>");
            }
            catch (System.Exception e)
            {
                SeerGoLogger.LogError($"<color=red>[ItemModel] 加载道具配置失败: {e.Message}</color>");
            }
        }

        /// <summary>
        /// 根据ID获取道具配置
        /// </summary>
        public ItemConfig GetById(int id)
        {
            if (ItemConfigs.ContainsKey(id))
                return ItemConfigs[id];
            return null;
        }

        /// <summary>
        /// 根据类型获取道具配置列表
        /// </summary>
        public List<ItemConfig> GetByType(string type)
        {
            return ItemConfigs.Values
                .Where(item => item.type == type)
                .ToList();
        }

        /// <summary>
        /// 判断道具是否包含指定使用范围（精确匹配）
        /// </summary>
        public bool IsContainScope(ItemConfig item, string scope)
        {
            if (item == null || string.IsNullOrEmpty(item.useScope) || string.IsNullOrEmpty(scope))
                return false;

            // 将useScope按'|'分割，然后检查是否有完全匹配的项
            var scopes = item.useScope.Split('|')
                .Select(s => s.Trim().ToUpper())
                .ToArray();

            return scopes.Contains(scope.Trim().ToUpper());
        }

        /// <summary>
        /// 根据名称获取道具配置（精确匹配）
        /// </summary>
        public ItemConfig GetByName(string name)
        {
            return ItemConfigs.Values
                .FirstOrDefault(item => item.name == name);
        }
    }
}