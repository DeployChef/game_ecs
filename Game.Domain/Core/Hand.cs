using Game.Domain.Exceptions;

namespace Game.Domain.Core;

/// <summary>
/// Рука игрока - коллекция карт, разыгрываемая за один ход.
/// Порядок карт важен (слева направо).
/// </summary>
public class Hand
{
    private readonly List<Card> _cards;
    
    /// <summary>
    /// Максимальный размер руки (может изменяться бафами/джокерами)
    /// </summary>
    public int MaxHandSize { get; set; }

    /// <summary>
    /// Карты в руке (порядок важен!)
    /// </summary>
    public IReadOnlyList<Card> Cards => _cards.AsReadOnly();

    /// <summary>
    /// Текущее количество карт в руке
    /// </summary>
    public int Count => _cards.Count;

    public Hand(int maxHandSize = 5)
    {
        if (maxHandSize < 1)
            throw new InvalidCardOperationException("MaxHandSize must be at least 1");
        
        _cards = new List<Card>();
        MaxHandSize = maxHandSize;
    }

    /// <summary>
    /// Добавляет карту в руку.
    /// Валидирует MaxHandSize.
    /// Предполагается, что State карты уже установлен в InHand (например, через Deck.Draw()).
    /// </summary>
    public void AddCard(Card card)
    {
        if (card == null) throw new ArgumentNullException(nameof(card));
        
        if (_cards.Count >= MaxHandSize)
            throw new InvalidCardOperationException($"Cannot add card: hand is full (MaxHandSize = {MaxHandSize})");

        _cards.Add(card);
    }

    /// <summary>
    /// Удаляет карту по индексу.
    /// Меняет State карты на Discarded.
    /// </summary>
    public Card RemoveCard(int index)
    {
        if (index < 0 || index >= _cards.Count)
            throw new InvalidCardOperationException($"Invalid card index: {index}");

        var card = _cards[index];
        _cards.RemoveAt(index);
        card.State = CardState.Discarded;
        return card;
    }

    /// <summary>
    /// Удаляет все карты из руки (сброс после игры).
    /// Меняет State всех карт на Discarded.
    /// </summary>
    public List<Card> DiscardAll()
    {
        var discarded = new List<Card>(_cards);
        foreach (var card in _cards)
        {
            card.State = CardState.Discarded;
        }
        _cards.Clear();
        return discarded;
    }

    /// <summary>
    /// Очищает руку без изменения State карт (для внутренних операций)
    /// </summary>
    public void Clear()
    {
        _cards.Clear();
    }
}

