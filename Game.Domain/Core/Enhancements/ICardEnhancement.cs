namespace Game.Domain.Core.Enhancements;

/// <summary>
/// Интерфейс для улучшений карты (золотая, фольгированная и т.д.)
/// Точка расширения для будущих механик
/// </summary>
public interface ICardEnhancement
{
    string Name { get; }
}

