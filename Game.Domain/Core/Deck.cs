using Game.Domain.Random;

namespace Game.Domain.Core;

/// <summary>
/// Колода карт. Содержит все карты игры, управляет их состоянием.
/// Карты с State = InDeck доступны для взятия.
/// </summary>
public class Deck
{
    private readonly List<Card> _allCards;

    /// <summary>
    /// Все карты в колоде (независимо от состояния)
    /// </summary>
    public IReadOnlyList<Card> AllCards => _allCards.AsReadOnly();

    /// <summary>
    /// Карты, доступные для взятия (State = InDeck)
    /// </summary>
    public IEnumerable<Card> AvailableCards => _allCards.Where(c => c.State == CardState.InDeck);

    /// <summary>
    /// Количество доступных карт
    /// </summary>
    public int AvailableCount => AvailableCards.Count();

    /// <summary>
    /// Общее количество карт в колоде
    /// </summary>
    public int TotalCount => _allCards.Count;

    public Deck(IEnumerable<Card> cards)
    {
        _allCards = new List<Card>(cards);
        // Устанавливаем дефолтное состояние для всех карт
        foreach (var card in _allCards)
        {
            card.State = CardState.InDeck;
        }
    }

    /// <summary>
    /// Берет первую доступную карту из колоды.
    /// Меняет State карты на InHand.
    /// Возвращает null, если нет доступных карт.
    /// </summary>
    public Card? Draw()
    {
        var card = AvailableCards.FirstOrDefault();
        if (card != null)
        {
            card.State = CardState.InHand;
        }
        return card;
    }

    /// <summary>
    /// Берет указанное количество карт из колоды.
    /// </summary>
    public List<Card> Draw(int count)
    {
        var drawn = new List<Card>();
        for (int i = 0; i < count; i++)
        {
            var card = Draw();
            if (card == null) break;
            drawn.Add(card);
        }
        return drawn;
    }

    /// <summary>
    /// Добавляет новую карту в колоду.
    /// Карта получает State = InDeck (дефолтное состояние).
    /// </summary>
    public void AddCard(Card card)
    {
        if (card == null) throw new ArgumentNullException(nameof(card));
        card.State = CardState.InDeck;
        _allCards.Add(card);
    }

    /// <summary>
    /// Перемешивает доступные карты (только те, у которых State = InDeck).
    /// </summary>
    public void Shuffle(IRandomNumberGenerator rng)
    {
        if (rng == null) throw new ArgumentNullException(nameof(rng));
        
        var availableCards = AvailableCards.ToList();
        var shuffled = availableCards.OrderBy(_ => rng.Next()).ToList();
        
        // Обновляем порядок в _allCards, сохраняя позиции недоступных карт
        int availableIndex = 0;
        for (int i = 0; i < _allCards.Count; i++)
        {
            if (_allCards[i].State == CardState.InDeck)
            {
                _allCards[i] = shuffled[availableIndex++];
            }
        }
    }
}

