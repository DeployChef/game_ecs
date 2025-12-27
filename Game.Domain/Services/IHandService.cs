using Game.Domain.ECS;
using Game.Domain.Random;

namespace Game.Domain.Services;

/// <summary>
/// Доменный сервис для работы с рукой игрока.
/// Инкапсулирует операции с рукой, скрывая детали ECS от клиентов.
/// </summary>
public interface IHandService
{
    /// <summary>
    /// Берет указанное количество карт из колоды в руку.
    /// </summary>
    /// <param name="handEntity">Entity руки</param>
    /// <param name="count">Количество карт для взятия</param>
    /// <param name="rng">Генератор случайных чисел (обязателен для детерминизма)</param>
    /// <returns>Количество реально взятых карт</returns>
    /// <exception cref="ArgumentNullException">Если rng == null</exception>
    /// <exception cref="ArgumentException">Если count < 0</exception>
    int DrawCards(Entity handEntity, int count, IRandomNumberGenerator rng);
    
    /// <summary>
    /// Сортирует карты в руке по возрастанию (ранг, затем масть).
    /// </summary>
    /// <param name="handEntity">Entity руки</param>
    void SortHand(Entity handEntity);
    
    /// <summary>
    /// Сбрасывает все карты из руки.
    /// </summary>
    /// <param name="handEntity">Entity руки</param>
    void DiscardHand(Entity handEntity);
    
    /// <summary>
    /// Сбрасывает указанные карты из руки.
    /// </summary>
    /// <param name="handEntity">Entity руки</param>
    /// <param name="cardsToDiscard">Список Entity карт для сброса</param>
    /// <returns>Количество реально сброшенных карт</returns>
    int DiscardCards(Entity handEntity, IReadOnlyList<Entity> cardsToDiscard);
    
    /// <summary>
    /// Переключает выбор карты (toggle).
    /// </summary>
    /// <param name="handEntity">Entity руки</param>
    /// <param name="cardEntity">Entity карты</param>
    /// <returns>true если операция успешна, false если карта не в руке или достигнут лимит</returns>
    bool ToggleCardSelection(Entity handEntity, Entity cardEntity);
    
    /// <summary>
    /// Очищает выбор всех карт.
    /// </summary>
    /// <param name="handEntity">Entity руки</param>
    void ClearSelection(Entity handEntity);
    
    /// <summary>
    /// Получает информацию о руке (только чтение).
    /// </summary>
    /// <param name="handEntity">Entity руки</param>
    /// <returns>Информация о руке или null если Entity не является рукой</returns>
    HandInfo? GetHandInfo(Entity handEntity);
    
    /// <summary>
    /// Получает список выбранных карт.
    /// </summary>
    /// <param name="handEntity">Entity руки</param>
    /// <returns>Список Entity выбранных карт или пустой список</returns>
    IReadOnlyList<Entity> GetSelectedCards(Entity handEntity);
}

