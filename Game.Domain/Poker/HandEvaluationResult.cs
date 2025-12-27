namespace Game.Domain.Poker;

/// <summary>
/// Результат оценки руки - тип комбинации, базовые очки комбинации и множитель.
/// 
/// В Balatro формула подсчета очков:
/// Итоговые очки = (Сумма очков карт + Базовые очки комбинации) * Множитель
/// </summary>
public sealed class HandEvaluationResult
{
    /// <summary>
    /// Тип покерной комбинации
    /// </summary>
    public PokerHandType HandType { get; }
    
    /// <summary>
    /// Базовые очки комбинации (добавляются к сумме очков карт)
    /// </summary>
    public int BaseScore { get; }
    
    /// <summary>
    /// Множитель комбинации (применяется к сумме очков карт + базовые очки)
    /// </summary>
    public int Multiplier { get; }

    public HandEvaluationResult(PokerHandType handType, int baseScore, int multiplier)
    {
        HandType = handType;
        BaseScore = baseScore;
        Multiplier = multiplier;
    }
}
