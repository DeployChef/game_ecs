using System.Collections;

namespace Game.Domain.ECS;

/// <summary>
/// World - центральный контейнер для всех Entity и их компонентов.
/// 
/// Почему Dictionary, а не массив?
/// - Entity ID могут быть разрозненными (после удаления Entity)
/// - Dictionary позволяет эффективно находить компоненты по Entity
/// 
/// Почему Dictionary<Type, IComponent> для каждого Entity?
/// - Один Entity может иметь несколько компонентов разных типов
/// - Type используется как ключ для быстрого поиска компонента
/// - IComponent - базовый тип для всех компонентов
/// </summary>
public class World
{
    private int _nextEntityId = 1;
    
    // Хранилище: Entity -> Dictionary<Type компонента, сам компонент>
    private readonly Dictionary<Entity, Dictionary<Type, IComponent>> _components;

    public World()
    {
        _components = new Dictionary<Entity, Dictionary<Type, IComponent>>();
    }

    /// <summary>
    /// Создает новую Entity с уникальным ID.
    /// Entity пока пустая (без компонентов).
    /// </summary>
    public Entity CreateEntity()
    {
        var entity = new Entity(_nextEntityId++);
        _components[entity] = new Dictionary<Type, IComponent>();
        return entity;
    }

    /// <summary>
    /// Добавляет компонент к Entity.
    /// Если компонент такого типа уже есть - заменяет его.
    /// </summary>
    public void AddComponent<T>(Entity entity, T component) where T : struct, IComponent
    {
        if (!_components.ContainsKey(entity))
        {
            throw new ArgumentException($"Entity {entity} does not exist in this World");
        }

        _components[entity][typeof(T)] = component;
    }

    /// <summary>
    /// Получает компонент определенного типа у Entity.
    /// Возвращает null, если компонента нет.
    /// </summary>
    public T? GetComponent<T>(Entity entity) where T : struct, IComponent
    {
        if (!_components.TryGetValue(entity, out var entityComponents))
        {
            return null;
        }

        if (entityComponents.TryGetValue(typeof(T), out var component))
        {
            return (T)component;
        }

        return null;
    }

    /// <summary>
    /// Проверяет, есть ли у Entity компонент определенного типа.
    /// </summary>
    public bool HasComponent<T>(Entity entity) where T : struct, IComponent
    {
        return GetComponent<T>(entity).HasValue;
    }

    /// <summary>
    /// Удаляет компонент определенного типа у Entity.
    /// </summary>
    public void RemoveComponent<T>(Entity entity) where T : struct, IComponent
    {
        if (_components.TryGetValue(entity, out var entityComponents))
        {
            entityComponents.Remove(typeof(T));
        }
    }

    /// <summary>
    /// Удаляет Entity и все ее компоненты.
    /// </summary>
    public void DestroyEntity(Entity entity)
    {
        _components.Remove(entity);
    }

    /// <summary>
    /// Находит все Entity, у которых есть компонент определенного типа.
    /// Используется системами для поиска Entity по компонентам.
    /// </summary>
    public IEnumerable<Entity> GetEntitiesWith<T>() where T : struct, IComponent
    {
        foreach (var (entity, components) in _components)
        {
            if (components.ContainsKey(typeof(T)))
            {
                yield return entity;
            }
        }
    }

    /// <summary>
    /// Получает все Entity в World.
    /// </summary>
    public IEnumerable<Entity> GetAllEntities()
    {
        return _components.Keys;
    }
}

