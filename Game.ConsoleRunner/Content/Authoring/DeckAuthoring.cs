namespace Game.ConsoleRunner.Content.Authoring;

/// <summary>
/// Authoring данные колоды - аналог MonoBehaviour компонента в Unity DOTS.
/// 
/// В Unity DOTS это был бы MonoBehaviour с массивом CardAuthoring.
/// У нас - структура данных, которая может быть из JSON, кода, редактора.
/// </summary>
public class DeckAuthoring
{
    public List<CardAuthoring> Cards { get; set; } = new();
}

