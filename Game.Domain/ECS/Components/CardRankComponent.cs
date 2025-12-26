using Game.Domain.Core;
using Game.Domain.ECS;

namespace Game.Domain.ECS.Components;

/// <summary>
/// Компонент, хранящий ранг карты.
/// 
/// Почему struct, а не class?
/// - Value type - быстрее, меньше аллокаций в памяти
/// - Компоненты обычно маленькие (одно-два поля)
/// - Копирование дешевое для маленьких структур
/// 
/// Почему только данные?
/// - В ECS компоненты = данные, системы = логика
/// - Системы будут читать/изменять этот компонент
/// - Нет методов - только данные
/// </summary>
public struct CardRankComponent : IComponent
{
    public CardRank Rank;

    public CardRankComponent(CardRank rank)
    {
        Rank = rank;
    }
}

