using Game.Application.UseCases;
using Game.Domain.Content;
using Game.Domain.Core;
using Game.Domain.ECS;
using Game.Domain.ECS.Components;
using Game.Domain.GameState;
using Game.Domain.Poker;
using Game.Domain.Run;
using Game.Domain.Services;
using Game.Infrastructure.Baking;
using Game.Infrastructure.Content.Authoring;
using Game.Infrastructure.Random;

namespace Game.UI.Services;

/// <summary>
/// Реализация сервиса для управления игровой сессией.
/// Scoped - одна игра на пользователя.
/// </summary>
public class GameSessionService : IGameSessionService
{
    private readonly IDeckContentLoader _contentLoader;
    private readonly IRunService _runService;
    private readonly StartRunUseCase _startRunUseCase;
    
    private Run? _currentRun;
    private IHandService? _handService;
    
    public bool IsGameActive => _currentRun != null && _currentRun.State == RunState.Playing;
    
    public GameSessionService(
        IDeckContentLoader contentLoader,
        IRunService runService,
        StartRunUseCase startRunUseCase)
    {
        _contentLoader = contentLoader ?? throw new ArgumentNullException(nameof(contentLoader));
        _runService = runService ?? throw new ArgumentNullException(nameof(runService));
        _startRunUseCase = startRunUseCase ?? throw new ArgumentNullException(nameof(startRunUseCase));
    }
    
    public bool StartNewGame(string deckId)
    {
        try
        {
            // 1. Загружаем данные колоды
            var deckData = _contentLoader.LoadDeck(deckId);
            
            // 2. Создаем World и выполняем Baking
            var world = new World();
            var deckAuthoring = ConvertToAuthoring(deckData);
            var bakingSystem = new BakingSystem(world);
            bakingSystem.RegisterBaker(new DeckBaker());
            bakingSystem.Bake(deckAuthoring);
            
            // 3. Создаем RNG
            int seed = new Random().Next();
            var rng = new SeededRandomNumberGenerator(seed);
            
            // 4. Создаем Run через UseCase
            _currentRun = _startRunUseCase.Execute(deckId, world, rng, seed);
            
            // 5. Создаем HandService с World из Run
            _handService = new HandService(_currentRun.World);
            
            // 6. Инициализируем первый ход
            _currentRun.StateMachine.StartTurn();
            
            // 7. Автоматически берем карты в руку
            HandleDrawingHand();
            
            // 8. Проверяем состояние после инициализации
            Console.WriteLine($"Run создан. State: {_currentRun.State}, CurrentState: {_currentRun.StateMachine.CurrentState}");
            Console.WriteLine($"IsGameActive: {IsGameActive}");
            
            return true;
        }
        catch (Exception ex)
        {
            // Логируем ошибку для отладки
            System.Diagnostics.Debug.WriteLine($"Ошибка при запуске игры: {ex}");
            Console.WriteLine($"Ошибка при запуске игры: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"InnerException: {ex.InnerException.Message}");
            }
            _currentRun = null;
            _handService = null;
            return false;
        }
    }
    
    public GameStateViewModel? GetGameState()
    {
        if (_currentRun == null || _handService == null)
            return null;
        
        var handInfo = GetHandInfo();
        var roundState = GetRoundState();
        var selectedCards = _handService.GetSelectedCards(_currentRun.HandEntity);
        
        return new GameStateViewModel
        {
            CurrentState = _currentRun.StateMachine.CurrentState,
            RunState = _currentRun.State,
            RoundState = roundState,
            HandInfo = handInfo,
            SelectedCards = selectedCards.ToList()
        };
    }
    
    public bool ToggleCardSelection(Entity cardEntity)
    {
        if (_currentRun == null || _handService == null)
            return false;
        
        if (_currentRun.StateMachine.CurrentState != GameStateType.SelectingCards)
            return false;
        
        return _handService.ToggleCardSelection(_currentRun.HandEntity, cardEntity);
    }
    
    public HandResultViewModel? PlaySelectedCards()
    {
        if (_currentRun == null || _handService == null)
            return null;
        
        if (_currentRun.State != RunState.Playing)
            return null;
        
        if (_currentRun.StateMachine.CurrentState != GameStateType.SelectingCards)
            return null;
        
        // Проверяем ограничение на количество сыгранных рук
        if (!_currentRun.RoundState.CanPlayHand)
        {
            _currentRun.CheckGameOver();
            return null;
        }
        
        var selectedCards = _handService.GetSelectedCards(_currentRun.HandEntity);
        if (selectedCards.Count == 0)
            return null;
        
        if (selectedCards.Count > 5)
            return null;
        
        // Переходим в состояние PlayingHand
        _currentRun.StateMachine.CardsSelected();
        
        // Оцениваем комбинацию
        var evaluationResult = HandEvaluator.Evaluate(_currentRun.World, _currentRun.HandEntity);
        
        // Вычисляем итоговые очки
        int totalScore = HandScoreCalculator.CalculateTotalScore(_currentRun.World, _currentRun.HandEntity, evaluationResult);
        
        // Вычисляем сумму очков карт для отображения
        int cardsScore = 0;
        foreach (var cardEntity in selectedCards)
        {
            cardsScore += CardScoreCalculator.GetCardScore(_currentRun.World, cardEntity);
        }
        
        // Добавляем очки к раунду
        try
        {
            _currentRun.AddHandScore(totalScore);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
        
        // Переходим в состояние HandComplete
        _currentRun.StateMachine.HandCompleted();
        
        return new HandResultViewModel
        {
            HandType = evaluationResult.HandType.ToString(),
            CardsScore = cardsScore,
            BaseScore = evaluationResult.BaseScore,
            Multiplier = evaluationResult.Multiplier,
            TotalScore = totalScore
        };
    }
    
    public bool DiscardSelectedCards()
    {
        if (_currentRun == null || _handService == null)
            return false;
        
        if (_currentRun.State != RunState.Playing)
            return false;
        
        if (_currentRun.StateMachine.CurrentState != GameStateType.SelectingCards)
            return false;
        
        // Проверяем ограничение на количество сброшенных рук
        if (!_currentRun.RoundState.CanDiscardHand)
        {
            _currentRun.CheckGameOver();
            return false;
        }
        
        var selectedCards = _handService.GetSelectedCards(_currentRun.HandEntity);
        if (selectedCards.Count == 0)
            return false;
        
        if (selectedCards.Count > 5)
            return false;
        
        // Сбрасываем карты
        int discarded = _handService.DiscardCards(_currentRun.HandEntity, selectedCards);
        
        // Увеличиваем счетчик сброшенных рук
        try
        {
            _currentRun.IncrementHandsDiscarded();
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        
        // Очищаем выбор
        _handService.ClearSelection(_currentRun.HandEntity);
        
        // Добираем карты до максимума
        var handAfter = _handService.GetHandInfo(_currentRun.HandEntity);
        if (handAfter.HasValue && handAfter.Value.AvailableSlots > 0)
        {
            int drawn = _handService.DrawCards(_currentRun.HandEntity, handAfter.Value.AvailableSlots, _currentRun.Rng);
            _handService.SortHand(_currentRun.HandEntity);
        }
        
        // Проверяем конец игры
        _currentRun.CheckGameOver();
        
        return true;
    }
    
    public HandInfoViewModel? GetHandInfo()
    {
        if (_currentRun == null || _handService == null)
            return null;
        
        var handInfo = _handService.GetHandInfo(_currentRun.HandEntity);
        if (!handInfo.HasValue)
            return null;
        
        var selectedCards = _handService.GetSelectedCards(_currentRun.HandEntity);
        var selectedSet = new HashSet<Entity>(selectedCards);
        
        var cards = new List<CardViewModel>();
        foreach (var cardEntity in handInfo.Value.Cards)
        {
            var rank = _currentRun.World.GetComponent<CardRankComponent>(cardEntity);
            var suit = _currentRun.World.GetComponent<CardSuitComponent>(cardEntity);
            var score = CardScoreCalculator.GetCardScore(_currentRun.World, cardEntity);
            
            if (rank.HasValue && suit.HasValue)
            {
                var (rankStr, suitSymbol, suitColor) = FormatCard(rank.Value.Rank, suit.Value.Suit);
                cards.Add(new CardViewModel
                {
                    Entity = cardEntity,
                    Rank = rankStr,
                    Suit = suit.Value.Suit.ToString(),
                    SuitSymbol = suitSymbol,
                    IsSelected = selectedSet.Contains(cardEntity),
                    Score = score,
                    SuitColor = suitColor
                });
            }
        }
        
        return new HandInfoViewModel
        {
            Cards = cards,
            MaxHandSize = handInfo.Value.MaxHandSize,
            CurrentCount = handInfo.Value.CurrentCount,
            AvailableSlots = handInfo.Value.AvailableSlots
        };
    }
    
    public RoundStateViewModel? GetRoundState()
    {
        if (_currentRun == null)
            return null;
        
        var roundState = _currentRun.RoundState;
        return new RoundStateViewModel
        {
            Ante = roundState.Ante.Value,
            Round = roundState.Round.Value,
            Score = roundState.Score,
            Goal = roundState.Goal,
            Remaining = roundState.Remaining,
            HandsPlayed = roundState.HandsPlayed,
            HandsDiscarded = roundState.HandsDiscarded,
            MaxHandsToPlay = RoundState.MaxHandsToPlay,
            MaxHandsToDiscard = RoundState.MaxHandsToDiscard,
            IsComplete = roundState.IsComplete,
            CanPlayHand = roundState.CanPlayHand,
            CanDiscardHand = roundState.CanDiscardHand
        };
    }
    
    /// <summary>
    /// Обрабатывает состояние DrawingHand - автоматически берет карты.
    /// </summary>
    private void HandleDrawingHand()
    {
        if (_currentRun == null || _handService == null)
            return;
        
        if (_currentRun.StateMachine.CurrentState != GameStateType.DrawingHand)
            return;
        
        var handInfo = _handService.GetHandInfo(_currentRun.HandEntity);
        if (!handInfo.HasValue)
        {
            _currentRun.Lose();
            return;
        }
        
        int cardsToDraw = handInfo.Value.AvailableSlots;
        if (cardsToDraw > 0)
        {
            int drawn = _handService.DrawCards(_currentRun.HandEntity, cardsToDraw, _currentRun.Rng);
            if (drawn > 0)
            {
                _handService.SortHand(_currentRun.HandEntity);
            }
        }
        
        _currentRun.StateMachine.CardsDrawn();
    }
    
    /// <summary>
    /// Обрабатывает состояние HandComplete - сбрасывает карты и переходит к следующему ходу.
    /// </summary>
    public void HandleHandComplete()
    {
        if (_currentRun == null || _handService == null)
            return;
        
        if (_currentRun.StateMachine.CurrentState != GameStateType.HandComplete)
            return;
        
        // Проверяем конец игры
        if (_currentRun.State != RunState.Playing)
        {
            _currentRun.StateMachine.EndTurn();
            return;
        }
        
        // Проверяем, был ли переход к новому раунду
        var currentRoundState = _currentRun.RoundState;
        var isNewRound = currentRoundState.Score == 0 && currentRoundState.HandsPlayed == 0 && currentRoundState.HandsDiscarded == 0;
        
        if (isNewRound)
        {
            // Новый раунд - сбрасываем всю руку
            _handService.DiscardHand(_currentRun.HandEntity);
            
            // Берем новую руку до максимума
            var handInfo = _handService.GetHandInfo(_currentRun.HandEntity);
            if (handInfo.HasValue)
            {
                int cardsToDraw = handInfo.Value.MaxHandSize;
                int drawn = _handService.DrawCards(_currentRun.HandEntity, cardsToDraw, _currentRun.Rng);
                _handService.SortHand(_currentRun.HandEntity);
            }
        }
        else
        {
            // Внутри раунда - сбрасываем только сыгранные карты
            var selectedCards = _handService.GetSelectedCards(_currentRun.HandEntity);
            if (selectedCards.Count > 0)
            {
                _handService.DiscardCards(_currentRun.HandEntity, selectedCards);
                _handService.ClearSelection(_currentRun.HandEntity);
            }
            else
            {
                // Если игрались все карты - сбрасываем всю руку
                _handService.DiscardHand(_currentRun.HandEntity);
            }
            
            // Добираем карты до максимума
            var handAfter = _handService.GetHandInfo(_currentRun.HandEntity);
            if (handAfter.HasValue && handAfter.Value.AvailableSlots > 0)
            {
                int drawn = _handService.DrawCards(_currentRun.HandEntity, handAfter.Value.AvailableSlots, _currentRun.Rng);
            }
        }
        
        // Проверяем проигрыш
        _currentRun.CheckGameOver();
        if (_currentRun.State != RunState.Playing)
        {
            _currentRun.StateMachine.EndTurn();
            return;
        }
        
        // Проверяем, можем ли продолжать играть
        if (!_currentRun.RoundState.CanPlayHand && !_currentRun.RoundState.CanDiscardHand && !_currentRun.RoundState.IsComplete)
        {
            _currentRun.StateMachine.EndTurn();
            return;
        }
        
        // Автоматически продолжаем игру
        _currentRun.StateMachine.StartTurn();
        HandleDrawingHand();
    }
    
    /// <summary>
    /// Конвертирует DeckContentData в DeckAuthoring для Baking.
    /// </summary>
    private static DeckAuthoring ConvertToAuthoring(DeckContentData deckData)
    {
        var authoring = new DeckAuthoring();
        foreach (var cardData in deckData.Cards)
        {
            authoring.Cards.Add(new CardAuthoring
            {
                Rank = cardData.Rank,
                Suit = cardData.Suit,
                BaseScore = cardData.BaseScore
            });
        }
        return authoring;
    }
    
    /// <summary>
    /// Форматирует карту для отображения.
    /// </summary>
    private static (string rank, string suitSymbol, string suitColor) FormatCard(CardRank rank, CardSuit suit)
    {
        string rankStr = rank switch
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
        
        string suitSymbol = suit switch
        {
            CardSuit.Spades => "♠",
            CardSuit.Hearts => "♥",
            CardSuit.Diamonds => "♦",
            CardSuit.Clubs => "♣",
            _ => suit.ToString()
        };
        
        string suitColor = (suit == CardSuit.Hearts || suit == CardSuit.Diamonds) ? "red" : "black";
        
        return (rankStr, suitSymbol, suitColor);
    }
}

