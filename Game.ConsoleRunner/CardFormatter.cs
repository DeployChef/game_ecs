using Game.Domain.Core;

namespace Game.ConsoleRunner;

/// <summary>
/// Форматирует карты для вывода в консоль.
/// 
/// Ранги: 2-10 как цифры, J (Jack), Q (Queen), K (King), A (Ace)
/// Масти: ♠ (Spades), ♥ (Hearts), ♦ (Diamonds), ♣ (Clubs)
/// </summary>
public static class CardFormatter
{
    /// <summary>
    /// Форматирует ранг карты для вывода.
    /// </summary>
    public static string FormatRank(CardRank rank)
    {
        return rank switch
        {
            CardRank.Two => "2",
            CardRank.Three => "3",
            CardRank.Four => "4",
            CardRank.Five => "5",
            CardRank.Six => "6",
            CardRank.Seven => "7",
            CardRank.Eight => "8",
            CardRank.Nine => "9",
            CardRank.Ten => "10",
            CardRank.Jack => "J",
            CardRank.Queen => "Q",
            CardRank.King => "K",
            CardRank.Ace => "A",
            _ => rank.ToString()
        };
    }
    
    /// <summary>
    /// Форматирует масть карты для вывода.
    /// </summary>
    public static string FormatSuit(CardSuit suit)
    {
        return suit switch
        {
            CardSuit.Spades => "♠",
            CardSuit.Hearts => "♥",
            CardSuit.Diamonds => "♦",
            CardSuit.Clubs => "♣",
            _ => suit.ToString()
        };
    }
    
    /// <summary>
    /// Форматирует карту для вывода (ранг + табуляция + масть).
    /// </summary>
    public static string FormatCard(CardRank rank, CardSuit suit)
    {
        return $"{FormatRank(rank)}\t{FormatSuit(suit)}";
    }
}

