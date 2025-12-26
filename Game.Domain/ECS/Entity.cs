namespace Game.Domain.ECS;

/// <summary>
/// Entity - это просто уникальный идентификатор сущности в ECS мире.
/// Не содержит данных, не содержит логики - только ID.
/// 
/// Почему struct, а не class?
/// - Entity - это value type, легковесный
/// - Меньше аллокаций в памяти
/// - Быстрее сравнение (по значению, а не по ссылке)
/// </summary>
public readonly struct Entity : IEquatable<Entity>
{
    public int Id { get; }

    public Entity(int id)
    {
        Id = id;
    }

    public bool Equals(Entity other)
    {
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return obj is Entity other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Id;
    }

    public static bool operator ==(Entity left, Entity right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Entity left, Entity right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return $"Entity({Id})";
    }
}

