namespace Game.Domain.Poker;

/// <summary>
/// Тип покерной комбинации (от младшей к старшей)
/// </summary>
public enum PokerHandType
{
    HighCard,        // Старшая карта
    Pair,            // Пара
    TwoPair,         // Две пары
    ThreeOfAKind,    // Тройка
    Straight,        // Стрит
    Flush,           // Флеш
    FullHouse,       // Фулл-хаус
    FourOfAKind,     // Каре
    StraightFlush,   // Стрит-флеш
    FlushRoyal       // Флеш-рояль (стрит-флеш от 10 до туза)
}

