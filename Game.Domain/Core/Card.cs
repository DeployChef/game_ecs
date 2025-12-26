using System.Runtime.CompilerServices;
using Game.Domain.Core.Enhancements;
using Game.Domain.Core.Editions;

namespace Game.Domain.Core;

/// <summary>
/// Карта - базовая единица игры. Мутабельный объект - состояние, улучшения и издание могут изменяться.
/// </summary>
public sealed class Card
{
    public CardRank Rank { get; }
    public CardSuit Suit { get; }
    public List<ICardEnhancement> Enhancements { get; }
    public ICardEdition? Edition { get; set; }
    public CardState State { get; set; }

    public Card(CardRank rank, CardSuit suit, List<ICardEnhancement>? enhancements = null, ICardEdition? edition = null)
    {
        Rank = rank;
        Suit = suit;
        Enhancements = enhancements ?? new List<ICardEnhancement>();
        Edition = edition;
        State = CardState.InDeck; // Дефолтное состояние при создании
    }

    /// <summary>
    /// Добавляет улучшение к карте (мутабельная операция)
    /// </summary>
    public void AddEnhancement(ICardEnhancement enhancement)
    {
        Enhancements.Add(enhancement);
    }

    public override bool Equals(object? obj)
    {
        return obj is Card card &&
               Rank == card.Rank &&
               Suit == card.Suit &&
               ReferenceEquals(this, card); // Сравнение по ссылке, так как карты мутабельные
    }

    public override int GetHashCode()
    {
        return RuntimeHelpers.GetHashCode(this); // Хэш по ссылке
    }

    public override string ToString()
    {
        var rankStr = Rank switch
        {
            CardRank.Jack => "J",
            CardRank.Queen => "Q",
            CardRank.King => "K",
            CardRank.Ace => "A",
            _ => ((int)Rank).ToString()
        };

        var suitStr = Suit switch
        {
            CardSuit.Spades => "♠",
            CardSuit.Hearts => "♥",
            CardSuit.Diamonds => "♦",
            CardSuit.Clubs => "♣",
            _ => Suit.ToString()
        };

        return $"{rankStr}{suitStr}";
    }
}

