using Game.Domain.Core;

namespace Game.Domain.Content.Authoring;

/// <summary>
/// Authoring данные карты - аналог MonoBehaviour компонента в Unity DOTS.
/// 
/// В Unity DOTS Authoring - это MonoBehaviour компоненты, созданные в редакторе.
/// У нас (без Unity) - это структуры данных, которые могут быть из JSON, кода, редактора.
/// 
/// Baking система (IBaker<CardAuthoring>) конвертирует эти данные в Entity/Component.
/// </summary>
public class CardAuthoring
{
    public CardRank Rank { get; set; }
    public CardSuit Suit { get; set; }
}
