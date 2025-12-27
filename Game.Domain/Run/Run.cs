using Game.Domain.ECS;
using Game.Domain.GameState;
using Game.Domain.Random;

namespace Game.Domain.Run;

/// <summary>
/// Ран (игровая сессия) - агрегат, содержащий весь контекст одного рана.
/// 
/// Ран создается при выборе колоды и инициализирует World с картами.
/// </summary>
public class Run
{
    /// <summary>
    /// ECS World с активными Entity (карты, рука, и т.д.)
    /// </summary>
    public World World { get; }
    
    /// <summary>
    /// Entity руки игрока
    /// </summary>
    public Entity HandEntity { get; }
    
    /// <summary>
    /// Машина состояний игры
    /// </summary>
    public GameStateMachine StateMachine { get; }
    
    /// <summary>
    /// ID выбранной колоды
    /// </summary>
    public string DeckId { get; }
    
    /// <summary>
    /// Seed для RNG (для воспроизводимости)
    /// </summary>
    public int Seed { get; }
    
    /// <summary>
    /// Генератор случайных чисел для рана
    /// </summary>
    public IRandomNumberGenerator Rng { get; }
    
    internal Run(World world, Entity handEntity, GameStateMachine stateMachine, 
                 string deckId, int seed, IRandomNumberGenerator rng)
    {
        World = world ?? throw new ArgumentNullException(nameof(world));
        HandEntity = handEntity;
        StateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
        DeckId = deckId ?? throw new ArgumentNullException(nameof(deckId));
        Seed = seed;
        Rng = rng ?? throw new ArgumentNullException(nameof(rng));
    }
}

