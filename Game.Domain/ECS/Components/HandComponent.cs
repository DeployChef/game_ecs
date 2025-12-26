using Game.Domain.ECS;

namespace Game.Domain.ECS.Components;

/// <summary>
/// Компонент, хранящий данные о руке игрока.
/// 
/// Почему компонент, а не отдельный класс Hand?
/// - В ECS все данные - это компоненты
/// - Системы работают с компонентами, а не с классами
/// - Рука - это просто коллекция Entity ID карт
/// 
/// Почему List<Entity>, а не List<Card>?
/// - В ECS мы работаем с Entity, а не с объектами
/// - Entity - это ссылка на набор компонентов
/// - Системы находят карты по Entity ID
/// </summary>
public struct HandComponent : IComponent
{
    /// <summary>
    /// Entity ID карт в руке (порядок важен - слева направо)
    /// </summary>
    public List<Entity> Cards;

    /// <summary>
    /// Максимальный размер руки (может изменяться бафами/джокерами)
    /// </summary>
    public int MaxHandSize;

    public HandComponent(int maxHandSize)
    {
        Cards = new List<Entity>();
        MaxHandSize = maxHandSize;
    }
}

