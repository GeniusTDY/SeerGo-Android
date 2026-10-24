using SeerGoGame.Buffs;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

/// <summary>
/// 技能副作用管理器 - 统一管理所有技能副作用处理器
/// </summary>
public class SkillSideEffectManager
{
    private Dictionary<int, ISkillSideEffectHandler> handlers = new Dictionary<int, ISkillSideEffectHandler>();

    public SkillSideEffectManager()
    {
        // 注册所有技能副作用处理器
        RegisterHandler(new SideEffect_1());                  // ID=1: 吸取
        RegisterHandler(new SideEffect_2());                  // ID=2: 体力低于一半威力加倍
        RegisterHandler(new SideEffect_3());                  // ID=3: 解除debuff
        RegisterHandler(new StatChangeSideEffect(4));         // ID=4: 改变自身属性
        RegisterHandler(new StatChangeSideEffect(5));         // ID=5: 改变对手属性
        RegisterHandler(new SideEffect_6());                  // ID=6: 反弹伤害
        RegisterHandler(new SideEffect_7());                  // ID=7: 愤怒
        RegisterHandler(new SideEffect_8());                  // ID=8: 保留
        RegisterHandler(new SideEffect_9());                  // ID=9: 连击增强
        RegisterHandler(new SideEffect_10());                 // ID=10: 麻痹
        RegisterHandler(new StatusConditionSideEffect(11));   // ID=11: 中毒
        RegisterHandler(new StatusConditionSideEffect(12));   // ID=12: 烧伤
        RegisterHandler(new SideEffect_13());                 // ID=13: 寄生
        RegisterHandler(new StatusConditionSideEffect(14));   // ID=14: 冻伤
        RegisterHandler(new SideEffect_15());                 // ID=15: 害怕
        RegisterHandler(new SideEffect_16());                 // ID=16: 睡眠
        RegisterHandler(new SideEffect_20());                 // ID=20: 令自身疲惫
        RegisterHandler(new SideEffect_21());                 // ID=21: 作用{0}~{1}回合, 每回合反弹对手1/{2}伤害
        RegisterHandler(new SideEffect_22());                 // ID=22: 令对方疲惫
        RegisterHandler(new SideEffect_28());                 // ID=28: 降低对方1/{0}的hp
        RegisterHandler(new SideEffect_29());                 // ID=28: 额外增加{0}点固定伤害"
        RegisterHandler(new SideEffect_30());                 // ID=30: 报复
        RegisterHandler(new SideEffect_31());                 // ID=31: 1回合做{0}~{1}次攻击
        RegisterHandler(new SideEffect_32());                 // ID=32: 使用后{0}回合攻击击中对象要害概率增加1/16
        RegisterHandler(new SideEffect_33());                 // ID=33: 消除对手能力提升状态
        RegisterHandler(new SideEffect_34());                 // ID=34: 双倍奉还
        RegisterHandler(new SideEffect_35());                 // ID=35: 惩罚
        RegisterHandler(new SideEffect_36());                 // ID=36: 命中时{0}%的概率秒杀对方
        RegisterHandler(new SideEffect_37());                 // ID=37: 自身HP小于1/{0}时威力为{1}倍
        RegisterHandler(new SideEffect_38());                 // ID=38: 降低对方{0}点战斗时的最高HP
        RegisterHandler(new SideEffect_39());                 // ID=39: PP封印
        RegisterHandler(new SideEffect_40());                 // ID=40: 电光火石
        RegisterHandler(new SideEffect_41());                 // ID=41: {0}~{1}回合本方受到的火系攻击伤害减半
        RegisterHandler(new SideEffect_42());                 // ID=42: {0}~{1}回合自己使用电招式伤害×2
        RegisterHandler(new SideEffect_43());                 // ID=43: {恢复自身最大体力的1/{0}
        RegisterHandler(new SideEffect_44());                 // ID=44: {0}回合自己受到特殊攻击伤害减半
        RegisterHandler(new SideEffect_45());                 // ID=45: {0}回合自身的防御力和对手相同
        RegisterHandler(new SideEffect_46());                 // ID=46: 抵挡
        RegisterHandler(new SideEffect_47());                 // ID=47: 能力防护
        RegisterHandler(new SideEffect_48());                 // ID=48: 异常免疫
        RegisterHandler(new SideEffect_49());                 // ID=49: 可以抵挡{0}点伤害"
        RegisterHandler(new SideEffect_50());                 // ID=50: {0}回合自己受到物理攻击伤害减半
        RegisterHandler(new SideEffect_51());                 // ID=51: {0}回合自身的攻击力和对手相同
        RegisterHandler(new SideEffect_52());                 // ID=52: 先制失效
        RegisterHandler(new SideEffect_53());                 // ID=53: {0}回合攻击伤害是正常状态下的{1}倍"
        RegisterHandler(new SideEffect_54());                 // ID=54: {0}回合使对方攻击伤害是正常状态下的1/{1}倍
        RegisterHandler(new SideEffect_55());                 // ID=55: 属性反转
        RegisterHandler(new SideEffect_56());                 // ID=56: 属性同化
        RegisterHandler(new SideEffect_57());                 // ID=57: {0}回合内每回合使用技能恢复自身最大体力的1/{1}
        RegisterHandler(new SideEffect_58());                 // ID=58: 必定暴击
        RegisterHandler(new SideEffect_59());                 // ID=59: 遗愿
        RegisterHandler(new SideEffect_60());                 // ID=60: {0}回合内，每回合都能附加{1}点固定伤害
        RegisterHandler(new SideEffect_61());                 // ID=61: 随机威力
        RegisterHandler(new SideEffect_62());                 // ID=62: 延时反杀
        RegisterHandler(new SideEffect_63());                 // ID=63: 将能力下降状态反馈给对手
        RegisterHandler(new SideEffect_64());                 // ID=64: 自身在烧伤、冻伤、中毒状态下造成的伤害反而加倍
        RegisterHandler(new SideEffect_65());                 // ID=65: {0}回合内{1}系技能威力为{2}倍
        RegisterHandler(new SideEffect_66());                 // ID=66: 当次攻击击败对方出战精灵时恢复自身最大体力的1/{0}
        RegisterHandler(new SideEffect_67());                 // ID=67: 当次攻击击败对方时减少对方下次出战精灵的最大体力1/{0}
        RegisterHandler(new SideEffect_68());                 // ID=68: {0}回合内，受到致死攻击时则余下1点体力
        RegisterHandler(new SideEffect_69());                 // ID=69: 下{0}回合对手使用体力药剂时效果变成减少相应的体力
        RegisterHandler(new SideEffect_70());                 // ID=70: 威力随机，随机范围150~220
        RegisterHandler(new SideEffect_71());                 // ID=71: 牺牲暴击
        RegisterHandler(new SideEffect_72());                 // ID=72: 若没有命中对手则自己失去全部体力
        RegisterHandler(new SideEffect_73());                 // ID=73: 若先手攻击，则将所受伤害2倍反击给对手
        RegisterHandler(new SideEffect_74());                 // ID=74: 命中时有30%几率令对手中毒、烧伤或冻伤
        RegisterHandler(new SideEffect_75());                 // ID=75: 命中时有30%几率令对手麻痹、睡眠或害怕
        RegisterHandler(new SideEffect_76());                 // ID=76:{0}%几率令对手在{1}回合内，每回合受到{2}点固定伤害"
        RegisterHandler(new SideEffect_77());                 // ID=77:{0}回合内若使用技能则恢复{1}点体力值
        RegisterHandler(new SideEffect_78());                 // ID=78:{0}回合内，物理攻击对自身必定miss
        RegisterHandler(new SideEffect_79());                 // ID=79:损失1/2的体力，提升自身的能力
        RegisterHandler(new SideEffect_80());                 // ID=80:损失1/2的体力，给予对手同等的伤害
        RegisterHandler(new SideEffect_81());                 // ID=81:下{0}回合自身攻击技能必定命中
        RegisterHandler(new SideEffect_82());                 // ID=82:目标为雄性，伤害为200%；目标为雌性，伤害为50%
        RegisterHandler(new SideEffect_83());                 // ID=83:自身雄性，下两回合必定先手；自身雌性，下两回合必定致命一击
        RegisterHandler(new SideEffect_84());                 // ID=84:{0}回合内受到物理攻击时有{1}%几率将对手麻痹
        RegisterHandler(new SideEffect_85());                 // ID=85:使对手的能力提升效果转化到自己身上
        RegisterHandler(new SideEffect_86());                 // ID=86:{0}回合内，属性攻击对自身必定miss
        RegisterHandler(new SideEffect_87());                 // ID=87:恢复自身所有PP值
        RegisterHandler(new SideEffect_88());                 // ID=88:{0}%几率伤害为{1}倍
        RegisterHandler(new SideEffect_89());                 // ID=89:{0}回合内，每次造成伤害的1/{1}会恢复自己的HP
        RegisterHandler(new SideEffect_90());                 // ID=90:{0}回合内，自身造成的伤害为{1}倍
        RegisterHandler(new SideEffect_91());                 // ID=91:{0}回合内，对手的状态变化会同时作用在自己身上
        RegisterHandler(new SideEffect_92());                 // ID=92:{0}回合内，受到物理攻击时，有{1}%几率将对手冻伤
        RegisterHandler(new SideEffect_93());                 // ID=93:{0}%几率额外附加{1}点固定伤害
        RegisterHandler(new SideEffect_94());                 // ID=94:命中后{0}%令对手石化
        RegisterHandler(new SideEffect_95());                 // ID=95:对手处于睡眠状态时，致命一击率提升{0}%
        RegisterHandler(new SideEffect_96());                 // ID=96:对手处于烧伤状态时，威力翻倍
        RegisterHandler(new SideEffect_97());                 // ID=97:对手处于冻伤状态时，威力翻倍
        RegisterHandler(new SideEffect_98());                 // ID=98:{0}回合内，对雄性精灵的伤害为{1}倍
        RegisterHandler(new SideEffect_99());                 // ID=99:命中时，{0}%几率令对手混乱
        RegisterHandler(new SideEffect_100());                // ID=100:自身体力越少则威力越大
        RegisterHandler(new SideEffect_101());                 // ID=101:给对手造成伤害时，伤害数值的{0}%恢复自身体力
        RegisterHandler(new SideEffect_102());                 // ID=102:对手处于麻痹状态时，威力翻倍
        RegisterHandler(new SideEffect_103());                 // ID=103:{0}%几率令对手增加一层衰弱
        RegisterHandler(new SideEffect_104());                 // ID=104:{0}回合内，每次直接攻击都有{1}%几率附带衰弱效果
        RegisterHandler(new SideEffect_105());                 // ID=105:给予对象损伤的1/{0}，会回复自己的HP
        RegisterHandler(new SideEffect_106());                 // ID=106:{0}回合内，特殊攻击对自身必定miss
        RegisterHandler(new SideEffect_107());                 // ID=107:若本次攻击造成的伤害小于{0}，则自身{1}提升1个等级
        RegisterHandler(new SideEffect_108());                 // ID=108:{0}回合内受到物理攻击时，有{1}%几率将对手烧伤
        RegisterHandler(new SideEffect_109());                 // ID=109:{0}回合内，给对手造成伤害时，有{1}%几率令对手冻伤
        RegisterHandler(new SideEffect_110());                 // ID=110:{0}回合内，每次躲避攻击都有{1}%几率使自身{2}提升1个等级
        RegisterHandler(new SideEffect_111());                 // ID=111:附加额外伤害，自身等级越高，附加的伤害越高
        RegisterHandler(new SideEffect_112());                 // ID=112:牺牲全部体力给对手造成250~300点伤害
        RegisterHandler(new SideEffect_113());                 // ID=113:个体越高，威力越大
        RegisterHandler(new SideEffect_114());                 // ID=114:命中后有{0}%几率令对方易燃
        RegisterHandler(new SideEffect_115());                 // ID=115:{0}%几率附加速度的1/{1}伤害
        RegisterHandler(new SideEffect_116());                 // ID=116:3回合内，如果先出手，伤害的20%恢复体力
        RegisterHandler(new SideEffect_117());                 // ID=117:5回合内，如果先出手，50%几率对手害怕1到3回合
        RegisterHandler(new SideEffect_118());                 // ID=118:威力随机，随机范围150~220
        RegisterHandler(new SideEffect_119());                 // ID=119:若伤害为奇数，30%对手疲惫1回合；若为偶数，30%速度+1
        RegisterHandler(new SideEffect_120());                 // ID=120:50%几率对方减血1/{0}，50%几率自己减血1/{0}
        RegisterHandler(new SideEffect_121());                 // ID=121:属性相同时，{0}%几率让对方麻痹
        RegisterHandler(new SideEffect_122());                 // ID=122:先出手时，{1}%改变对方{0}等级{2}
        RegisterHandler(new SideEffect_123());                 // ID=123:{0}回合内，受到任何伤害，自身{1}提高{2}个等级
        RegisterHandler(new SideEffect_124());                 // ID=124:命中后，{0}%几率随机对方一个属性{1}个等级
        RegisterHandler(new SideEffect_125());                 // ID=125:{0}回合内，每回合被攻击所受到的伤害不超过{1}
        RegisterHandler(new SideEffect_126());                 // ID=126:{0}回合内，每回合自身攻击和速度提高{1}个等级
        RegisterHandler(new SideEffect_128());                 // ID=128:{0}回合内将受到的攻击伤害转化为自身体力
        RegisterHandler(new SideEffect_129());                 // ID=129:对方为{0}则技能威力翻倍
        RegisterHandler(new SideEffect_130());                 // ID=130:对方为{0}则附加{1}点伤害
        RegisterHandler(new SideEffect_132());                 // ID=132:若当前HP低于对手，则技能威力翻倍
        RegisterHandler(new SideEffect_143());                 // ID=143: 使对手的能力提升效果反转成能力下降效果
        RegisterHandler(new SideEffect_180());                 // ID=180:消除对手回合类效果
        RegisterHandler(new SideEffect_195());                 // ID=195: 无视对手双防能力提升状态
        RegisterHandler(new SideEffect_201());                 // ID=201: 对选中对象或本方全体恢复1/{1}的体力
        RegisterHandler(new SideEffect_422());                 // ID=422:附加所造成伤害值{0}%的固定伤害
        RegisterHandler(new SideEffect_438());                 // ID=438: {0}%的几率恢复自身体力的1/{1}
        RegisterHandler(new SideEffect_439());                 // ID=439: {0}回合内若自身处于能力下降或异常状态，则对手每回合受到{1}点固定伤害
        RegisterHandler(new SideEffect_451());                 // ID=451: 命中后{0}%随机令对手进入烧伤，冻伤，中毒中的{1}种
        RegisterHandler(new SideEffect_455());                 // ID=455: 每损失{0}点体力则额外附加{1}点固定伤害
        RegisterHandler(new SideEffect_463());                 // ID=463: {0}回合内每回合所受的伤害减少{1}点
        RegisterHandler(new SideEffect_478());                 // ID=478: {0}回合内令对手使用的属性技能无效
        RegisterHandler(new SideEffect_508());                 // ID=508: 减少{0}点下回合所受的伤害
        RegisterHandler(new SideEffect_511());                 // ID=511: {0}%概率威力翻倍，体力低于1/{1}时概率增加{2}%
        RegisterHandler(new SideEffect_545());                 // ID=545: {0}回合内若受到伤害高于{1}则对手{2}
        RegisterHandler(new SideEffect_609());                 // ID=609: 若对手{0}，技能威力翻倍麻痹0，中毒1，烧伤2，冻伤5，害怕6，疲惫7，睡眠8，石化9，冰封15）
        RegisterHandler(new SideEffect_620());                 // ID=620: {0}回合内致命一击率上升{1}/16
        RegisterHandler(new SideEffect_1044());                // ID=1044: 吸取对手能力提升状态，吸取成功则下{0}回合造成的伤害翻倍
        RegisterHandler(new SideEffect_10001());                // ID=10001: 个体越高，威力越大  卡鲁耶克“异界幻象”、宿主岩“本能一击”
        RegisterHandler(new SideEffect_10002());                // ID=10002: 技能威力=自身体力/2+个体值
        RegisterHandler(new SideEffect_10003());                // ID=10003: 技能威力=【精灵个体值* 5*精灵等级÷100】

    }

    public void RegisterHandler(ISkillSideEffectHandler handler)
    {
        handlers[handler.EffectId] = handler;
        //SeerGoLogger.Log($"[SkillSideEffectManager] 注册副作用处理器: ID={handler.EffectId} ({handler.GetType().Name})");
    }

    /// <summary>
    /// 处理技能副作用
    /// </summary>
    public void OnAfterCastSkill(int effectId, IBuffable caster, IBuffable target,
                                  SkillData skill, string[] args, EffectInfo effectInfo,
                                  BuffSystem buffSystem)
    {
        if (handlers.TryGetValue(effectId, out var handler))
        {
            SeerGoLogger.Log($"[副作用处理] 执行副作用 ID={effectId}");
            handler.OnAfterCastSkill(caster, target, skill, args, effectInfo, buffSystem);
        }
        else
        {
            SeerGoLogger.LogWarning($"[副作用处理] 未找到副作用ID={effectId}的处理器");
        }
    }


    /// <summary>
    /// 处理技能伤害计算前的效果
    /// </summary>
    public void OnBeforeDamageCalculation(int effectId, IBuffable caster, IBuffable target,
                                  SkillData skill, string[] args, EffectInfo effectInfo,
                                  BuffSystem buffSystem)
    {
        if (handlers.TryGetValue(effectId, out var handler))
        {
            SeerGoLogger.Log($"[处理技能伤害计算前的效果] 执行副作用 ID={effectId}");
            handler.OnBeforeDamageCalculation(caster, target, skill, args, effectInfo, buffSystem);
        }
        else
        {
            SeerGoLogger.LogWarning($"[处理技能伤害计算前的效果] 未找到副作用ID={effectId}的处理器");
        }
    }


    /// <summary>
    /// 处理技能伤害计算后的效果
    /// </summary>
    public void OnAfterDamageCalculation(int effectId, IBuffable caster, IBuffable target,
                                  SkillData skill, string[] args, EffectInfo effectInfo,
                                  BuffSystem buffSystem)
    {
        if (handlers.TryGetValue(effectId, out var handler))
        {
            SeerGoLogger.Log($"[处理技能伤害计算后的效果] 执行副作用 ID={effectId}");
            handler.OnAfterDamageCalculation(caster, target, skill, args, effectInfo, buffSystem);
        }
        else
        {
            SeerGoLogger.LogWarning($"[处理技能伤害计算后的效果] 未找到副作用ID={effectId}的处理器");
        }
    }

    /// <summary>
    /// 处理技能伤害应用前的效果
    /// </summary>
    public void OnBeforeApplyDamage(int effectId, IBuffable caster, IBuffable target,
                                  SkillData skill, string[] args, EffectInfo effectInfo,
                                  BuffSystem buffSystem)
    {
        if (handlers.TryGetValue(effectId, out var handler))
        {
            SeerGoLogger.Log($"[处理技能伤害应用前的效果] 执行副作用 ID={effectId}");
            handler.OnBeforeApplyDamage(caster, target, skill, args, effectInfo, buffSystem);
        }
        else
        {
            SeerGoLogger.LogWarning($"[处理技能伤害应用前的效果] 未找到副作用ID={effectId}的处理器");
        }
    }


    /// <summary>
    /// 处理技能伤害应用后的效果
    /// </summary>
    public void OnAfterApplyDamage(int effectId, IBuffable caster, IBuffable target,
                                  SkillData skill, string[] args, EffectInfo effectInfo,
                                  BuffSystem buffSystem)
    {
        if (handlers.TryGetValue(effectId, out var handler))
        {
            SeerGoLogger.Log($"[处理技能伤害应用后的效果] 执行副作用 ID={effectId}");
            handler.OnAfterApplyDamage(caster, target, skill, args, effectInfo, buffSystem);
        }
        else
        {
            SeerGoLogger.LogWarning($"[处理技能伤害应用后的效果] 未找到副作用ID={effectId}的处理器");
        }
    }

    /// <summary>
    /// 回合结束buff处理后
    /// </summary>
    public void OnAfterBuffTurnEnd(int effectId, IBuffable caster, IBuffable target,
                                  SkillData skill, string[] args, EffectInfo effectInfo,
                                  BuffSystem buffSystem)
    {
        if (handlers.TryGetValue(effectId, out var handler))
        {
            SeerGoLogger.Log($"[处理回合结束buff处理后的效果] 执行副作用 ID={effectId}");
            handler.OnAfterBuffTurnEnd(caster, target, skill, args, effectInfo, buffSystem);
        }
        else
        {
            SeerGoLogger.LogWarning($"[处理回合结束buff处理后的效果] 未找到副作用ID={effectId}的处理器");
        }
    }
    /// <summary>
    /// 技能未命中处理
    /// </summary>
    public void OnSkillNotHit(int effectId, IBuffable caster, IBuffable target,
                                  SkillData skill, string[] args, EffectInfo effectInfo,
                                  BuffSystem buffSystem)
    {
        if (handlers.TryGetValue(effectId, out var handler))
        {
            SeerGoLogger.Log($"[处理技能未命中的效果] 执行副作用 ID={effectId}");
            handler.OnSkillNotHit(caster, target, skill, args, effectInfo, buffSystem);
        }
        else
        {
            SeerGoLogger.LogWarning($"[处理技能未命中的效果] 未找到副作用ID={effectId}的处理器");
        }
    }

    public void UnRegisterHandler()
    {
        handlers.Clear();
    }

}
