using Game.Domain.Content;
using Game.Infrastructure.Content.Authoring;
using Game.Infrastructure.Content.CMS;

namespace Game.Infrastructure.Content.CMS;

/// <summary>
/// Адаптер, который конвертирует IContentManager (инфраструктура) в IDeckContentLoader (домен).
/// 
/// Это мост между инфраструктурой (JSON загрузка) и доменом (RunService).
/// </summary>
public class DeckContentLoaderAdapter : IDeckContentLoader
{
    private readonly IContentManager _contentManager;
    
    public DeckContentLoaderAdapter(IContentManager contentManager)
    {
        _contentManager = contentManager ?? throw new ArgumentNullException(nameof(contentManager));
    }
    
    public DeckContentData LoadDeck(string deckId)
    {
        // Загружаем через инфраструктурный ContentManager
        var deckAuthoring = _contentManager.LoadDeck(deckId);
        
        // Конвертируем в доменное представление
        var deckData = new DeckContentData();
        foreach (var cardAuthoring in deckAuthoring.Cards)
        {
            deckData.Cards.Add(new CardContentData
            {
                Rank = cardAuthoring.Rank,
                Suit = cardAuthoring.Suit,
                BaseScore = cardAuthoring.BaseScore
            });
        }
        
        return deckData;
    }
}

