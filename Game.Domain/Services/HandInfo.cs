using Game.Domain.ECS;

namespace Game.Domain.Services;

/// <summary>
/// Immutable информация о руке (value object).
/// Используется для чтения состояния без прямого доступа к компонентам.
/// </summary>
public readonly struct HandInfo
{
    /// <summary>
    /// Карты в руке (порядок важен)
    /// </summary>
    public IReadOnlyList<Entity> Cards { get; }
    
    /// <summary>
    /// Максимальный размер руки
    /// </summary>
    public int MaxHandSize { get; }
    
    /// <summary>
    /// Текущее количество карт в руке
    /// </summary>
    public int CurrentCount => Cards.Count;
    
    /// <summary>
    /// Рука заполнена до максимума
    /// </summary>
    public bool IsFull => CurrentCount >= MaxHandSize;
    
    /// <summary>
    /// Доступные слоты для карт
    /// </summary>
    public int AvailableSlots => Math.Max(0, MaxHandSize - CurrentCount);
    
    /// <summary>
    /// Создает новую информацию о руке.
    /// </summary>
    /// <param name="cards">Карты в руке</param>
    /// <param name="maxHandSize">Максимальный размер руки</param>
    /// <exception cref="ArgumentNullException">Если cards == null</exception>
    /// <exception cref="ArgumentException">Если maxHandSize <= 0</exception>
    public HandInfo(IReadOnlyList<Entity> cards, int maxHandSize)
    {
        Cards = cards ?? throw new ArgumentNullException(nameof(cards));
        MaxHandSize = maxHandSize > 0 
            ? maxHandSize 
            : throw new ArgumentException("MaxHandSize must be positive", nameof(maxHandSize));
    }
}

