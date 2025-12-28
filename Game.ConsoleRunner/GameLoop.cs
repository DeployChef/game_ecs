using Game.Domain.Core;
using Game.Domain.ECS;
using Game.Domain.ECS.Components;
using Game.Domain.GameState;
using Game.Domain.Poker;
using Game.Domain.Run;
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
        ShowRoundState();

        // Игровой цикл
        // Завершаем цикл если: state machine в EndTurn ИЛИ игра не в состоянии Playing (проигрыш/победа)
        while (_run.StateMachine.CurrentState != GameStateType.EndTurn && _run.State == Game.Domain.Run.RunState.Playing)
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
            
            // Дополнительная проверка после обработки состояния
            // Если игра завершена (проигрыш/победа), выходим из цикла
            if (_run.State != Game.Domain.Run.RunState.Playing)
            {
                break;
            }
        }

        // Проверка конца игры
        if (_run.State == Game.Domain.Run.RunState.Lost)
        {
            Console.WriteLine("\n=== ВЫ ПРОИГРАЛИ ===");
            Console.WriteLine($"Не удалось достичь цели в {_run.RoundState.Ante} - {_run.RoundState.Round}");
            Console.WriteLine($"Набрано очков: {_run.RoundState.Score}/{_run.RoundState.Goal}");
            Console.WriteLine("\nНажмите любую клавишу для возврата в меню...");
            Console.ReadKey();
        }
        else if (_run.State == Game.Domain.Run.RunState.Won)
        {
            Console.WriteLine("\n=== ВЫ ВЫИГРАЛИ ===");
            Console.WriteLine($"Поздравляем! Вы прошли все анте!");
            Console.WriteLine("\nНажмите любую клавишу для возврата в меню...");
            Console.ReadKey();
        }
        else
        {
            Console.WriteLine("\n=== Игра завершена ===");
        }
    }
    
    private void ShowRoundState()
    {
        var state = _run.RoundState;
        Console.WriteLine($"\n{state.Ante} - {state.Round} | Очки: {state.Score}/{state.Goal} (осталось: {state.Remaining})");
        Console.WriteLine($"Сыграно рук: {state.HandsPlayed}/{RoundState.MaxHandsToPlay} | Сброшено рук: {state.HandsDiscarded}/{RoundState.MaxHandsToDiscard}");
        if (state.IsComplete)
        {
            Console.WriteLine("✓ Раунд завершен! Переход к следующему...");
        }
    }
    
    /// <summary>
    /// Подсчитывает количество карт в колоде (состояние InDeck).
    /// </summary>
    private int CountCardsInDeck()
    {
        return _run.World.GetEntitiesWith<CardStateComponent>()
            .Count(e =>
            {
                var state = _run.World.GetComponent<CardStateComponent>(e);
                return state.HasValue && state.Value.State == CardState.InDeck;
            });
    }

    private void HandleDrawingHand()
    {
        Console.WriteLine("\n--- Взятие карт ---");
        
        // Получаем информацию о руке через сервис
        var handInfo = _handService.GetHandInfo(_run.HandEntity);
        if (handInfo == null)
        {
            Console.WriteLine("Ошибка: рука не найдена!");
            _run.Lose();
            return;
        }
        
        // Подсчитываем оставшиеся карты в колоде ДО взятия
        int cardsInDeck = CountCardsInDeck();
        
        int cardsToDraw = handInfo.Value.AvailableSlots;
        int drawn = _handService.DrawCards(_run.HandEntity, cardsToDraw, _run.Rng);
        
        // Подсчитываем оставшиеся карты в колоде ПОСЛЕ взятия
        int cardsInDeckAfter = CountCardsInDeck();
        
        Console.WriteLine($"Взято карт: {drawn} | Карт в колоде: {cardsInDeckAfter}");

        if (drawn > 0)
        {
            // Сортируем руку после взятия карт
            _handService.SortHand(_run.HandEntity);
            _run.StateMachine.CardsDrawn();
        }
        else
        {
            // Не взяли карты - проверяем причину
            if (cardsInDeckAfter == 0)
            {
                // Колода действительно пуста
                var currentHandInfo = _handService.GetHandInfo(_run.HandEntity);
                if (currentHandInfo.HasValue && currentHandInfo.Value.Cards.Count > 0)
                {
                    // В руке есть карты - можно продолжать играть
                    Console.WriteLine("Колода пуста, но в руке есть карты. Продолжаем с текущими картами.");
                    _run.StateMachine.CardsDrawn();
                }
                else
                {
                    // Колода пуста и рука пуста - конец игры (проигрыш)
                    Console.WriteLine("Колода пуста и рука пуста. Игра завершена.");
                    _run.Lose();
                }
            }
            else
            {
                // Колода не пуста, но не взяли карты - значит рука полная
                // Это нормальная ситуация, просто переходим к выбору карт
                _run.StateMachine.CardsDrawn();
            }
        }
    }

    private void HandleSelectingCards()
    {
        // Проверяем, не завершена ли игра
        if (_run.State != Game.Domain.Run.RunState.Playing)
        {
            // Игра уже завершена, не ждем ввода
            return;
        }
        
        Console.WriteLine("\n--- Управление картами ---");
        Console.WriteLine("Команды:");
        Console.WriteLine("  - Номера карт (например: 1 3 5) - выбрать/снять выбор с карт (toggle, максимум 5)");
        Console.WriteLine("  - 'discard' или 'd' - сбросить выбранные карты");
        Console.WriteLine("  - 'play' или 'p' - сыграть выбранные карты (максимум 5)");
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
            // Проверяем ограничение на количество сброшенных рук
            if (!_run.RoundState.CanDiscardHand)
            {
                Console.WriteLine($"Достигнут лимит сброса рук! Можно сбросить максимум {RoundState.MaxHandsToDiscard} рук за раунд.");
                Console.WriteLine($"Уже сброшено: {_run.RoundState.HandsDiscarded}");
                
                // Проверяем проигрыш: если не можем играть, не можем сбрасывать и не достигли цели
                _run.CheckGameOver();
                if (_run.State != Game.Domain.Run.RunState.Playing)
                {
                    Console.WriteLine("\n⚠ Нет доступных действий! Вы не можете больше играть или сбрасывать руки.");
                    Console.WriteLine("Игра завершена.");
                    // Состояние игры уже Lost, главный цикл завершится при следующей проверке
                    return;
                }
                return;
            }
            
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
            
            // Увеличиваем счетчик сброшенных рук
            try
            {
                _run.IncrementHandsDiscarded();
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                return;
            }
            
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
            
            // Проверяем конец игры после сброса
            if (_run.State != Game.Domain.Run.RunState.Playing)
            {
                return;
            }
            
            ShowRoundState();
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
        if (lowerInput == "play" || lowerInput == "p")
        {
            // Проверяем ограничение на количество сыгранных рук
            if (!_run.RoundState.CanPlayHand)
            {
                Console.WriteLine($"Достигнут лимит игры рук! Можно сыграть максимум {RoundState.MaxHandsToPlay} рук за раунд.");
                Console.WriteLine($"Уже сыграно: {_run.RoundState.HandsPlayed}");
                
                // Проверяем проигрыш: если не можем играть, не можем сбрасывать и не достигли цели
                _run.CheckGameOver();
                if (_run.State != Game.Domain.Run.RunState.Playing)
                {
                    Console.WriteLine("\n⚠ Нет доступных действий! Вы не можете больше играть или сбрасывать руки.");
                    Console.WriteLine("Игра завершена.");
                    // Состояние игры уже Lost, главный цикл завершится при следующей проверке
                    return;
                }
                return;
            }
            
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
            Console.WriteLine($"Выбрано карт: {selectedAfter.Count}/5. Введите 'play' или 'p' для игры, 'discard' или 'd' для сброса или выберите еще карты.");
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

        // Оценка комбинации
        var evaluationResult = HandEvaluator.Evaluate(_run.World, _run.HandEntity);
        
        // Вычисляем итоговые очки по формуле Balatro: (сумма очков карт + базовые очки комбинации) * множитель
        int totalScore = HandScoreCalculator.CalculateTotalScore(_run.World, _run.HandEntity, evaluationResult);
        
        // Вычисляем сумму очков карт для отображения
        var cardsToShow = _handService.GetSelectedCards(_run.HandEntity);
        if (cardsToShow.Count == 0)
        {
            var handInfo = _handService.GetHandInfo(_run.HandEntity);
            if (handInfo.HasValue)
            {
                cardsToShow = handInfo.Value.Cards;
            }
        }
        
        int cardsScore = 0;
        foreach (var cardEntity in cardsToShow)
        {
            var cardScore = CardScoreCalculator.GetCardScore(_run.World, cardEntity);
            cardsScore += cardScore;
            
            // Отладка: проверяем наличие компонента
            var scoreComponent = _run.World.GetComponent<CardScoreComponent>(cardEntity);
            var rank = _run.World.GetComponent<CardRankComponent>(cardEntity);
            if (!scoreComponent.HasValue && rank.HasValue)
            {
                Console.WriteLine($"⚠ ОШИБКА: У карты {rank.Value.Rank} отсутствует CardScoreComponent! Очки = 0");
            }
        }
        
        Console.WriteLine($"\nКомбинация: {evaluationResult.HandType}");
        Console.WriteLine($"Очки карт: {cardsScore}");
        Console.WriteLine($"Базовые очки комбинации: {evaluationResult.BaseScore}");
        Console.WriteLine($"Множитель: x{evaluationResult.Multiplier}");
        Console.WriteLine($"Итоговые очки: {totalScore} = ({cardsScore} + {evaluationResult.BaseScore}) × {evaluationResult.Multiplier}");

        // Сохраняем старое состояние раунда ДО добавления очков
        var oldRoundState = _run.RoundState;
        var wasRoundCompleteBefore = oldRoundState.IsComplete;

        // Добавляем итоговые очки к раунду (также увеличивает счетчик сыгранных рук)
        try
        {
            _run.AddHandScore(totalScore);
            Console.WriteLine($"\nОчки раунда: {_run.RoundState.Score}/{_run.RoundState.Goal}");
            
            // Проверяем, завершился ли раунд после добавления очков
            if (!wasRoundCompleteBefore && _run.RoundState.IsComplete)
            {
                Console.WriteLine("✓ Цель раунда достигнута!");
            }
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }

        _run.StateMachine.HandCompleted();
    }

    private void HandleHandComplete()
    {
        // Проверяем конец игры
        if (_run.State != Game.Domain.Run.RunState.Playing)
        {
            _run.StateMachine.EndTurn();
            return;
        }

        // Проверяем, был ли переход к новому раунду (сравниваем текущее состояние с предыдущим)
        // Если раунд изменился, значит был переход
        var currentRoundState = _run.RoundState;
        var isNewRound = currentRoundState.Score == 0 && currentRoundState.HandsPlayed == 0 && currentRoundState.HandsDiscarded == 0;

        // Если новый раунд - полная пересборка руки (сброс всех карт, взятие новой, сортировка)
        if (isNewRound)
        {
            Console.WriteLine("\n--- Переход к новому раунду ---");
            Console.WriteLine($"Текущий раунд: {_run.RoundState.Ante} - {_run.RoundState.Round}");
            
            // Возвращаем все карты в колоду
            int returned = _handService.ReturnAllCardsToDeck();
            Console.WriteLine($"Возвращено карт в колоду: {returned}");
            
            // Сбрасываем всю руку (очищаем HandComponent)
            _handService.DiscardHand(_run.HandEntity);
            Console.WriteLine("Рука сброшена");
            
            // Очищаем выбор карт
            _handService.ClearSelection(_run.HandEntity);
            
            // Берем новую руку до максимума
            var handInfo = _handService.GetHandInfo(_run.HandEntity);
            if (handInfo.HasValue)
            {
                int cardsToDraw = handInfo.Value.MaxHandSize;
                int drawn = _handService.DrawCards(_run.HandEntity, cardsToDraw, _run.Rng);
                Console.WriteLine($"Взято карт: {drawn}");
                
                // Сортируем руку при новом раунде
                _handService.SortHand(_run.HandEntity);
            }
        }
        else
        {
            // Внутри раунда - сбрасываем только сыгранные карты и добираем до максимума
            Console.WriteLine("\n--- Сброс сыгранных карт ---");
            
            var selectedCards = _handService.GetSelectedCards(_run.HandEntity);
            if (selectedCards.Count > 0)
            {
                // Сбрасываем только сыгранные карты
                int discarded = _handService.DiscardCards(_run.HandEntity, selectedCards);
                Console.WriteLine($"Сброшено карт: {discarded}");
                
                // Очищаем выбор
                _handService.ClearSelection(_run.HandEntity);
            }
            else
            {
                // Если игрались все карты в руке - сбрасываем всю руку
                _handService.DiscardHand(_run.HandEntity);
                Console.WriteLine("Все карты сброшены");
            }
            
            // Добираем карты до максимума в руке
            var handAfter = _handService.GetHandInfo(_run.HandEntity);
            if (handAfter.HasValue)
            {
                int cardsToDraw = handAfter.Value.AvailableSlots;
                if (cardsToDraw > 0)
                {
                    int drawn = _handService.DrawCards(_run.HandEntity, cardsToDraw, _run.Rng);
                    Console.WriteLine($"Добрано карт: {drawn}");
                    // НЕ сортируем - сортировка только при новом раунде
                }
            }
        }

        ShowRoundState();

        // Проверяем проигрыш после завершения руки
        _run.CheckGameOver();
        if (_run.State != Game.Domain.Run.RunState.Playing)
        {
            // Игра завершена (проигрыш)
            _run.StateMachine.EndTurn();
            return;
        }

        // Проверяем, можем ли продолжать играть
        if (!_run.RoundState.CanPlayHand && !_run.RoundState.CanDiscardHand && !_run.RoundState.IsComplete)
        {
            Console.WriteLine("\n⚠ Нет доступных действий! Вы не можете больше играть или сбрасывать руки.");
            Console.WriteLine("Игра завершена.");
            _run.StateMachine.EndTurn();
            return;
        }

        // Автоматически продолжаем игру
        _run.StateMachine.StartTurn();
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
                Console.WriteLine($"  - {CardFormatter.FormatCard(rank.Value.Rank, suit.Value.Suit)}");
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
                Console.WriteLine($"  {i + 1}. {CardFormatter.FormatCard(rank.Value.Rank, suit.Value.Suit)}{marker}");
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
                Console.WriteLine($"  - {CardFormatter.FormatCard(rank.Value.Rank, suit.Value.Suit)}");
            }
        }
    }

}











