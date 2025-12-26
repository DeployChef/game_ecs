namespace Game.Domain.ECS;

/// <summary>
/// Базовый интерфейс для всех систем в ECS.
/// 
/// Почему интерфейс, а не абстрактный класс?
/// - Системы могут быть struct (value types) для производительности
/// - Интерфейс позволяет использовать и class, и struct
/// 
/// Почему Update принимает World?
/// - Система не хранит World - это dependency injection
/// - Одна система может работать с разными World (для тестирования)
/// - World передается явно - нет скрытых зависимостей
/// </summary>
public interface ISystem
{
    /// <summary>
    /// Выполняется каждый кадр/тик игры.
    /// Система получает World и работает с Entity через него.
    /// </summary>
    void Update(World world);
}

