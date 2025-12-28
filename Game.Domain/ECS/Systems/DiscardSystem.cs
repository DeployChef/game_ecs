using Game.Domain.Core;
using Game.Domain.ECS.Components;

namespace Game.Domain.ECS.Systems;

/// <summary>
/// Система для сброса карт из руки.
/// 
/// Как работает:
/// 1. Находит все карты в HandComponent
/// 2. Меняет State на Discarded
/// 3. Очищает HandComponent.Cards
/// 
/// Почему система, а не метод в Hand?
/// - В ECS логика в системах, а не в компонентах
/// - Компоненты - это только данные
/// - Системы работают с компонентами через World
/// </summary>
public class DiscardSystem : ISystem
{
    /// <summary>
    /// Сбрасывает все карты из руки.
    /// </summary>
    public void DiscardHand(World world, Entity handEntity)
    {
        var hand = world.GetComponent<HandComponent>(handEntity);
        if (!hand.HasValue)
            return;

        var handComponent = hand.Value;

        // Меняем State всех карт на Discarded
        foreach (var cardEntity in handComponent.Cards)
        {
            var cardState = world.GetComponent<CardStateComponent>(cardEntity);
            if (cardState.HasValue)
            {
                var newState = cardState.Value;
                newState.State = CardState.Discarded;
                world.AddComponent(cardEntity, newState);
            }
        }

        // Очищаем руку
        handComponent.Cards.Clear();
        world.AddComponent(handEntity, handComponent);
    }

    /// <summary>
    /// Сбрасывает конкретные карты из руки.
    /// </summary>
    public int DiscardCards(World world, Entity handEntity, List<Entity> cardsToDiscard)
    {
        var hand = world.GetComponent<HandComponent>(handEntity);
        if (!hand.HasValue)
            return 0;

        var handComponent = hand.Value;
        int discarded = 0;

        // Меняем State выбранных карт на Discarded и удаляем из руки
        foreach (var cardEntity in cardsToDiscard)
        {
            if (!handComponent.Cards.Contains(cardEntity))
                continue;

            var cardState = world.GetComponent<CardStateComponent>(cardEntity);
            if (cardState.HasValue)
            {
                var newState = cardState.Value;
                newState.State = CardState.Discarded;
                world.AddComponent(cardEntity, newState);
            }

            handComponent.Cards.Remove(cardEntity);
            discarded++;
        }

        // Обновляем руку
        world.AddComponent(handEntity, handComponent);

        return discarded;
    }

    /// <summary>
    /// Возвращает все сброшенные карты обратно в колоду.
    /// Используется при переходе к новому раунду.
    /// </summary>
    public int ReturnAllCardsToDeck(World world)
    {
        // Находим все карты с состоянием Discarded или InHand
        var cardsToReturn = world.GetEntitiesWith<CardStateComponent>()
            .Where(e =>
            {
                var state = world.GetComponent<CardStateComponent>(e);
                return state.HasValue && (state.Value.State == CardState.Discarded || state.Value.State == CardState.InHand);
            })
            .ToList();

        int returned = 0;
        foreach (var cardEntity in cardsToReturn)
        {
            var cardState = world.GetComponent<CardStateComponent>(cardEntity);
            if (cardState.HasValue)
            {
                var newState = cardState.Value;
                newState.State = CardState.InDeck;
                world.AddComponent(cardEntity, newState);
                returned++;
            }
        }

        return returned;
    }

    /// <summary>
    /// Update не используется - сброс вызывается явно.
    /// </summary>
    public void Update(World world)
    {
        // Discard вызывается явно через DiscardHand
    }
}

