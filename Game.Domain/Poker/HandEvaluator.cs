using Game.Domain.Core;
using Game.Domain.ECS;
using Game.Domain.ECS.Components;

namespace Game.Domain.Poker;

/// <summary>
/// Оценщик покерных комбинаций - чистая функция.
/// 
/// Почему чистая функция, а не система?
/// - Оценка руки - это вычисление, а не изменение состояния
/// - Можно вызывать из разных систем
/// - Легко тестировать
/// 
/// Почему принимает World и Entity, а не Hand?
/// - В ECS мы работаем с Entity и компонентами
/// - HandComponent содержит список Entity карт
/// - Читаем компоненты карт через World
/// </summary>
public static class HandEvaluator
{
    /// <summary>
    /// Оценивает руку и возвращает тип комбинации и базовые очки.
    /// Если есть выбранные карты - оценивает их, иначе оценивает все карты в руке.
    /// </summary>
    public static HandEvaluationResult Evaluate(World world, Entity handEntity)
    {
        // Проверяем, есть ли выбранные карты
        var selected = world.GetComponent<SelectedCardsComponent>(handEntity);
        List<Entity> cardsToEvaluate;

        if (selected.HasValue && selected.Value.SelectedCards.Count > 0)
        {
            // Оцениваем выбранные карты
            cardsToEvaluate = selected.Value.SelectedCards;
        }
        else
        {
            // Оцениваем все карты в руке
            var hand = world.GetComponent<HandComponent>(handEntity);
            if (!hand.HasValue || hand.Value.Cards.Count == 0)
            {
                return new HandEvaluationResult(PokerHandType.HighCard, 5, 1);
            }
            cardsToEvaluate = hand.Value.Cards;
        }

        if (cardsToEvaluate.Count == 0)
        {
            return new HandEvaluationResult(PokerHandType.HighCard, 5, 1);
        }

        var cardData = GetCardData(world, cardsToEvaluate);

        // Проверяем комбинации от старшей к младшей
        // Правила Balatro: (базовые очки комбинации, множитель)
        if (IsFlushRoyal(cardData))
            return new HandEvaluationResult(PokerHandType.FlushRoyal, 100, 8);
        
        if (IsStraightFlush(cardData))
            return new HandEvaluationResult(PokerHandType.StraightFlush, 100, 8);
        
        if (IsFourOfAKind(cardData))
            return new HandEvaluationResult(PokerHandType.FourOfAKind, 60, 7);
        
        if (IsFullHouse(cardData))
            return new HandEvaluationResult(PokerHandType.FullHouse, 40, 4);
        
        if (IsFlush(cardData))
            return new HandEvaluationResult(PokerHandType.Flush, 35, 4);
        
        if (IsStraight(cardData))
            return new HandEvaluationResult(PokerHandType.Straight, 30, 4);
        
        if (IsThreeOfAKind(cardData))
            return new HandEvaluationResult(PokerHandType.ThreeOfAKind, 30, 3);
        
        if (IsTwoPair(cardData))
            return new HandEvaluationResult(PokerHandType.TwoPair, 20, 2);
        
        if (IsPair(cardData))
            return new HandEvaluationResult(PokerHandType.Pair, 10, 2);

        return new HandEvaluationResult(PokerHandType.HighCard, 5, 1);
    }

    private static List<(CardRank Rank, CardSuit Suit)> GetCardData(World world, List<Entity> cardEntities)
    {
        var data = new List<(CardRank, CardSuit)>();
        foreach (var cardEntity in cardEntities)
        {
            var rank = world.GetComponent<CardRankComponent>(cardEntity);
            var suit = world.GetComponent<CardSuitComponent>(cardEntity);
            if (rank.HasValue && suit.HasValue)
            {
                data.Add((rank.Value.Rank, suit.Value.Suit));
            }
        }
        return data;
    }

    private static bool IsFlushRoyal(List<(CardRank Rank, CardSuit Suit)> cards)
    {
        return IsStraightFlush(cards) && 
               cards.Any(c => c.Rank == CardRank.Ten) &&
               cards.Any(c => c.Rank == CardRank.Ace);
    }

    private static bool IsStraightFlush(List<(CardRank Rank, CardSuit Suit)> cards)
    {
        return IsFlush(cards) && IsStraight(cards);
    }

    private static bool IsFourOfAKind(List<(CardRank Rank, CardSuit Suit)> cards)
    {
        var ranks = cards.Select(c => c.Rank).ToList();
        return ranks.GroupBy(r => r).Any(g => g.Count() >= 4);
    }

    private static bool IsFullHouse(List<(CardRank Rank, CardSuit Suit)> cards)
    {
        var ranks = cards.Select(c => c.Rank).ToList();
        var groups = ranks.GroupBy(r => r).ToList();
        return groups.Any(g => g.Count() == 3) && groups.Any(g => g.Count() == 2);
    }

    private static bool IsFlush(List<(CardRank Rank, CardSuit Suit)> cards)
    {
        return cards.Select(c => c.Suit).Distinct().Count() == 1;
    }

    private static bool IsStraight(List<(CardRank Rank, CardSuit Suit)> cards)
    {
        if (cards.Count < 5) return false;
        
        var ranks = cards.Select(c => (int)c.Rank).OrderBy(r => r).ToList();
        
        // Проверяем обычный стрит
        bool isStraight = true;
        for (int i = 1; i < ranks.Count; i++)
        {
            if (ranks[i] != ranks[i - 1] + 1)
            {
                isStraight = false;
                break;
            }
        }
        
        // Проверяем стрит с тузом (A-2-3-4-5)
        bool isWheel = ranks.SequenceEqual(new[] { 2, 3, 4, 5, 14 });
        
        return isStraight || isWheel;
    }

    private static bool IsThreeOfAKind(List<(CardRank Rank, CardSuit Suit)> cards)
    {
        var ranks = cards.Select(c => c.Rank).ToList();
        return ranks.GroupBy(r => r).Any(g => g.Count() >= 3);
    }

    private static bool IsTwoPair(List<(CardRank Rank, CardSuit Suit)> cards)
    {
        var ranks = cards.Select(c => c.Rank).ToList();
        var pairs = ranks.GroupBy(r => r).Where(g => g.Count() >= 2).Count();
        return pairs >= 2;
    }

    private static bool IsPair(List<(CardRank Rank, CardSuit Suit)> cards)
    {
        var ranks = cards.Select(c => c.Rank).ToList();
        return ranks.GroupBy(r => r).Any(g => g.Count() >= 2);
    }
}

