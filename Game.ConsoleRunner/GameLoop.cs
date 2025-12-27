using Game.Domain.Core;
using Game.Domain.ECS;
using Game.Domain.ECS.Components;
using Game.Domain.GameState;
using Game.Domain.Poker;
using Game.Domain.Services;
using Run = Game.Domain.Run.Run;

namespace Game.ConsoleRunner;

/// <summary>
/// Игровой цикл для консольного интерфейса.
/// </summary>
public class GameLoop
{
    private readonly Run _run;
    private readonly IHandService _handService;

    public GameLoop(Run run, IHandService handService)
    {
        _run = run ?? throw new ArgumentNullException(nameof(run));
        _handService = handService ?? throw new ArgumentNullException(nameof(handService));
    }

    public void Run()
    {
        Console.WriteLine("=== Balatro-like Card Game ===");
        Console.WriteLine();

        // Инициализация
        _run.StateMachine.StartTurn();
        Console.WriteLine($"Состояние: {_run.StateMachine.CurrentState}");

        // Игровой цикл
        while (_run.StateMachine.CurrentState != GameStateType.EndTurn)
        {
            switch (_run.StateMachine.CurrentState)
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
        
        // Получаем информацию о руке через сервис
        var handInfo = _handService.GetHandInfo(_run.HandEntity);
        if (handInfo == null)
        {
            Console.WriteLine("Ошибка: рука не найдена!");
            _run.StateMachine.EndTurn();
            return;
        }
        
        int cardsToDraw = handInfo.Value.AvailableSlots;
        int drawn = _handService.DrawCards(_run.HandEntity, cardsToDraw, _run.Rng);
        Console.WriteLine($"Взято карт: {drawn} (максимум в руке: {handInfo.Value.MaxHandSize})");

        if (drawn > 0)
        {
            // Сортируем руку после взятия карт
            _handService.SortHand(_run.HandEntity);
            ShowHand();
            _run.StateMachine.CardsDrawn();
        }
        else
        {
            Console.WriteLine("Нет доступных карт в колоде!");
            _run.StateMachine.EndTurn();
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
            var selectedCards = _handService.GetSelectedCards(_run.HandEntity);
            if (selectedCards.Count == 0)
            {
                Console.WriteLine("Нет выбранных карт для сброса! Выберите карты сначала.");
                return;
            }

            // Ограничение: максимум 5 карт для сброса
            if (selectedCards.Count > 5)
            {
                Console.WriteLine($"Можно сбросить максимум 5 карт! Выбрано: {selectedCards.Count}");
                Console.WriteLine("Снимите выбор с лишних карт.");
                return;
            }

            int discarded = _handService.DiscardCards(_run.HandEntity, selectedCards);
            Console.WriteLine($"Сброшено карт: {discarded}");
            
            // Очищаем выбор после сброса
            _handService.ClearSelection(_run.HandEntity);
            
            // Добираем карты до максимума в руке
            var handAfter = _handService.GetHandInfo(_run.HandEntity);
            if (handAfter.HasValue)
            {
                int cardsToDraw = handAfter.Value.AvailableSlots;
                
                if (cardsToDraw > 0)
                {
                    int drawn = _handService.DrawCards(_run.HandEntity, cardsToDraw, _run.Rng);
                    Console.WriteLine($"Добрано карт: {drawn}");
                    
                    // Сортируем руку после добора карт
                    _handService.SortHand(_run.HandEntity);
                }
            }
            return;
        }

        // Команда очистки выбора
        if (lowerInput == "clear")
        {
            _handService.ClearSelection(_run.HandEntity);
            Console.WriteLine("Выбор снят со всех карт.");
            return;
        }

        // Команда игры выбранных карт
        if (lowerInput == "play")
        {
            var selectedCards = _handService.GetSelectedCards(_run.HandEntity);
            if (selectedCards.Count == 0)
            {
                Console.WriteLine("Нет выбранных карт! Выберите карты для игры.");
                return;
            }

            // Ограничение: максимум 5 карт для игры
            if (selectedCards.Count > 5)
            {
                Console.WriteLine($"Можно сыграть максимум 5 карт! Выбрано: {selectedCards.Count}");
                Console.WriteLine("Снимите выбор с лишних карт.");
                return;
            }

            _run.StateMachine.CardsSelected();
            return;
        }

        // Выбор карт по номерам (toggle)
        var numbers = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var handForSelection = _handService.GetHandInfo(_run.HandEntity);
        if (!handForSelection.HasValue)
            return;

        var currentSelected = _handService.GetSelectedCards(_run.HandEntity);
        var selectedSet = new HashSet<Entity>(currentSelected);

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

                if (_handService.ToggleCardSelection(_run.HandEntity, cardEntity))
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
            var selectedAfter = _handService.GetSelectedCards(_run.HandEntity);
            Console.WriteLine($"Выбрано карт: {selectedAfter.Count}/5. Введите 'play' для игры, 'discard' для сброса или выберите еще карты.");
        }
        else
        {
            Console.WriteLine("Неверные номера карт!");
        }
    }

    private void HandlePlayingHand()
    {
        Console.WriteLine("\n--- Игра руки ---");

        var selectedCards = _handService.GetSelectedCards(_run.HandEntity);
        if (selectedCards.Count > 0)
        {
            Console.WriteLine("Выбранные карты:");
            ShowSelectedCards(selectedCards);
        }
        else
        {
            Console.WriteLine("Играются все карты в руке:");
            ShowHand();
        }

        // Оценка руки
        var result = HandEvaluator.Evaluate(_run.World, _run.HandEntity);
        Console.WriteLine($"\nКомбинация: {result.HandType}");
        Console.WriteLine($"Базовые очки: {result.BaseScore}");

        _run.StateMachine.HandCompleted();
    }

    private void HandleHandComplete()
    {
        Console.WriteLine("\n--- Сброс карт ---");
        _handService.DiscardHand(_run.HandEntity);
        Console.WriteLine("Карты сброшены");

        // При новом раунде колода автоматически перемешивается через RNG в DrawSystem
        Console.WriteLine("Колода перемешана для нового раунда");

        Console.WriteLine("\nПродолжить? (y/n)");
        string? input = Console.ReadLine();
        if (input?.ToLower() == "y")
        {
            _run.StateMachine.StartTurn();
        }
        else
        {
            _run.StateMachine.EndTurn();
        }
    }

    private void ShowHand()
    {
        var handInfo = _handService.GetHandInfo(_run.HandEntity);
        if (!handInfo.HasValue || handInfo.Value.Cards.Count == 0)
        {
            Console.WriteLine("Рука пуста");
            return;
        }

        foreach (var cardEntity in handInfo.Value.Cards)
        {
            var rank = _run.World.GetComponent<CardRankComponent>(cardEntity);
            var suit = _run.World.GetComponent<CardSuitComponent>(cardEntity);
            if (rank.HasValue && suit.HasValue)
            {
                Console.WriteLine($"  - {rank.Value.Rank} {suit.Value.Suit}");
            }
        }
    }

    private void ShowHandWithNumbers()
    {
        var handInfo = _handService.GetHandInfo(_run.HandEntity);
        if (!handInfo.HasValue || handInfo.Value.Cards.Count == 0)
        {
            Console.WriteLine("Рука пуста");
            return;
        }

        var selectedCards = _handService.GetSelectedCards(_run.HandEntity);
        var selectedSet = new HashSet<Entity>(selectedCards);

        for (int i = 0; i < handInfo.Value.Cards.Count; i++)
        {
            var cardEntity = handInfo.Value.Cards[i];
            var rank = _run.World.GetComponent<CardRankComponent>(cardEntity);
            var suit = _run.World.GetComponent<CardSuitComponent>(cardEntity);
            var marker = selectedSet.Contains(cardEntity) ? " [✓]" : "";
            
            if (rank.HasValue && suit.HasValue)
            {
                Console.WriteLine($"  {i + 1}. {rank.Value.Rank} {suit.Value.Suit}{marker}");
            }
        }
    }

    private void ShowSelectedCards(IReadOnlyList<Entity> selectedCards)
    {
        foreach (var cardEntity in selectedCards)
        {
            var rank = _run.World.GetComponent<CardRankComponent>(cardEntity);
            var suit = _run.World.GetComponent<CardSuitComponent>(cardEntity);
            if (rank.HasValue && suit.HasValue)
            {
                Console.WriteLine($"  - {rank.Value.Rank} {suit.Value.Suit}");
            }
        }
    }

}

