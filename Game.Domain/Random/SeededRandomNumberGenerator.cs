namespace Game.Domain.Random;

/// <summary>
/// Детерминированный генератор случайных чисел на основе seed.
/// Использует простой линейный конгруэнтный генератор для воспроизводимости.
/// </summary>
public class SeededRandomNumberGenerator : IRandomNumberGenerator
{
    private long _seed;

    public SeededRandomNumberGenerator(int seed)
    {
        _seed = seed;
    }

    public int Next(int maxValue)
    {
        if (maxValue <= 0)
            throw new ArgumentException("maxValue must be positive", nameof(maxValue));

        return Next(0, maxValue);
    }

    public int Next(int minValue, int maxValue)
    {
        if (minValue >= maxValue)
            throw new ArgumentException("minValue must be less than maxValue", nameof(minValue));

        _seed = (_seed * 1103515245 + 12345) & 0x7fffffff; // Linear congruential generator
        var range = maxValue - minValue;
        return minValue + (int)(_seed % range);
    }

    public int Next()
    {
        _seed = (_seed * 1103515245 + 12345) & 0x7fffffff;
        return (int)(_seed & 0x7fffffff);
    }
}

