namespace Game.Domain.Run;

/// <summary>
/// Value Object для Анте (уровень сложности).
/// 
/// Инвариант: Ante >= 1
/// Immutable - все операции возвращают новый экземпляр.
/// 
/// Почему Value Object:
/// - Инварианты проверяются при создании
/// - Неизменяемость исключает невалидные состояния
/// - Легко тестировать и сравнивать
/// </summary>
public readonly struct Ante : IEquatable<Ante>
{
    /// <summary>
    /// Значение анте (начинается с 1)
    /// </summary>
    public int Value { get; }
    
    /// <summary>
    /// Создает новый Ante.
    /// </summary>
    /// <param name="value">Значение анте (должно быть >= 1)</param>
    /// <exception cref="ArgumentException">Если value < 1</exception>
    public Ante(int value)
    {
        if (value < 1)
            throw new ArgumentException("Ante must be at least 1", nameof(value));
        Value = value;
    }
    
    /// <summary>
    /// Возвращает следующий анте.
    /// </summary>
    public Ante Next() => new Ante(Value + 1);
    
    public bool Equals(Ante other) => Value == other.Value;
    
    public override bool Equals(object? obj) => obj is Ante other && Equals(other);
    
    public override int GetHashCode() => Value.GetHashCode();
    
    public static bool operator ==(Ante left, Ante right) => left.Equals(right);
    
    public static bool operator !=(Ante left, Ante right) => !left.Equals(right);
    
    public override string ToString() => $"Ante {Value}";
}

