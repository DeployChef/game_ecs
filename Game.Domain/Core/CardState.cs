namespace Game.Domain.Core;

/// <summary>
/// Состояние карты в игре
/// </summary>
public enum CardState
{
    InDeck,    // В колоде (доступна для взятия)
    InHand,    // В руке
    Discarded  // Сброшена
}

