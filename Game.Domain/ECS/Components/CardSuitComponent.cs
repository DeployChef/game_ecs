using Game.Domain.Core;
using Game.Domain.ECS;

namespace Game.Domain.ECS.Components;

/// <summary>
/// Компонент, хранящий масть карты.
/// </summary>
public struct CardSuitComponent : IComponent
{
    public CardSuit Suit;

    public CardSuitComponent(CardSuit suit)
    {
        Suit = suit;
    }
}

