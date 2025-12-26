namespace Game.Domain.Exceptions;

/// <summary>
/// Базовое исключение для нарушений доменных инвариантов
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
    protected DomainException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Нарушение инварианта колоды (карты не сохраняются)
/// </summary>
public class DeckInvariantViolationException : DomainException
{
    public DeckInvariantViolationException(string message) : base(message) { }
}

/// <summary>
/// Попытка выполнить операцию с картами, которая нарушает правила игры
/// </summary>
public class InvalidCardOperationException : DomainException
{
    public InvalidCardOperationException(string message) : base(message) { }
}

/// <summary>
/// Недопустимый переход состояния игры
/// </summary>
public class InvalidStateTransitionException : DomainException
{
    public InvalidStateTransitionException(string message) : base(message) { }
}

/// <summary>
/// Нарушение инварианта подсчета очков
/// </summary>
public class ScoringInvariantViolationException : DomainException
{
    public ScoringInvariantViolationException(string message) : base(message) { }
}

