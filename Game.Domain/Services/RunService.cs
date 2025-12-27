using Game.Domain.ECS;
using Game.Domain.ECS.Components;
using Game.Domain.GameState;
using Game.Domain.Random;
using Game.Domain.Run;

namespace Game.Domain.Services;

/// <summary>
/// Сервис для создания и управления ранами (игровыми сессиями).
/// 
/// Инкапсулирует логику инициализации рана:
/// 1. Принимает уже загруженные данные колоды (DeckContentData)
/// 2. Создает World и инициализирует его с картами
/// 3. Инициализирует руку, StateMachine, RNG, RoundState
/// 
/// Примечание: Baking (конвертация данных в Entity) происходит в инфраструктуре,
/// перед вызовом CreateRun. RunService работает только с готовым World.
/// RNG создается в инфраструктуре и передается через параметр.
/// </summary>
public interface IRunService
{
    /// <summary>
    /// Создает новый ран с указанной колодой.
    /// </summary>
    /// <param name="deckId">ID колоды</param>
    /// <param name="world">World с уже загруженными Entity карт (после Baking)</param>
    /// <param name="rng">Генератор случайных чисел (создается в инфраструктуре)</param>
    /// <param name="seed">Seed для RNG (опционально, для логирования)</param>
    /// <returns>Инициализированный ран</returns>
    Game.Domain.Run.Run CreateRun(string deckId, World world, IRandomNumberGenerator rng, int? seed = null);
}

public class RunService : IRunService
{
    public Game.Domain.Run.Run CreateRun(string deckId, World world, IRandomNumberGenerator rng, int? seed = null)
    {
        if (string.IsNullOrWhiteSpace(deckId))
            throw new ArgumentException("DeckId cannot be null or empty", nameof(deckId));
        if (world == null)
            throw new ArgumentNullException(nameof(world));
        if (rng == null)
            throw new ArgumentNullException(nameof(rng));
        
        // 1. Создаем руку
        var handEntity = world.CreateEntity();
        world.AddComponent(handEntity, new HandComponent(maxHandSize: 8));
        
        // 2. Создаем StateMachine
        var stateMachine = new GameStateMachine(world, handEntity);
        
        // 3. Инициализируем RoundState: Ante 1, Round 1, Goal 300
        var initialAnte = new Ante(1);
        var initialRound = new Round(1);
        var initialGoal = BlindGoalCalculator.Calculate(initialAnte, initialRound);
        var initialRoundState = new RoundState(initialAnte, initialRound, 0, initialGoal);
        
        // 4. Создаем Run
        var actualSeed = seed ?? 0; // Seed для логирования, если не передан
        return new Game.Domain.Run.Run(world, handEntity, stateMachine, deckId, actualSeed, rng, initialRoundState);
    }
}

