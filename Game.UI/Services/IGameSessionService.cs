using Game.Domain.ECS;
using Game.Domain.GameState;
using Game.Domain.Run;
using Game.Domain.Services;

namespace Game.UI.Services;

/// <summary>
/// Сервис для управления игровой сессией пользователя.
/// Scoped - одна игра на пользователя.
/// </summary>
public interface IGameSessionService
{
    /// <summary>
    /// Начинает новую игру с указанной колодой.
    /// </summary>
    /// <param name="deckId">ID колоды</param>
    /// <returns>true если игра успешно начата</returns>
    bool StartNewGame(string deckId);
    
    /// <summary>
    /// Получает текущее состояние игры.
    /// </summary>
    /// <returns>Текущее состояние или null если игра не начата</returns>
    GameStateViewModel? GetGameState();
    
    /// <summary>
    /// Переключает выбор карты (toggle).
    /// </summary>
    /// <param name="cardEntity">Entity карты</param>
    /// <returns>true если операция успешна</returns>
    bool ToggleCardSelection(Entity cardEntity);
    
    /// <summary>
    /// Играет выбранные карты.
    /// </summary>
    /// <returns>Результат игры руки или null если ошибка</returns>
    HandResultViewModel? PlaySelectedCards();
    
    /// <summary>
    /// Сбрасывает выбранные карты.
    /// </summary>
    /// <returns>true если операция успешна</returns>
    bool DiscardSelectedCards();
    
    /// <summary>
    /// Получает информацию о руке.
    /// </summary>
    HandInfoViewModel? GetHandInfo();
    
    /// <summary>
    /// Получает состояние раунда.
    /// </summary>
    RoundStateViewModel? GetRoundState();
    
    /// <summary>
    /// Проверяет, начата ли игра.
    /// </summary>
    bool IsGameActive { get; }
    
    /// <summary>
    /// Обрабатывает состояние HandComplete - сбрасывает карты и переходит к следующему ходу.
    /// </summary>
    void HandleHandComplete();
}

/// <summary>
/// ViewModel для состояния игры.
/// </summary>
public class GameStateViewModel
{
    public GameStateType CurrentState { get; set; }
    public RunState RunState { get; set; }
    public RoundStateViewModel? RoundState { get; set; }
    public HandInfoViewModel? HandInfo { get; set; }
    public List<Entity> SelectedCards { get; set; } = new();
}

/// <summary>
/// ViewModel для информации о руке.
/// </summary>
public class HandInfoViewModel
{
    public List<CardViewModel> Cards { get; set; } = new();
    public int MaxHandSize { get; set; }
    public int CurrentCount { get; set; }
    public int AvailableSlots { get; set; }
}

/// <summary>
/// ViewModel для карты.
/// </summary>
public class CardViewModel
{
    public Entity Entity { get; set; }
    public string Rank { get; set; } = string.Empty;
    public string Suit { get; set; } = string.Empty;
    public string SuitSymbol { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
    public int Score { get; set; }
    public string SuitColor { get; set; } = string.Empty; // "red" or "black"
}

/// <summary>
/// ViewModel для состояния раунда.
/// </summary>
public class RoundStateViewModel
{
    public int Ante { get; set; }
    public int Round { get; set; }
    public int Score { get; set; }
    public int Goal { get; set; }
    public int Remaining { get; set; }
    public int HandsPlayed { get; set; }
    public int HandsDiscarded { get; set; }
    public int MaxHandsToPlay { get; set; }
    public int MaxHandsToDiscard { get; set; }
    public bool IsComplete { get; set; }
    public bool CanPlayHand { get; set; }
    public bool CanDiscardHand { get; set; }
}

/// <summary>
/// ViewModel для результата игры руки.
/// </summary>
public class HandResultViewModel
{
    public string HandType { get; set; } = string.Empty;
    public int CardsScore { get; set; }
    public int BaseScore { get; set; }
    public int Multiplier { get; set; }
    public int TotalScore { get; set; }
}

