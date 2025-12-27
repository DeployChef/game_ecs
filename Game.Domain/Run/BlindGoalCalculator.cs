namespace Game.Domain.Run;

/// <summary>
/// Вычисляет цель по очкам (Blind Goal) для раунда.
/// 
/// Правила Balatro:
/// - Round 1: 300 очков
/// - Round 2: 450 очков  
/// - Round 3: 600 очков
/// - Round 4+: формула может быть более сложной
/// 
/// Это чистая функция - легко тестировать, нет побочных эффектов.
/// 
/// Почему отдельный класс:
/// - Бизнес-правила в одном месте
/// - Легко расширить (модификаторы, сложность)
/// - Тестируемость
/// </summary>
public static class BlindGoalCalculator
{
    /// <summary>
    /// Вычисляет цель для раунда внутри анте.
    /// </summary>
    /// <param name="ante">Текущий анте</param>
    /// <param name="round">Текущий раунд</param>
    /// <returns>Цель по очкам для победы в раунде</returns>
    public static int Calculate(Ante ante, Round round)
    {
        // Базовая формула: 300 + (round - 1) * 150
        // Round 1: 300
        // Round 2: 450
        // Round 3: 600
        
        if (round.Value == 1)
            return 300;
        
        return 300 + (round.Value - 1) * 150;
    }
}

