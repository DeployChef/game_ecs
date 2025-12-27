namespace Game.Domain.Content;

/// <summary>
/// Интерфейс для загрузки данных колоды (аналог ScriptableObject в Unity).
/// 
/// В Unity: ScriptableObject загружается через Resources.Load или Addressables.
/// У нас: JSON файлы загружаются через IDeckContentLoader.
/// 
/// Реализация находится в инфраструктуре (Game.ConsoleRunner),
/// но интерфейс в домене для использования доменными сервисами.
/// </summary>
public interface IDeckContentLoader
{
    /// <summary>
    /// Загружает данные колоды по ID.
    /// Возвращает DeckContentData - доменное представление данных колоды.
    /// </summary>
    DeckContentData LoadDeck(string deckId);
}

