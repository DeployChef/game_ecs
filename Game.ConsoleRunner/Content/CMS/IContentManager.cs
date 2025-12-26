using Game.ConsoleRunner.Content.Authoring;

namespace Game.ConsoleRunner.Content.CMS;

/// <summary>
/// Интерфейс для CMS (Content Management System).
/// 
/// В Unity DOTS контент создается в редакторе через MonoBehaviour.
/// У нас (без Unity) контент загружается из JSON, кода, или редактора.
/// 
/// CMS отвечает за:
/// - Загрузку контента (из JSON, редактора, и т.д.)
/// - Валидацию данных
/// - Возврат Authoring объектов
/// </summary>
public interface IContentManager
{
    /// <summary>
    /// Загружает колоду из контента и возвращает в Authoring формате.
    /// Валидация происходит здесь.
    /// </summary>
    DeckAuthoring LoadDeck(string deckId);
}

