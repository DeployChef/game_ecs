using Game.Domain.Core;
using Game.Domain.ECS;
using Game.Domain.ECS.Components;

namespace Game.Domain.Poker;

/// <summary>
/// Вычисляет итоговые очки руки по правилам Balatro.
/// 
/// Формула: Итоговые очки = (Сумма очков карт + Базовые очки комбинации) * Множитель
/// 
/// Где:
/// - Сумма очков карт = сумма базовых очков всех карт в руке (по рангу)
/// - Базовые очки комбинации = из HandEvaluationResult.BaseScore
/// - Множитель = из HandEvaluationResult.Multiplier
/// 
/// Это чистая функция - легко тестировать, нет побочных эффектов.
/// </summary>
public static class HandScoreCalculator
{
    /// <summary>
    /// Вычисляет итоговые очки руки.
    /// </summary>
    /// <param name="world">ECS World</param>
    /// <param name="handEntity">Entity руки</param>
    /// <param name="evaluationResult">Результат оценки комбинации</param>
    /// <returns>Итоговые очки руки</returns>
    public static int CalculateTotalScore(World world, Entity handEntity, HandEvaluationResult evaluationResult)
    {
        // Получаем список карт для подсчета очков
        var selected = world.GetComponent<SelectedCardsComponent>(handEntity);
        List<Entity> cardsToScore;

        if (selected.HasValue && selected.Value.SelectedCards.Count > 0)
        {
            // Считаем очки только выбранных карт
            cardsToScore = selected.Value.SelectedCards;
        }
        else
        {
            // Считаем очки всех карт в руке
            var hand = world.GetComponent<HandComponent>(handEntity);
            if (!hand.HasValue || hand.Value.Cards.Count == 0)
            {
                return 0;
            }
            cardsToScore = hand.Value.Cards;
        }

        // Суммируем базовые очки всех карт (из компонентов)
        int cardsScore = 0;
        foreach (var cardEntity in cardsToScore)
        {
            cardsScore += CardScoreCalculator.GetCardScore(world, cardEntity);
        }

        // Формула Balatro: (Сумма очков карт + Базовые очки комбинации) * Множитель
        int totalScore = (cardsScore + evaluationResult.BaseScore) * evaluationResult.Multiplier;

        return totalScore;
    }
}

