namespace SeerGoGame.Controllers.Battle
{
    /// <summary>
    /// 战斗状态枚举
    /// </summary>
    public enum BattlePhase
    {
        Idle,           // 空闲，等待玩家操作
        Processing,     // 回合执行中
        Switching,      // 精灵切换中
        Ended           // 战斗结束
    }

    /// <summary>
    /// 切换类型
    /// </summary>
    public enum SwitchType
    {
        None,           // 无切换
        PlayerActive,   // 玩家主动切换
        PlayerDefeat,   // 玩家被击败切换
        EnemyActive,    // 敌方主动切换
        EnemyDefeat     // 敌方被击败切换
    }

    /// <summary>
    /// 击败类型
    /// </summary>
    public enum DefeatType
    {
        None,           // 未击败
        BySkill,        // 技能击败
        ByBuff          // Buff击败
    }

    /// <summary>
    /// 集中管理的战斗状态
    /// </summary>
    public class BattleState
    {
        // === 核心状态 ===
        public BattlePhase Phase { get; set; } = BattlePhase.Idle;
        public bool IsPlayerTurn { get; set; } = true;
        public int RoundCount { get; set; } = 1;

        // === 本回合击败信息 ===
        public DefeatType PlayerDefeatType { get; private set; } = DefeatType.None;
        public DefeatType EnemyDefeatType { get; private set; } = DefeatType.None;

        // === 切换状态 ===
        public SwitchType CurrentSwitchType { get; set; } = SwitchType.None;
        public bool WaitingForPlayerSwitch { get; set; } = false;
        public bool WaitingForEnemySwitch { get; set; } = false;

        // === 精灵索引 ===
        public int CurrentPlayerPetIndex { get; set; } = 0;
        public int CurrentEnemyPetIndex { get; set; } = 0;

        // === 方法 ===
        
        /// <summary>
        /// 标记精灵被击败
        /// </summary>
        public void MarkDefeated(bool isPlayer, DefeatType defeatType)
        {
            if (isPlayer)
                PlayerDefeatType = defeatType;
            else
                EnemyDefeatType = defeatType;
        }

        /// <summary>
        /// 重置本回合击败标记
        /// </summary>
        public void ResetRoundDefeats()
        {
            PlayerDefeatType = DefeatType.None;
            EnemyDefeatType = DefeatType.None;
        }

        /// <summary>
        /// 检查是否双方都被击败
        /// </summary>
        public bool IsBothDefeated()
        {
            return PlayerDefeatType != DefeatType.None && EnemyDefeatType != DefeatType.None;
        }

        /// <summary>
        /// 检查是否有任一方被击败
        /// </summary>
        public bool IsAnyDefeated()
        {
            return PlayerDefeatType != DefeatType.None || EnemyDefeatType != DefeatType.None;
        }

        /// <summary>
        /// 检查对方是否已被击败（用于混合场景判断）
        /// </summary>
        public bool IsOpponentDefeated(bool checkPlayer)
        {
            return checkPlayer ? EnemyDefeatType != DefeatType.None : PlayerDefeatType != DefeatType.None;
        }

        /// <summary>
        /// 是否可以接受玩家输入
        /// </summary>
        public bool CanAcceptInput()
        {
            return Phase == BattlePhase.Idle && IsPlayerTurn;
        }

        /// <summary>
        /// 是否正在切换精灵
        /// </summary>
        public bool IsSwitching()
        {
            return CurrentSwitchType != SwitchType.None;
        }

        /// <summary>
        /// 重置切换状态
        /// </summary>
        public void ResetSwitchState()
        {
            CurrentSwitchType = SwitchType.None;
            WaitingForPlayerSwitch = false;
            WaitingForEnemySwitch = false;
        }

        /// <summary>
        /// 开始回合
        /// </summary>
        public void StartRound()
        {
            Phase = BattlePhase.Processing;
            IsPlayerTurn = false;
            // 开始新回合时重置击败标记
            ResetRoundDefeats();
        }

        /// <summary>
        /// 结束回合
        /// </summary>
        public void EndRound()
        {
            if (Phase == BattlePhase.Ended) return;
            
            Phase = BattlePhase.Idle;
            IsPlayerTurn = true;
            RoundCount++;
            // 注意：不在这里重置击败标记，由调用者控制时机
        }

        /// <summary>
        /// 结束战斗
        /// </summary>
        public void EndBattle()
        {
            Phase = BattlePhase.Ended;
            IsPlayerTurn = false;
        }

        /// <summary>
        /// 获取状态描述（调试用）
        /// </summary>
        public string GetStateDescription()
        {
            return $"[回合{RoundCount}] Phase:{Phase} | " +
                   $"PlayerTurn:{IsPlayerTurn} | " +
                   $"Switch:{CurrentSwitchType} | " +
                   $"PlayerDefeat:{PlayerDefeatType} | " +
                   $"EnemyDefeat:{EnemyDefeatType}";
        }
    }
}
