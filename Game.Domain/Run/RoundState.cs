namespace Game.Domain.Run;

/// <summary>
/// Состояние текущего раунда - Value Object.
/// 
/// Инварианты:
/// - Score >= 0
/// - Goal > 0
/// - HandsPlayed >= 0
/// - HandsDiscarded >= 0
/// 
/// Immutable - все операции возвращают новое состояние.
/// 
/// Почему Value Object:
/// - Неизменяемость исключает невалидные состояния
/// - Явные инварианты проверяются при создании
/// - Безопасно передавать и сравнивать
/// </summary>
public readonly struct RoundState : IEquatable<RoundState>
{
    /// <summary>
    /// Текущий анте
    /// </summary>
    public Ante Ante { get; }
    
    /// <summary>
    /// Текущий раунд внутри анте
    /// </summary>
    public Round Round { get; }
    
    /// <summary>
    /// Накопленные очки в текущем раунде
    /// </summary>
    public int Score { get; }
    
    /// <summary>
    /// Цель по очкам для победы в раунде
    /// </summary>
    public int Goal { get; }
    
    /// <summary>
    /// Количество сыгранных рук в текущем раунде
    /// </summary>
    public int HandsPlayed { get; }
    
    /// <summary>
    /// Количество сброшенных рук в текущем раунде
    /// </summary>
    public int HandsDiscarded { get; }
    
    /// <summary>
    /// Раунд завершен (достигнута цель)
    /// </summary>
    public bool IsComplete => Score >= Goal;
    
    /// <summary>
    /// Осталось очков до цели
    /// </summary>
    public int Remaining => Math.Max(0, Goal - Score);
    
    /// <summary>
    /// Максимальное количество рук для игры за раунд
    /// </summary>
    public const int MaxHandsToPlay = 5;
    
    /// <summary>
    /// Максимальное количество рук для сброса за раунд
    /// </summary>
    public const int MaxHandsToDiscard = 5;
    
    /// <summary>
    /// Можно ли сыграть еще руку (не превышен лимит)
    /// </summary>
    public bool CanPlayHand => HandsPlayed < MaxHandsToPlay;
    
    /// <summary>
    /// Можно ли сбросить еще руку (не превышен лимит)
    /// </summary>
    public bool CanDiscardHand => HandsDiscarded < MaxHandsToDiscard;
    
    /// <summary>
    /// Создает новое состояние раунда.
    /// </summary>
    /// <exception cref="ArgumentException">Если нарушены инварианты</exception>
    public RoundState(Ante ante, Round round, int score, int goal, int handsPlayed = 0, int handsDiscarded = 0)
    {
        if (score < 0)
            throw new ArgumentException("Score cannot be negative", nameof(score));
        if (goal <= 0)
            throw new ArgumentException("Goal must be positive", nameof(goal));
        if (handsPlayed < 0)
            throw new ArgumentException("HandsPlayed cannot be negative", nameof(handsPlayed));
        if (handsDiscarded < 0)
            throw new ArgumentException("HandsDiscarded cannot be negative", nameof(handsDiscarded));
            
        Ante = ante;
        Round = round;
        Score = score;
        Goal = goal;
        HandsPlayed = handsPlayed;
        HandsDiscarded = handsDiscarded;
    }
    
    /// <summary>
    /// Добавить очки к текущему раунду. Возвращает новое состояние (immutable).
    /// </summary>
    /// <param name="points">Очки для добавления (должно быть >= 0)</param>
    /// <exception cref="ArgumentException">Если points < 0</exception>
    public RoundState AddScore(int points)
    {
        if (points < 0)
            throw new ArgumentException("Points cannot be negative", nameof(points));
            
        return new RoundState(Ante, Round, Score + points, Goal, HandsPlayed, HandsDiscarded);
    }
    
    /// <summary>
    /// Увеличить счетчик сыгранных рук. Возвращает новое состояние.
    /// </summary>
    /// <exception cref="InvalidOperationException">Если достигнут лимит рук для игры</exception>
    public RoundState IncrementHandsPlayed()
    {
        if (!CanPlayHand)
            throw new InvalidOperationException($"Cannot play more than {MaxHandsToPlay} hands per round");
            
        return new RoundState(Ante, Round, Score, Goal, HandsPlayed + 1, HandsDiscarded);
    }
    
    /// <summary>
    /// Увеличить счетчик сброшенных рук. Возвращает новое состояние.
    /// </summary>
    /// <exception cref="InvalidOperationException">Если достигнут лимит рук для сброса</exception>
    public RoundState IncrementHandsDiscarded()
    {
        if (!CanDiscardHand)
            throw new InvalidOperationException($"Cannot discard more than {MaxHandsToDiscard} hands per round");
            
        return new RoundState(Ante, Round, Score, Goal, HandsPlayed, HandsDiscarded + 1);
    }
    
    /// <summary>
    /// Переход к следующему раунду. Сбрасывает очки и счетчики, вычисляет новую цель.
    /// </summary>
    public RoundState AdvanceToNextRound()
    {
        var nextRound = Round.Next();
        var newGoal = BlindGoalCalculator.Calculate(Ante, nextRound);
        return new RoundState(Ante, nextRound, 0, newGoal, 0, 0);
    }
    
    /// <summary>
    /// Переход к следующему анте. Сбрасывает раунд на 1, вычисляет новую цель.
    /// </summary>
    public RoundState AdvanceToNextAnte()
    {
        var nextAnte = Ante.Next();
        var firstRound = new Round(1);
        var newGoal = BlindGoalCalculator.Calculate(nextAnte, firstRound);
        return new RoundState(nextAnte, firstRound, 0, newGoal, 0, 0);
    }
    
    public bool Equals(RoundState other) =>
        Ante.Equals(other.Ante) &&
        Round.Equals(other.Round) &&
        Score == other.Score &&
        Goal == other.Goal &&
        HandsPlayed == other.HandsPlayed &&
        HandsDiscarded == other.HandsDiscarded;
    
    public override bool Equals(object? obj) => obj is RoundState other && Equals(other);
    
    public override int GetHashCode() =>
        HashCode.Combine(Ante, Round, Score, Goal, HandsPlayed, HandsDiscarded);
    
    public static bool operator ==(RoundState left, RoundState right) => left.Equals(right);
    
    public static bool operator !=(RoundState left, RoundState right) => !left.Equals(right);
    
    public override string ToString() =>
        $"{Ante} - {Round}: {Score}/{Goal} (Played: {HandsPlayed}/{MaxHandsToPlay}, Discarded: {HandsDiscarded}/{MaxHandsToDiscard})";
}

