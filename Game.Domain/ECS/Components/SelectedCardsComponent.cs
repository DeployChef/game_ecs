using Game.Domain.ECS;

namespace Game.Domain.ECS.Components;

/// <summary>
/// Компонент для хранения выбранных карт для игры.
/// 
/// Игрок выбирает карты из руки, они помечаются как выбранные.
/// Выбранные карты используются для оценки комбинации.
/// </summary>
public struct SelectedCardsComponent : IComponent
{
    /// <summary>
    /// Entity ID выбранных карт (порядок важен)
    /// </summary>
    public List<Entity> SelectedCards;

    public SelectedCardsComponent()
    {
        SelectedCards = new List<Entity>();
    }
}

