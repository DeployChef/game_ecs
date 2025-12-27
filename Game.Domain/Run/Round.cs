namespace Game.Domain.Run;

/// <summary>
/// Value Object для Раунда (этап внутри анте).
/// 
/// Инвариант: Round >= 1
/// Immutable - все операции возвращают новый экземпляр.
/// </summary>
public readonly struct Round : IEquatable<Round>
{
    /// <summary>
    /// Значение раунда (начинается с 1)
    /// </summary>
    public int Value { get; }
    
    /// <summary>
    /// Создает новый Round.
    /// </summary>
    /// <param name="value">Значение раунда (должно быть >= 1)</param>
    /// <exception cref="ArgumentException">Если value < 1</exception>
    public Round(int value)
    {
        if (value < 1)
            throw new ArgumentException("Round must be at least 1", nameof(value));
        Value = value;
    }
    
    /// <summary>
    /// Возвращает следующий раунд.
    /// </summary>
    public Round Next() => new Round(Value + 1);
    
    public bool Equals(Round other) => Value == other.Value;
    
    public override bool Equals(object? obj) => obj is Round other && Equals(other);
    
    public override int GetHashCode() => Value.GetHashCode();
    
    public static bool operator ==(Round left, Round right) => left.Equals(right);
    
    public static bool operator !=(Round left, Round right) => !left.Equals(right);
    
    public override string ToString() => $"Round {Value}";
}

