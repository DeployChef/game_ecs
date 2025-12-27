using Game.Domain.Core;
using Game.Domain.ECS.Components;

namespace Game.Domain.ECS.Systems;

/// <summary>
/// Система для сортировки карт в руке.
/// 
/// Сортирует карты по возрастанию:
/// 1. По рангу (от младшего к старшему)
/// 2. При одинаковом ранге - по масти (Spades, Hearts, Diamonds, Clubs)
/// </summary>
public class HandSortSystem : ISystem
{
    /// <summary>
    /// Сортирует карты в руке по возрастанию (ранг, затем масть).
    /// </summary>
    public void SortHand(World world, Entity handEntity)
    {
        var hand = world.GetComponent<HandComponent>(handEntity);
        if (!hand.HasValue)
            return;

        var handComponent = hand.Value;
        if (handComponent.Cards.Count <= 1)
            return; // Нет смысла сортировать 0 или 1 карту

        // Сортируем карты: сначала по рангу, затем по масти
        handComponent.Cards.Sort((card1, card2) =>
        {
            var rank1 = world.GetComponent<CardRankComponent>(card1);
            var suit1 = world.GetComponent<CardSuitComponent>(card1);
            var rank2 = world.GetComponent<CardRankComponent>(card2);
            var suit2 = world.GetComponent<CardSuitComponent>(card2);

            if (!rank1.HasValue || !suit1.HasValue || !rank2.HasValue || !suit2.HasValue)
                return 0; // Если нет данных - оставляем как есть

            // Сначала сравниваем по рангу
            int rankComparison = rank1.Value.Rank.CompareTo(rank2.Value.Rank);
            if (rankComparison != 0)
                return rankComparison;

            // Если ранги одинаковые - сравниваем по масти
            return suit1.Value.Suit.CompareTo(suit2.Value.Suit);
        });

        // Обновляем компонент
        world.AddComponent(handEntity, handComponent);
    }

    public void Update(World world)
    {
        // Sort вызывается явно через SortHand
    }
}

