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
    /// Update не используется - сброс вызывается явно.
    /// </summary>
    public void Update(World world)
    {
        // Discard вызывается явно через DiscardHand
    }
}

