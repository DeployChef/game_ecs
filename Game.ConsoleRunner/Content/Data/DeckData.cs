namespace Game.ConsoleRunner.Content.Data;

/// <summary>
/// Структура данных колоды из JSON.
/// Содержит массив ID карт, которые входят в колоду.
/// Аналог ScriptableObject в Unity.
/// </summary>
public class DeckData
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> CardIds { get; set; } = new();
}

