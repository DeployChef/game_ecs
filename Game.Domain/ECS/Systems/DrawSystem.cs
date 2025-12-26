using Game.Domain.Core;
using Game.Domain.ECS.Components;

namespace Game.Domain.ECS.Systems;

/// <summary>
/// Система для взятия карт из колоды в руку.
/// 
/// Как работает:
/// 1. Находит все карты с CardStateComponent.InDeck
/// 2. Берет первую доступную карту
/// 3. Меняет State на InHand
/// 4. Добавляет Entity в HandComponent.Cards
/// 
/// Почему система, а не метод в World?
/// - Система инкапсулирует игровую логику
/// - Можно расширить (добавить условия, ограничения)
/// - Легко тестировать
/// </summary>
public class DrawSystem : ISystem
{
    /// <summary>
    /// Берет одну карту из колоды в руку.
    /// </summary>
    public bool DrawCard(World world, Entity handEntity)
    {
        // 1. Проверяем, что это рука
        var hand = world.GetComponent<HandComponent>(handEntity);
        if (!hand.HasValue)
            return false;

        var handComponent = hand.Value;

        // 2. Проверяем, есть ли место в руке
        if (handComponent.Cards.Count >= handComponent.MaxHandSize)
            return false; // Рука полная

        // 3. Находим первую доступную карту (InDeck)
        var availableCard = world.GetEntitiesWith<CardStateComponent>()
            .FirstOrDefault(e =>
            {
                var state = world.GetComponent<CardStateComponent>(e);
                return state.HasValue && state.Value.State == CardState.InDeck;
            });

        if (availableCard.Id == 0) // Entity(0) = не найдено
            return false; // Нет доступных карт

        // 4. Меняем состояние карты
        var cardState = world.GetComponent<CardStateComponent>(availableCard);
        if (cardState.HasValue)
        {
            var newState = cardState.Value;
            newState.State = CardState.InHand;
            world.AddComponent(availableCard, newState);
        }

        // 5. Добавляем карту в руку
        handComponent.Cards.Add(availableCard);
        world.AddComponent(handEntity, handComponent);

        return true;
    }

    /// <summary>
    /// Берет указанное количество карт из колоды в руку.
    /// </summary>
    public int DrawCards(World world, Entity handEntity, int count)
    {
        int drawn = 0;
        for (int i = 0; i < count; i++)
        {
            if (DrawCard(world, handEntity))
                drawn++;
            else
                break; // Больше нет карт или рука полная
        }
        return drawn;
    }

    /// <summary>
    /// Update вызывается каждый кадр/тик.
    /// Для DrawSystem не используется - рисование карт вызывается явно.
    /// </summary>
    public void Update(World world)
    {
        // Draw вызывается явно через DrawCard/DrawCards
    }
}

