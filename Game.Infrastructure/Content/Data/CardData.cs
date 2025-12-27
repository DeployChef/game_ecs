namespace Game.Infrastructure.Content.Data;

/// <summary>
/// Структура данных карты из JSON.
/// Аналог ScriptableObject в Unity.
/// </summary>
public class CardData
{
    public string Id { get; set; } = string.Empty;
    public string Rank { get; set; } = string.Empty;
    public string Suit { get; set; } = string.Empty;
    /// <summary>
    /// Базовые очки карты (из контента, не вычисляются из ранга)
    /// </summary>
    public int BaseScore { get; set; }
}

