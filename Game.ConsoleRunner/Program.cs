using Game.ConsoleRunner.Baking;
using Game.ConsoleRunner.Content.Authoring;
using Game.ConsoleRunner.Content.CMS;
using Game.Domain.Core;
using Game.Domain.ECS;
using Game.Domain.ECS.Components;
using Game.Domain.ECS.Systems;
using Game.Domain.Poker;

namespace Game.ConsoleRunner;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Balatro-like Card Game ===");
        Console.WriteLine();

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
        Console.WriteLine("Baking завершен - Entity созданы в World");

        // 4. Создаем руку (домен)
        Entity handEntity = world.CreateEntity();
        world.AddComponent(handEntity, new HandComponent(maxHandSize: 5));
        Console.WriteLine("Рука создана");

        // 5. Взятие карт (домен)
        DrawSystem drawSystem = new DrawSystem();
        int drawn = drawSystem.DrawCards(world, handEntity, 5);
        Console.WriteLine($"Взято карт: {drawn}");

        // 6. Показываем карты в руке
        var hand = world.GetComponent<HandComponent>(handEntity);
        if (hand.HasValue)
        {
            Console.WriteLine("\nКарты в руке:");
            foreach (var cardEntity in hand.Value.Cards)
            {
                var rank = world.GetComponent<CardRankComponent>(cardEntity);
                var suit = world.GetComponent<CardSuitComponent>(cardEntity);
                if (rank.HasValue && suit.HasValue)
                {
                    Console.WriteLine($"  - {rank.Value.Rank} {suit.Value.Suit}");
                }
            }
        }

        // 7. Оценка руки (домен)
        var result = HandEvaluator.Evaluate(world, handEntity);
        Console.WriteLine($"\nКомбинация: {result.HandType}");
        Console.WriteLine($"Базовые очки: {result.BaseScore}");

        // 8. Сброс карт (домен)
        DiscardSystem discardSystem = new DiscardSystem();
        discardSystem.DiscardHand(world, handEntity);
        Console.WriteLine("\nКарты сброшены");

        Console.WriteLine("\n=== Игра завершена ===");
    }
}
