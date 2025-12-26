namespace Game.Domain.Random;

/// <summary>
/// Абстракция для генерации случайных чисел.
/// Позволяет использовать детерминированный RNG для тестирования и воспроизводимости.
/// </summary>
public interface IRandomNumberGenerator
{
    /// <summary>
    /// Генерирует случайное целое число от 0 (включительно) до maxValue (исключительно)
    /// </summary>
    int Next(int maxValue);

    /// <summary>
    /// Генерирует случайное целое число от minValue (включительно) до maxValue (исключительно)
    /// </summary>
    int Next(int minValue, int maxValue);

    /// <summary>
    /// Генерирует случайное целое число от 0 до Int32.MaxValue
    /// </summary>
    int Next();
}

