namespace Game.Domain.GameState;

/// <summary>
/// Типы состояний игры.
/// </summary>
public enum GameStateType
{
    Initialized,    // Игра инициализирована, колода создана
    DrawingHand,    // Игрок берет карты из колоды в руку
    SelectingCards, // Игрок выбирает карты для игры
    PlayingHand,    // Карты разыграны, идет подсчет
    HandComplete,   // Рука завершена, карты в сброс
    EndTurn         // Конец хода (для будущего расширения)
}

