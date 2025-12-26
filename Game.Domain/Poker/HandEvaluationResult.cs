namespace Game.Domain.Poker;

/// <summary>
/// Результат оценки руки - тип комбинации и базовые очки.
/// </summary>
public sealed class HandEvaluationResult
{
    public PokerHandType HandType { get; }
    public int BaseScore { get; }

    public HandEvaluationResult(PokerHandType handType, int baseScore)
    {
        HandType = handType;
        BaseScore = baseScore;
    }
}
