namespace Game.ConsoleRunner.Menu;

/// <summary>
/// Меню выбора колоды (презентация).
/// </summary>
public class DeckSelectionMenu
{
    /// <summary>
    /// Показывает меню выбора колоды и возвращает выбранный ID колоды.
    /// </summary>
    /// <returns>ID выбранной колоды или null если выход</returns>
    public string? ShowAndGetSelection()
    {
        Console.Clear();
        Console.WriteLine("=== Выбор колоды ===");
        Console.WriteLine();
        Console.WriteLine("1. Standard Deck");
        Console.WriteLine("2. Выход");
        Console.WriteLine();
        Console.Write("Выберите колоду: ");
        
        var input = Console.ReadLine();
        return input switch
        {
            "1" => "standard_deck",
            "2" => null, // Выход
            _ => ShowAndGetSelection() // Повтор при неверном вводе
        };
    }
}

