namespace Game.Domain.Random;

/// <summary>
/// Недетерминированный генератор случайных чисел для production.
/// Использует System.Random.
/// </summary>
public class NonDeterministicRandomNumberGenerator : IRandomNumberGenerator
{
    private readonly System.Random _random;

    public NonDeterministicRandomNumberGenerator()
    {
        _random = new System.Random();
    }

    public NonDeterministicRandomNumberGenerator(int seed)
    {
        _random = new System.Random(seed);
    }

    public int Next(int maxValue) => _random.Next(maxValue);

    public int Next(int minValue, int maxValue) => _random.Next(minValue, maxValue);

    public int Next() => _random.Next();
}

