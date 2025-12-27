using Game.Domain.ECS;
using Game.Domain.Random;
using Game.Domain.Services;
using Game.Domain.Run;

namespace Game.Application.UseCases;

/// <summary>
/// Use case для старта нового рана.
/// 
/// Оркестрирует создание рана через RunService.
/// Примечание: Baking (конвертация данных в Entity) выполняется в инфраструктуре
/// перед вызовом этого UseCase. World передается уже с загруженными Entity.
/// RNG создается в инфраструктуре и передается через параметр.
/// </summary>
public class StartRunUseCase
{
    private readonly IRunService _runService;
    
    public StartRunUseCase(IRunService runService)
    {
        _runService = runService ?? throw new ArgumentNullException(nameof(runService));
    }
    
    /// <summary>
    /// Запускает новый ран с указанной колодой.
    /// </summary>
    /// <param name="deckId">ID колоды</param>
    /// <param name="world">World с уже загруженными Entity карт (после Baking)</param>
    /// <param name="rng">Генератор случайных чисел (создается в инфраструктуре)</param>
    /// <param name="seed">Seed для RNG (опционально, для логирования)</param>
    /// <returns>Инициализированный ран</returns>
    public Game.Domain.Run.Run Execute(string deckId, World world, IRandomNumberGenerator rng, int? seed = null)
    {
        if (string.IsNullOrWhiteSpace(deckId))
            throw new ArgumentException("DeckId cannot be null or empty", nameof(deckId));
        if (world == null)
            throw new ArgumentNullException(nameof(world));
        if (rng == null)
            throw new ArgumentNullException(nameof(rng));
        
        return _runService.CreateRun(deckId, world, rng, seed);
    }
}

