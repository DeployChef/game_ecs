using Game.Application.UseCases;
using Game.ConsoleRunner.Menu;
using Game.Domain.Services;
using Game.Domain.Run;
using Game.Infrastructure.Baking;
using Game.Infrastructure.Content.Authoring;
using Game.Infrastructure.Content.CMS;
using Game.Infrastructure.Random;

namespace Game.ConsoleRunner;

class Program
{
    static void Main(string[] args)
    {
        // 1. Инициализация инфраструктуры
        IContentManager contentManager = new JsonContentManager();
        var contentLoader = new DeckContentLoaderAdapter(contentManager);
        
        // 2. Инициализация доменных сервисов
        IRunService runService = new RunService();
        IHandService handService = new HandService(new Game.Domain.ECS.World()); // Временный World, будет заменен
        
        // 3. Инициализация Application use cases
        var startRunUseCase = new StartRunUseCase(runService);
        
        // 4. Меню выбора колоды
        var menu = new DeckSelectionMenu();
        string? deckId = menu.ShowAndGetSelection();
        
        if (deckId == null)
        {
            Console.WriteLine("Выход из игры");
            return;
        }
        
        // 5. Загрузка данных колоды (через доменный интерфейс)
        Console.WriteLine($"\nЗагрузка колоды: {deckId}...");
        var deckData = contentLoader.LoadDeck(deckId);
        Console.WriteLine($"Загружена колода: {deckData.Cards.Count} карт");
        
        // 6. Создаем World и выполняем Baking (инфраструктура)
        // Baking конвертирует данные в Entity
        var world = new Game.Domain.ECS.World();
        var deckAuthoring = ConvertToAuthoring(deckData); // Временная конвертация
        var bakingSystem = new BakingSystem(world);
        bakingSystem.RegisterBaker(new DeckBaker());
        bakingSystem.Bake(deckAuthoring);
        Console.WriteLine("Baking завершен - Entity созданы в World\n");
        
        // 7. Создаем ран через UseCase (Application)
        var run = startRunUseCase.Execute(deckId, world);
        Console.WriteLine($"Ран создан (seed: {run.Seed})\n");
        
        // 8. Запускаем игровой цикл (презентация)
        handService = new HandService(run.World);
        var gameLoop = new GameLoop(run, handService);
        gameLoop.Run();
    }
    
    // Временный метод для конвертации DeckContentData в DeckAuthoring
    // В будущем нужно будет переделать Baking чтобы работал с DeckContentData напрямую
    private static DeckAuthoring ConvertToAuthoring(Game.Domain.Content.DeckContentData deckData)
    {
        var authoring = new DeckAuthoring();
        foreach (var cardData in deckData.Cards)
        {
            authoring.Cards.Add(new CardAuthoring
            {
                Rank = cardData.Rank,
                Suit = cardData.Suit
            });
        }
        return authoring;
    }
}
