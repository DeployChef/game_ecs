using Game.Domain.Content.Authoring;
using Game.Domain.Core;
using Game.Domain.ECS.Components;

namespace Game.Domain.ECS.Baking;

/// <summary>
/// Baking система для DeckAuthoring - аналог IBaker<DeckAuthoring> из Unity DOTS.
/// 
/// В Unity DOTS это был бы:
/// public class DeckBaker : Baker<DeckAuthoring>
/// {
///     public override void Bake(DeckAuthoring authoring)
///     {
///         var entity = GetEntity();
///         // добавляем компоненты
///     }
/// }
/// 
/// У нас:
/// - Реализует IBaker<DeckAuthoring>
/// - Bake() конвертирует DeckAuthoring → Entity с компонентами
/// </summary>
public class DeckBaker : IBaker<DeckAuthoring>
{
    public void Bake(BakingContext context, DeckAuthoring authoring)
    {
        if (authoring == null)
            throw new ArgumentNullException(nameof(authoring));

        // Для каждой карты создаем Entity
        foreach (var cardAuthoring in authoring.Cards)
        {
            // 1. Создаем Entity
            Entity card = context.CreateEntity();

            // 2. Добавляем компоненты
            context.AddComponent(card, new CardRankComponent(cardAuthoring.Rank));
            context.AddComponent(card, new CardSuitComponent(cardAuthoring.Suit));
            context.AddComponent(card, new CardStateComponent(CardState.InDeck));
        }
    }
}

