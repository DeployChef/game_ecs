using Game.ConsoleRunner.Baking;
using Game.ConsoleRunner.Content.Authoring;
using Game.ConsoleRunner.Content.CMS;
using Game.Domain.Core;
using Game.Domain.ECS;
using Game.Domain.ECS.Components;
using Game.Domain.ECS.Systems;
using Game.Domain.GameState;
using Game.Domain.Poker;
using Game.Domain.Random;

namespace Game.ConsoleRunner;

/// <summary>
/// Игровой цикл для консольного интерфейса.
/// </summary>
public class GameLoop
{
    private readonly World _world;
    private readonly Entity _handEntity;
    private readonly GameStateMachine _stateMachine;
    private readonly DrawSystem _drawSystem;
    private readonly DiscardSystem _discardSystem;
    private readonly CardSelectionSystem _selectionSystem;
    private IRandomNumberGenerator _rng;

    public GameLoop(World world, Entity handEntity, int? seed = null)
    {
        _world = world;
        _handEntity = handEntity;
        _stateMachine = new GameStateMachine(world, handEntity);
        _drawSystem = new DrawSystem();
        _discardSystem = new DiscardSystem();
        _selectionSystem = new CardSelectionSystem();
        
        // Создаем RNG с seed для детерминированного перемешивания
        _rng = seed.HasValue 
            ? new SeededRandomNumberGenerator(seed.Value) 
            : new SeededRandomNumberGenerator(Environment.TickCount);
    }

    public void Run()
    {
        Console.WriteLine("=== Balatro-like Card Game ===");
        Console.WriteLine();

        // Инициализация
        _stateMachine.StartTurn();
        Console.WriteLine($"Состояние: {_stateMachine.CurrentState}");

        // Игровой цикл
        while (_stateMachine.CurrentState != GameStateType.EndTurn)
        {
            switch (_stateMachine.CurrentState)
            {
                case GameStateType.DrawingHand:
                    HandleDrawingHand();
                    break;
                case GameStateType.SelectingCards:
                    HandleSelectingCards();
                    break;
                case GameStateType.PlayingHand:
                    HandlePlayingHand();
                    break;
                case GameStateType.HandComplete:
                    HandleHandComplete();
                    break;
            }
        }

        Console.WriteLine("\n=== Игра завершена ===");
    }

    private void HandleDrawingHand()
    {
        Console.WriteLine("\n--- Взятие карт ---");
        // Берем карты до максимума (8 карт)
        var hand = _world.GetComponent<HandComponent>(_handEntity);
        int maxCards = hand.HasValue ? hand.Value.MaxHandSize : 8;
        int currentCards = hand.HasValue ? hand.Value.Cards.Count : 0;
        int cardsToDraw = maxCards - currentCards;
        
        // Используем RNG для перемешивания колоды
        int drawn = _drawSystem.DrawCards(_world, _handEntity, cardsToDraw, _rng);
        Console.WriteLine($"Взято карт: {drawn} (максимум в руке: {maxCards})");

        if (drawn > 0)
        {
            // Сортируем руку после взятия карт
            SortHand();
            ShowHand();
            _stateMachine.CardsDrawn();
        }
        else
        {
            Console.WriteLine("Нет доступных карт в колоде!");
            _stateMachine.EndTurn();
        }
    }

    private void HandleSelectingCards()
    {
        Console.WriteLine("\n--- Управление картами ---");
        Console.WriteLine("Команды:");
        Console.WriteLine("  - Номера карт (например: 1 3 5) - выбрать/снять выбор с карт (toggle, максимум 5)");
        Console.WriteLine("  - 'discard' или 'd' - сбросить выбранные карты");
        Console.WriteLine("  - 'play' - сыграть выбранные карты (максимум 5)");
        Console.WriteLine("  - 'clear' - снять выбор со всех карт");

        ShowHandWithNumbers();

        string? input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("Неверный ввод!");
            return;
        }

        input = input.Trim();
        var lowerInput = input.ToLower();

        // Команда сброса выбранных карт
        if (lowerInput == "discard" || lowerInput == "d")
        {
            var selected = _world.GetComponent<SelectedCardsComponent>(_handEntity);
            if (!selected.HasValue || selected.Value.SelectedCards.Count == 0)
            {
                Console.WriteLine("Нет выбранных карт для сброса! Выберите карты сначала.");
                return;
            }

            // Ограничение: максимум 5 карт для сброса
            if (selected.Value.SelectedCards.Count > 5)
            {
                Console.WriteLine($"Можно сбросить максимум 5 карт! Выбрано: {selected.Value.SelectedCards.Count}");
                Console.WriteLine("Снимите выбор с лишних карт.");
                return;
            }

            int discarded = _discardSystem.DiscardCards(_world, _handEntity, selected.Value.SelectedCards);
            Console.WriteLine($"Сброшено карт: {discarded}");
            
            // Очищаем выбор после сброса
            _selectionSystem.ClearSelection(_world, _handEntity);
            
            // Добираем карты до максимума в руке
            var handAfter = _world.GetComponent<HandComponent>(_handEntity);
            if (handAfter.HasValue)
            {
                int maxCards = handAfter.Value.MaxHandSize;
                int currentCards = handAfter.Value.Cards.Count;
                int cardsToDraw = maxCards - currentCards;
                
                if (cardsToDraw > 0)
                {
                    // Используем RNG для перемешивания колоды
                    int drawn = _drawSystem.DrawCards(_world, _handEntity, cardsToDraw, _rng);
                    Console.WriteLine($"Добрано карт: {drawn}");
                    
                    // Сортируем руку после добора карт
                    SortHand();
                }
            }
            return;
        }

        // Команда очистки выбора
        if (lowerInput == "clear")
        {
            _selectionSystem.ClearSelection(_world, _handEntity);
            Console.WriteLine("Выбор снят со всех карт.");
            return;
        }

        // Команда игры выбранных карт
        if (lowerInput == "play")
        {
            var selected = _world.GetComponent<SelectedCardsComponent>(_handEntity);
            if (!selected.HasValue || selected.Value.SelectedCards.Count == 0)
            {
                Console.WriteLine("Нет выбранных карт! Выберите карты для игры.");
                return;
            }

            // Ограничение: максимум 5 карт для игры
            if (selected.Value.SelectedCards.Count > 5)
            {
                Console.WriteLine($"Можно сыграть максимум 5 карт! Выбрано: {selected.Value.SelectedCards.Count}");
                Console.WriteLine("Снимите выбор с лишних карт.");
                return;
            }

            _stateMachine.CardsSelected();
            return;
        }

        // Выбор карт по номерам (toggle)
        var numbers = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var handForSelection = _world.GetComponent<HandComponent>(_handEntity);
        if (!handForSelection.HasValue)
            return;

        var currentSelected = _world.GetComponent<SelectedCardsComponent>(_handEntity);
        var selectedSet = currentSelected.HasValue 
            ? new HashSet<Entity>(currentSelected.Value.SelectedCards) 
            : new HashSet<Entity>();

        int toggled = 0;
        foreach (var numStr in numbers)
        {
            if (int.TryParse(numStr, out int index) && index >= 1 && index <= handForSelection.Value.Cards.Count)
            {
                var cardEntity = handForSelection.Value.Cards[index - 1];
                
                // Проверяем, выбрана ли карта
                bool wasSelected = selectedSet.Contains(cardEntity);
                
                // Toggle: если выбрана - снимаем, если не выбрана - выбираем (с проверкой лимита)
                if (!wasSelected)
                {
                    // Проверяем лимит перед выбором
                    if (selectedSet.Count >= 5)
                    {
                        Console.WriteLine($"Можно выбрать максимум 5 карт! Уже выбрано: {selectedSet.Count}");
                        continue;
                    }
                }

                if (_selectionSystem.ToggleCardSelection(_world, _handEntity, cardEntity))
                {
                    toggled++;
                    // Обновляем set для следующей итерации
                    if (wasSelected)
                        selectedSet.Remove(cardEntity);
                    else
                        selectedSet.Add(cardEntity);
                }
            }
        }

        if (toggled > 0)
        {
            var selectedAfter = _world.GetComponent<SelectedCardsComponent>(_handEntity);
            int selectedCount = selectedAfter.HasValue ? selectedAfter.Value.SelectedCards.Count : 0;
            Console.WriteLine($"Выбрано карт: {selectedCount}/5. Введите 'play' для игры, 'discard' для сброса или выберите еще карты.");
        }
        else
        {
            Console.WriteLine("Неверные номера карт!");
        }
    }

    private void HandlePlayingHand()
    {
        Console.WriteLine("\n--- Игра руки ---");

        var selected = _world.GetComponent<SelectedCardsComponent>(_handEntity);
        if (selected.HasValue && selected.Value.SelectedCards.Count > 0)
        {
            Console.WriteLine("Выбранные карты:");
            ShowSelectedCards(selected.Value.SelectedCards);
        }
        else
        {
            Console.WriteLine("Играются все карты в руке:");
            ShowHand();
        }

        // Оценка руки
        var result = HandEvaluator.Evaluate(_world, _handEntity);
        Console.WriteLine($"\nКомбинация: {result.HandType}");
        Console.WriteLine($"Базовые очки: {result.BaseScore}");

        _stateMachine.HandCompleted();
    }

    private void HandleHandComplete()
    {
        Console.WriteLine("\n--- Сброс карт ---");
        _discardSystem.DiscardHand(_world, _handEntity);
        _selectionSystem.ClearSelection(_world, _handEntity);
        Console.WriteLine("Карты сброшены");

        // При новом раунде колода автоматически перемешивается через RNG в DrawSystem
        Console.WriteLine("Колода перемешана для нового раунда");

        Console.WriteLine("\nПродолжить? (y/n)");
        string? input = Console.ReadLine();
        if (input?.ToLower() == "y")
        {
            _stateMachine.StartTurn();
        }
        else
        {
            _stateMachine.EndTurn();
        }
    }

    private void ShowHand()
    {
        var hand = _world.GetComponent<HandComponent>(_handEntity);
        if (!hand.HasValue || hand.Value.Cards.Count == 0)
        {
            Console.WriteLine("Рука пуста");
            return;
        }

        foreach (var cardEntity in hand.Value.Cards)
        {
            var rank = _world.GetComponent<CardRankComponent>(cardEntity);
            var suit = _world.GetComponent<CardSuitComponent>(cardEntity);
            if (rank.HasValue && suit.HasValue)
            {
                Console.WriteLine($"  - {rank.Value.Rank} {suit.Value.Suit}");
            }
        }
    }

    private void ShowHandWithNumbers()
    {
        var hand = _world.GetComponent<HandComponent>(_handEntity);
        if (!hand.HasValue || hand.Value.Cards.Count == 0)
        {
            Console.WriteLine("Рука пуста");
            return;
        }

        var selected = _world.GetComponent<SelectedCardsComponent>(_handEntity);
        var selectedSet = selected.HasValue 
            ? new HashSet<Entity>(selected.Value.SelectedCards) 
            : new HashSet<Entity>();

        for (int i = 0; i < hand.Value.Cards.Count; i++)
        {
            var cardEntity = hand.Value.Cards[i];
            var rank = _world.GetComponent<CardRankComponent>(cardEntity);
            var suit = _world.GetComponent<CardSuitComponent>(cardEntity);
            var marker = selectedSet.Contains(cardEntity) ? " [✓]" : "";
            
            if (rank.HasValue && suit.HasValue)
            {
                Console.WriteLine($"  {i + 1}. {rank.Value.Rank} {suit.Value.Suit}{marker}");
            }
        }
    }

    private void ShowSelectedCards(List<Entity> selectedCards)
    {
        foreach (var cardEntity in selectedCards)
        {
            var rank = _world.GetComponent<CardRankComponent>(cardEntity);
            var suit = _world.GetComponent<CardSuitComponent>(cardEntity);
            if (rank.HasValue && suit.HasValue)
            {
                Console.WriteLine($"  - {rank.Value.Rank} {suit.Value.Suit}");
            }
        }
    }

    /// <summary>
    /// Сортирует карты в руке по возрастанию (ранг, затем масть).
    /// </summary>
    private void SortHand()
    {
        var hand = _world.GetComponent<HandComponent>(_handEntity);
        if (!hand.HasValue || hand.Value.Cards.Count <= 1)
            return;

        var handComponent = hand.Value;

        // Сортируем карты: сначала по рангу, затем по масти
        handComponent.Cards.Sort((card1, card2) =>
        {
            var rank1 = _world.GetComponent<CardRankComponent>(card1);
            var suit1 = _world.GetComponent<CardSuitComponent>(card1);
            var rank2 = _world.GetComponent<CardRankComponent>(card2);
            var suit2 = _world.GetComponent<CardSuitComponent>(card2);

            if (!rank1.HasValue || !suit1.HasValue || !rank2.HasValue || !suit2.HasValue)
                return 0;

            // Сначала сравниваем по рангу
            int rankComparison = rank1.Value.Rank.CompareTo(rank2.Value.Rank);
            if (rankComparison != 0)
                return rankComparison;

            // Если ранги одинаковые - сравниваем по масти
            return suit1.Value.Suit.CompareTo(suit2.Value.Suit);
        });

        // Обновляем компонент
        _world.AddComponent(_handEntity, handComponent);
    }
}

