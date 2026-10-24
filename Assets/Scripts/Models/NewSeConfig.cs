using System;
using System.Collections.Generic;

namespace SeerGoGame.Models
{
    /// <summary>
    /// 特性配置数据
    /// </summary>
    [Serializable]
    public class NewSeConfig
    {
        public int Idx;           // 特性ID
        public int Stat;           // 状态（固定为1）
        public int Eid;            // 效果ID，对应EffectInfo.json中的id
        public string Args;         // 参数
        public int CanReset;      // 是否可重置
        public string Desc;         // 特性名称
        public string Intro;        // 特性描述
        public int StarLevel;      // 星级
    }

    /// <summary>
    /// 特性配置集合
    /// </summary>
    [Serializable]
    public class NewSeConfigList
    {
        public NewSeConfig[] NewSeIdx;
    }

    /// <summary>
    /// 配置文件外层包装类，用于匹配 {"NewSe": {...}} 结构
    /// </summary>
    [Serializable]
    public class NewSeWrapper
    {
        public NewSeConfigList NewSe;
    }
}
