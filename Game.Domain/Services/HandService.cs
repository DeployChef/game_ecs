using Game.Domain.ECS;
using Game.Domain.ECS.Components;
using Game.Domain.ECS.Systems;
using Game.Domain.Exceptions;
using Game.Domain.Random;

namespace Game.Domain.Services;

/// <summary>
/// Реализация доменного сервиса для работы с рукой.
/// Инкапсулирует ECS-системы, предоставляя высокоуровневый интерфейс.
/// </summary>
public sealed class HandService : IHandService
{
    private readonly World _world;
    private readonly DrawSystem _drawSystem;
    private readonly DiscardSystem _discardSystem;
    private readonly HandSortSystem _sortSystem;
    private readonly CardSelectionSystem _selectionSystem;
    
    /// <summary>
    /// Создает новый экземпляр HandService.
    /// </summary>
    /// <param name="world">ECS World для работы с Entity и компонентами</param>
    /// <exception cref="ArgumentNullException">Если world == null</exception>
    public HandService(World world)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        
        // Системы создаются один раз и переиспользуются (stateless)
        _drawSystem = new DrawSystem();
        _discardSystem = new DiscardSystem();
        _sortSystem = new HandSortSystem();
        _selectionSystem = new CardSelectionSystem();
    }
    
    /// <inheritdoc />
    public int DrawCards(Entity handEntity, int count, IRandomNumberGenerator rng)
    {
        if (count < 0)
            throw new ArgumentException("Count cannot be negative", nameof(count));
        if (rng == null)
            throw new ArgumentNullException(nameof(rng));
        
        ValidateHandEntity(handEntity);
        
        return _drawSystem.DrawCards(_world, handEntity, count, rng);
    }
    
    /// <inheritdoc />
    public void SortHand(Entity handEntity)
    {
        ValidateHandEntity(handEntity);
        _sortSystem.SortHand(_world, handEntity);
    }
    
    /// <inheritdoc />
    public void DiscardHand(Entity handEntity)
    {
        ValidateHandEntity(handEntity);
        _discardSystem.DiscardHand(_world, handEntity);
        _selectionSystem.ClearSelection(_world, handEntity);
    }
    
    /// <inheritdoc />
    public int DiscardCards(Entity handEntity, IReadOnlyList<Entity> cardsToDiscard)
    {
        if (cardsToDiscard == null)
            throw new ArgumentNullException(nameof(cardsToDiscard));
        
        ValidateHandEntity(handEntity);
        
        // Конвертируем IReadOnlyList в List для совместимости с системой
        var cardsList = cardsToDiscard as List<Entity> ?? cardsToDiscard.ToList();
        return _discardSystem.DiscardCards(_world, handEntity, cardsList);
    }
    
    /// <inheritdoc />
    public bool ToggleCardSelection(Entity handEntity, Entity cardEntity)
    {
        ValidateHandEntity(handEntity);
        return _selectionSystem.ToggleCardSelection(_world, handEntity, cardEntity);
    }
    
    /// <inheritdoc />
    public void ClearSelection(Entity handEntity)
    {
        ValidateHandEntity(handEntity);
        _selectionSystem.ClearSelection(_world, handEntity);
    }
    
    /// <inheritdoc />
    public HandInfo? GetHandInfo(Entity handEntity)
    {
        var hand = _world.GetComponent<HandComponent>(handEntity);
        if (!hand.HasValue)
            return null;
        
        var handComponent = hand.Value;
        return new HandInfo(handComponent.Cards, handComponent.MaxHandSize);
    }
    
    /// <inheritdoc />
    public IReadOnlyList<Entity> GetSelectedCards(Entity handEntity)
    {
        ValidateHandEntity(handEntity);
        
        var selected = _world.GetComponent<SelectedCardsComponent>(handEntity);
        if (!selected.HasValue)
            return Array.Empty<Entity>();
        
        return selected.Value.SelectedCards;
    }
    
    /// <summary>
    /// Валидирует, что Entity является рукой (имеет HandComponent).
    /// </summary>
    /// <param name="handEntity">Entity для проверки</param>
    /// <exception cref="InvalidCardOperationException">Если Entity не является рукой</exception>
    private void ValidateHandEntity(Entity handEntity)
    {
        if (!_world.HasComponent<HandComponent>(handEntity))
        {
            throw new InvalidCardOperationException(
                $"Entity {handEntity} does not have HandComponent and is not a valid hand");
        }
    }
}

