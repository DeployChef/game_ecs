using Game.Domain.Core;
using Game.Domain.ECS;

namespace Game.Domain.ECS.Components;

/// <summary>
/// Компонент, хранящий состояние карты (InDeck/InHand/Discarded).
/// 
/// Почему отдельный компонент, а не часть CardRankComponent?
/// - Разделение ответственности: ранг - это данные карты, состояние - это игровая логика
/// - Системы могут работать только с состоянием, не читая ранг
/// - Легко добавлять/удалять компонент состояния без изменения ранга
/// </summary>
public struct CardStateComponent : IComponent
{
    public CardState State;

    public CardStateComponent(CardState state)
    {
        State = state;
    }
}

