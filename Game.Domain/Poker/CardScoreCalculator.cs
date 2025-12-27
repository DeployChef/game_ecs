using Game.Domain.Core;
using Game.Domain.ECS;
using Game.Domain.ECS.Components;

namespace Game.Domain.Poker;

/// <summary>
/// Получает базовые очки карты из компонента.
/// 
/// Базовые очки берутся из CardScoreComponent, который создается из контента (JSON).
/// Это позволяет гибко настраивать очки для каждой карты через контент.
/// 
/// Если компонент CardScoreComponent отсутствует, возвращает 0.
/// </summary>
public static class CardScoreCalculator
{
    /// <summary>
    /// Получает базовые очки карты из компонента CardScoreComponent.
    /// </summary>
    /// <param name="world">ECS World</param>
    /// <param name="cardEntity">Entity карты</param>
    /// <returns>Базовые очки карты (0 если компонент отсутствует)</returns>
    public static int GetCardScore(World world, Entity cardEntity)
    {
        var scoreComponent = world.GetComponent<CardScoreComponent>(cardEntity);
        if (scoreComponent.HasValue)
        {
            return scoreComponent.Value.BaseScore;
        }
        
        // Если компонент отсутствует, возвращаем 0
        // Это не должно происходить в нормальной игре, но защита от ошибок
        return 0;
    }
}

