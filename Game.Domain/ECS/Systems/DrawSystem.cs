using Game.Domain.Core;
using Game.Domain.ECS.Components;
using Game.Domain.Random;

namespace Game.Domain.ECS.Systems;

/// <summary>
/// Система для взятия карт из колоды в руку.
/// 
/// Как работает:
/// 1. Находит все карты с CardStateComponent.InDeck
/// 2. Берет случайную карту (используя RNG для перемешивания)
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
    /// Использует RNG для выбора случайной карты (эффект перемешивания).
    /// </summary>
    public bool DrawCard(World world, Entity handEntity, IRandomNumberGenerator? rng = null)
    {
        // 1. Проверяем, что это рука
        var hand = world.GetComponent<HandComponent>(handEntity);
        if (!hand.HasValue)
            return false;

        var handComponent = hand.Value;

        // 2. Проверяем, есть ли место в руке
        if (handComponent.Cards.Count >= handComponent.MaxHandSize)
            return false; // Рука полная

        // 3. Находим все доступные карты (InDeck)
        var availableCards = world.GetEntitiesWith<CardStateComponent>()
            .Where(e =>
            {
                var state = world.GetComponent<CardStateComponent>(e);
                return state.HasValue && state.Value.State == CardState.InDeck;
            })
            .ToList();

        if (availableCards.Count == 0)
            return false; // Нет доступных карт

        // 4. Выбираем случайную карту (перемешивание через RNG)
        Entity availableCard;
        if (rng != null && availableCards.Count > 1)
        {
            int randomIndex = rng.Next(availableCards.Count);
            availableCard = availableCards[randomIndex];
        }
        else
        {
            // Если RNG не передан или только одна карта - берем первую
            availableCard = availableCards[0];
        }

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
    /// Использует RNG для перемешивания колоды.
    /// </summary>
    public int DrawCards(World world, Entity handEntity, int count, IRandomNumberGenerator? rng = null)
    {
        int drawn = 0;
        for (int i = 0; i < count; i++)
        {
            if (DrawCard(world, handEntity, rng))
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

