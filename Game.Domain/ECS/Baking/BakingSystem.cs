using Game.Domain.ECS.Baking;

namespace Game.Domain.ECS.Baking;

/// <summary>
/// Система для выполнения Baking - аналог BakingSystem из Unity DOTS.
/// 
/// В Unity DOTS Baking происходит автоматически при сборке.
/// У нас - вызывается явно через Bake<T>().
/// 
/// Регистрирует IBaker<T> и выполняет baking для Authoring объектов.
/// </summary>
public class BakingSystem
{
    private readonly Dictionary<Type, object> _bakers = new();
    private readonly World _world;

    public BakingSystem(World world)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
    }

    /// <summary>
    /// Регистрирует Baker для определенного типа Authoring.
    /// </summary>
    public void RegisterBaker<T>(IBaker<T> baker) where T : class
    {
        _bakers[typeof(T)] = baker;
    }

    /// <summary>
    /// Выполняет Baking для Authoring объекта.
    /// Находит зарегистрированный IBaker<T> и вызывает его Bake().
    /// </summary>
    public void Bake<T>(T authoring) where T : class
    {
        if (authoring == null)
            throw new ArgumentNullException(nameof(authoring));

        if (!_bakers.TryGetValue(typeof(T), out var bakerObj))
        {
            throw new InvalidOperationException($"No baker registered for type {typeof(T).Name}");
        }

        var baker = (IBaker<T>)bakerObj;
        var context = new BakingContext(_world);
        baker.Bake(context, authoring);
    }
}

