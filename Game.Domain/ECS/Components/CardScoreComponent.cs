using Game.Domain.ECS;

namespace Game.Domain.ECS.Components;

/// <summary>
/// Компонент, хранящий базовые очки карты.
/// 
/// Базовые очки берутся из контента (JSON), а не вычисляются из ранга.
/// Это позволяет гибко настраивать очки для каждой карты.
/// 
/// Почему отдельный компонент, а не часть CardRankComponent?
/// - Разделение ответственности: ранг - это данные карты, очки - это игровая механика
/// - Можно изменять очки карты независимо от ранга (бафы, модификаторы)
/// - Легко добавлять/удалять компонент очков без изменения ранга
/// </summary>
public struct CardScoreComponent : IComponent
{
    /// <summary>
    /// Базовые очки карты (из контента)
    /// </summary>
    public int BaseScore;

    public CardScoreComponent(int baseScore)
    {
        BaseScore = baseScore;
    }
}

