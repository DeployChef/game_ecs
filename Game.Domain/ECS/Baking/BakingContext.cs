using Game.Domain.ECS;

namespace Game.Domain.ECS.Baking;

/// <summary>
/// Контекст для Baking - аналог BakerState из Unity DOTS.
/// 
/// В Unity DOTS BakerState предоставляет:
/// - GetEntity() - получить Entity для Authoring объекта
/// - AddComponent<T>() - добавить компонент
/// - World - доступ к World
/// 
/// У нас BakingContext предоставляет:
/// - CreateEntity() - создать Entity
/// - World - доступ к World
/// </summary>
public class BakingContext
{
    public World World { get; }

    public BakingContext(World world)
    {
        World = world ?? throw new ArgumentNullException(nameof(world));
    }

    /// <summary>
    /// Создает новую Entity в World.
    /// </summary>
    public Entity CreateEntity()
    {
        return World.CreateEntity();
    }

    /// <summary>
    /// Добавляет компонент к Entity.
    /// </summary>
    public void AddComponent<T>(Entity entity, T component) where T : struct, IComponent
    {
        World.AddComponent(entity, component);
    }
}

