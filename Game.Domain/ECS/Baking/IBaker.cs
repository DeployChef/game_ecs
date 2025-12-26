namespace Game.Domain.ECS.Baking;

/// <summary>
/// Интерфейс для Baking систем - аналог IBaker<T> из Unity DOTS.
/// 
/// В Unity DOTS:
/// - IBaker<T> - интерфейс для конвертации Authoring → Entity
/// - BakerState - контекст для baking (World, Entity, и т.д.)
/// 
/// У нас:
/// - IBaker<T> - интерфейс для конвертации Authoring → Entity
/// - BakingContext - контекст для baking (World)
/// </summary>
public interface IBaker<T> where T : class
{
    /// <summary>
    /// Конвертирует Authoring данные в Entity/Component.
    /// Вызывается для каждого Authoring объекта.
    /// </summary>
    void Bake(BakingContext context, T authoring);
}

