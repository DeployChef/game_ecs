using Game.ConsoleRunner.Baking;
using Game.ConsoleRunner.Content.Authoring;
using Game.ConsoleRunner.Content.CMS;
using Game.Domain.ECS;
using Game.Domain.ECS.Components;

namespace Game.ConsoleRunner;

class Program
{
    static void Main(string[] args)
    {
        // 1. Создаем World (домен)
        World world = new World();

        // 2. Загружаем контент из JSON (вне домена)
        IContentManager contentManager = new JsonContentManager();
        DeckAuthoring deckAuthoring = contentManager.LoadDeck("standard_deck");
        Console.WriteLine($"Загружена колода: {deckAuthoring.Cards.Count} карт");

        // 3. Baking - конвертация контента в Entity (вне домена)
        BakingSystem bakingSystem = new BakingSystem(world);
        bakingSystem.RegisterBaker(new DeckBaker());
        bakingSystem.Bake(deckAuthoring);
        Console.WriteLine("Baking завершен - Entity созданы в World\n");

        // 4. Создаем руку (домен)
        Entity handEntity = world.CreateEntity();
        world.AddComponent(handEntity, new HandComponent(maxHandSize: 8));

        // 5. Запускаем игровой цикл с фиксированным seed для воспроизводимой раздачи
        int seed = 12345; // Фиксированный seed - каждый запуск даст одинаковую раздачу
        GameLoop gameLoop = new GameLoop(world, handEntity, seed);
        gameLoop.Run();
    }
}
