using Game.Domain.Core;

namespace Game.ConsoleRunner.Content.Data;

/// <summary>
/// Структура данных карты из JSON.
/// Аналог ScriptableObject в Unity.
/// </summary>
public class CardData
{
    public string Id { get; set; } = string.Empty;
    public string Rank { get; set; } = string.Empty;
    public string Suit { get; set; } = string.Empty;
}

