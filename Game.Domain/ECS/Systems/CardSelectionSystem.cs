using Game.Domain.ECS.Components;

namespace Game.Domain.ECS.Systems;

/// <summary>
/// Система для выбора карт из руки для игры.
/// 
/// Позволяет игроку выбрать карты, которые будут использованы для оценки комбинации.
/// </summary>
public class CardSelectionSystem : ISystem
{
    /// <summary>
    /// Выбирает или снимает выбор с карты (toggle).
    /// Если карта уже выбрана - снимает выбор, если не выбрана - выбирает.
    /// </summary>
    public bool ToggleCardSelection(World world, Entity handEntity, Entity cardEntity)
    {
        var hand = world.GetComponent<HandComponent>(handEntity);
        if (!hand.HasValue)
            return false;

        // Проверяем, что карта в руке
        if (!hand.Value.Cards.Contains(cardEntity))
            return false;

        // Получаем или создаем SelectedCardsComponent
        var selected = world.GetComponent<SelectedCardsComponent>(handEntity);
        if (!selected.HasValue)
        {
            selected = new SelectedCardsComponent();
        }

        var selectedComponent = selected.Value;

        // Toggle логика: если карта уже выбрана - снимаем выбор, иначе - выбираем
        if (selectedComponent.SelectedCards.Contains(cardEntity))
        {
            // Снимаем выбор
            selectedComponent.SelectedCards.Remove(cardEntity);
            world.AddComponent(handEntity, selectedComponent);
            return true;
        }
        else
        {
            // Проверяем лимит (максимум 5 карт)
            if (selectedComponent.SelectedCards.Count >= 5)
                return false; // Достигнут лимит

            // Добавляем карту в выбранные
            selectedComponent.SelectedCards.Add(cardEntity);
            world.AddComponent(handEntity, selectedComponent);
            return true;
        }
    }

    /// <summary>
    /// Выбирает карту из руки (старый метод для обратной совместимости).
    /// </summary>
    [Obsolete("Use ToggleCardSelection instead")]
    public bool SelectCard(World world, Entity handEntity, Entity cardEntity)
    {
        return ToggleCardSelection(world, handEntity, cardEntity);
    }

    /// <summary>
    /// Отменяет выбор карты.
    /// </summary>
    public bool DeselectCard(World world, Entity handEntity, Entity cardEntity)
    {
        var selected = world.GetComponent<SelectedCardsComponent>(handEntity);
        if (!selected.HasValue)
            return false;

        var selectedComponent = selected.Value;
        if (!selectedComponent.SelectedCards.Remove(cardEntity))
            return false;

        world.AddComponent(handEntity, selectedComponent);
        return true;
    }

    /// <summary>
    /// Очищает все выбранные карты.
    /// </summary>
    public void ClearSelection(World world, Entity handEntity)
    {
        var selected = world.GetComponent<SelectedCardsComponent>(handEntity);
        if (selected.HasValue)
        {
            var selectedComponent = selected.Value;
            selectedComponent.SelectedCards.Clear();
            world.AddComponent(handEntity, selectedComponent);
        }
    }

    public void Update(World world)
    {
        // Selection вызывается явно через SelectCard/DeselectCard
    }
}

