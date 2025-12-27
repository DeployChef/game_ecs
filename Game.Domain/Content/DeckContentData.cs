using Game.Domain.Core;

namespace Game.Domain.Content;

/// <summary>
/// Доменное представление данных колоды (аналог ScriptableObject данных).
/// 
/// Это чисто доменная структура данных, не зависящая от формата хранения (JSON, бинарник, и т.д.).
/// </summary>
public class DeckContentData
{
    /// <summary>
    /// Список карт в колоде (ранг и масть)
    /// </summary>
    public List<CardContentData> Cards { get; set; } = new();
}

/// <summary>
/// Данные одной карты
/// </summary>
public class CardContentData
{
    public CardRank Rank { get; set; }
    public CardSuit Suit { get; set; }
}

