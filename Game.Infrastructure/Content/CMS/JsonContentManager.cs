using System.Text.Json;
using Game.Domain.Core;
using Game.Infrastructure.Content.Authoring;
using Game.Infrastructure.Content.Data;

namespace Game.Infrastructure.Content.CMS;

/// <summary>
/// Реализация IContentManager для загрузки колод из JSON.
/// 
/// Загружает:
/// 1. DeckData из JSON (список ID карт)
/// 2. CardData для каждой карты из JSON
/// 3. Конвертирует в DeckAuthoring
/// </summary>
public class JsonContentManager : IContentManager
{
    private readonly string _contentPath;

    public JsonContentManager(string? contentPath = null)
    {
        // Если путь не указан, используем путь относительно исполняемого файла
        _contentPath = contentPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "Json");
    }

    public DeckAuthoring LoadDeck(string deckId)
    {
        // 1. Загружаем DeckData из JSON
        var deckPath = Path.Combine(_contentPath, "Decks", $"{deckId}.json");
        if (!File.Exists(deckPath))
            throw new FileNotFoundException($"Deck file not found: {deckPath}");

        var deckJson = File.ReadAllText(deckPath);
        var deckData = JsonSerializer.Deserialize<DeckData>(deckJson);
        if (deckData == null)
            throw new InvalidOperationException($"Failed to deserialize deck: {deckId}");

        // 2. Загружаем CardData для каждой карты
        var deckAuthoring = new DeckAuthoring();
        foreach (var cardId in deckData.CardIds)
        {
            var cardPath = Path.Combine(_contentPath, "Cards", $"{cardId}.json");
            if (!File.Exists(cardPath))
                throw new FileNotFoundException($"Card file not found: {cardPath}");

            var cardJson = File.ReadAllText(cardPath);
            var cardData = JsonSerializer.Deserialize<CardData>(cardJson);
            if (cardData == null)
                throw new InvalidOperationException($"Failed to deserialize card: {cardId}");

            // 3. Конвертируем CardData → CardAuthoring
            deckAuthoring.Cards.Add(new CardAuthoring
            {
                Rank = ParseRank(cardData.Rank),
                Suit = ParseSuit(cardData.Suit)
            });
        }

        return deckAuthoring;
    }

    private static CardRank ParseRank(string rankStr)
    {
        return rankStr switch
        {
            "Two" => CardRank.Two,
            "Three" => CardRank.Three,
            "Four" => CardRank.Four,
            "Five" => CardRank.Five,
            "Six" => CardRank.Six,
            "Seven" => CardRank.Seven,
            "Eight" => CardRank.Eight,
            "Nine" => CardRank.Nine,
            "Ten" => CardRank.Ten,
            "Jack" => CardRank.Jack,
            "Queen" => CardRank.Queen,
            "King" => CardRank.King,
            "Ace" => CardRank.Ace,
            _ => throw new ArgumentException($"Unknown rank: {rankStr}")
        };
    }

    private static CardSuit ParseSuit(string suitStr)
    {
        return suitStr switch
        {
            "Spades" => CardSuit.Spades,
            "Hearts" => CardSuit.Hearts,
            "Diamonds" => CardSuit.Diamonds,
            "Clubs" => CardSuit.Clubs,
            _ => throw new ArgumentException($"Unknown suit: {suitStr}")
        };
    }
}

