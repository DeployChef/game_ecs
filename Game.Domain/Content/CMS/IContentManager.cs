using Game.Domain.Content.Authoring;

namespace Game.Domain.Content.CMS;

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
/// 
/// Почему интерфейс?
/// - Реализация может быть разной (JSON загрузка, Unity Editor, и т.д.)
/// - Домен не знает, откуда берется контент
/// - Легко тестировать (mock реализация)
/// </summary>
public interface IContentManager
{
    /// <summary>
    /// Загружает колоду из контента и возвращает в Authoring формате.
    /// Валидация происходит здесь.
    /// </summary>
    DeckAuthoring LoadDeck(string deckId);
}

