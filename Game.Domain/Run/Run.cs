using Game.Domain.ECS;
using Game.Domain.Exceptions;
using Game.Domain.GameState;
using Game.Domain.Random;

namespace Game.Domain.Run;

/// <summary>
/// Ран (игровая сессия) - агрегат, содержащий весь контекст одного рана.
/// 
/// Ран создается при выборе колоды и инициализирует World с картами.
/// 
/// Инварианты:
/// - RoundState всегда валидное (Score >= 0, Goal > 0)
/// - RunState отражает текущее состояние рана (Playing, Lost, Won)
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
    /// Машина состояний игры (управляет состоянием хода)
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
    
    /// <summary>
    /// Текущее состояние раунда/анте.
    /// Инвариант: всегда валидное состояние (Score >= 0, Goal > 0)
    /// </summary>
    public RoundState RoundState { get; private set; }
    
    /// <summary>
    /// Состояние рана (игра продолжается, проигрыш, победа)
    /// </summary>
    public RunState State { get; private set; }
    
    /// <summary>
    /// Количество раундов в анте (в Balatro обычно 3)
    /// </summary>
    public const int RoundsPerAnte = 3;
    
    internal Run(World world, Entity handEntity, GameStateMachine stateMachine, 
                 string deckId, int seed, IRandomNumberGenerator rng, RoundState initialRoundState)
    {
        World = world ?? throw new ArgumentNullException(nameof(world));
        HandEntity = handEntity;
        StateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
        DeckId = deckId ?? throw new ArgumentNullException(nameof(deckId));
        Seed = seed;
        Rng = rng ?? throw new ArgumentNullException(nameof(rng));
        RoundState = initialRoundState;
        State = RunState.Playing;
    }
    
    /// <summary>
    /// Добавить очки после сыгранной руки.
    /// Также увеличивает счетчик сыгранных рук.
    /// </summary>
    /// <param name="points">Очки за руку (должно быть >= 0)</param>
    /// <exception cref="InvalidOperationException">Если игра не в состоянии Playing</exception>
    /// <exception cref="InvalidOperationException">Если достигнут лимит рук для игры</exception>
    public void AddHandScore(int points)
    {
        if (State != RunState.Playing)
            throw new InvalidOperationException($"Cannot add score when run is in state {State}");
        
        if (points < 0)
            throw new ArgumentException("Points cannot be negative", nameof(points));
        
        // Увеличиваем счетчик сыгранных рук
        RoundState = RoundState.IncrementHandsPlayed();
        
        // Добавляем очки
        RoundState = RoundState.AddScore(points);
        
        // Проверяем победу в раунде
        if (RoundState.IsComplete)
        {
            TryAdvanceRound();
        }
    }
    
    /// <summary>
    /// Увеличить счетчик сброшенных рук.
    /// </summary>
    /// <exception cref="InvalidOperationException">Если игра не в состоянии Playing</exception>
    /// <exception cref="InvalidOperationException">Если достигнут лимит рук для сброса</exception>
    public void IncrementHandsDiscarded()
    {
        if (State != RunState.Playing)
            throw new InvalidOperationException($"Cannot discard hand when run is in state {State}");
        
        RoundState = RoundState.IncrementHandsDiscarded();
        
        // Проверяем проигрыш: если не можем ни играть, ни сбрасывать
        CheckGameOver();
    }
    
    /// <summary>
    /// Попытка перехода к следующему раунду/анте.
    /// Вызывается автоматически при достижении цели раунда.
    /// </summary>
    /// <returns>true если переход выполнен, false если раунд не завершен</returns>
    public bool TryAdvanceRound()
    {
        if (State != RunState.Playing)
            return false;
        
        if (!RoundState.IsComplete)
            return false;
        
        // В Balatro обычно 3 раунда в анте
        if (RoundState.Round.Value >= RoundsPerAnte)
        {
            // Переход к следующему анте
            RoundState = RoundState.AdvanceToNextAnte();
        }
        else
        {
            // Переход к следующему раунду
            RoundState = RoundState.AdvanceToNextRound();
        }
        
        return true;
    }
    
    /// <summary>
    /// Проверяет условия проигрыша.
    /// Игрок проигрывает, если:
    /// - Не может больше играть (достигнут лимит рук для игры)
    /// - И не может больше сбрасывать (достигнут лимит рук для сброса)
    /// - И не достигнута цель раунда
    /// </summary>
    private void CheckGameOver()
    {
        if (State != RunState.Playing)
            return;
        
        // Проигрыш: не можем играть, не можем сбрасывать, и не достигли цели
        if (!RoundState.CanPlayHand && 
            !RoundState.CanDiscardHand && 
            !RoundState.IsComplete)
        {
            State = RunState.Lost;
        }
    }
    
    /// <summary>
    /// Явно завершить игру как проигрыш.
    /// Используется для явного завершения игры.
    /// </summary>
    public void Lose()
    {
        if (State != RunState.Playing)
            throw new InvalidOperationException($"Cannot lose when run is in state {State}");
        
        State = RunState.Lost;
    }
    
    /// <summary>
    /// Явно завершить игру как победу.
    /// Используется для явного завершения игры (например, после финального анте).
    /// </summary>
    public void Win()
    {
        if (State != RunState.Playing)
            throw new InvalidOperationException($"Cannot win when run is in state {State}");
        
        State = RunState.Won;
    }
}

/// <summary>
/// Состояние рана (игра продолжается, проигрыш, победа).
/// </summary>
public enum RunState
{
    /// <summary>
    /// Игра продолжается
    /// </summary>
    Playing,
    
    /// <summary>
    /// Игрок проиграл
    /// </summary>
    Lost,
    
    /// <summary>
    /// Игрок выиграл (достиг финального анте)
    /// </summary>
    Won
}

