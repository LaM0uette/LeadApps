using System.Globalization;
using TCGPocketDex.Contracts.DTO;
using TCGPocketDex.Contracts.Request;
using TCGPocketDex.Domain.Models;
using TCGPocketDex.SDK.Mappings;

namespace TopDeck.Shared.Services.TCGPCard;

public class TCGPCardService : TCGPDexApiService, ITCGPCardService
{
    #region Statements

    private const string _route = "/cards";

    #endregion

    #region ITCGPCardService

    public async Task<List<Card>> GetAllAsync(string? cultureOverride = null, CancellationToken ct = default)
    {
        string culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        string urlParams = $"?lng={cultureOverride ?? culture}";
        
        List<CardOutputDTO> dtos = await GetAsync<List<CardOutputDTO>>($"/cards{urlParams}", ct);
        List<Card> cards = dtos.ToCards();
        
        return cards;
    }

    public async Task<Card?> GetByIdAsync(int id, string? cultureOverride = null, CancellationToken ct = default)
    {
        string culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        string urlParams = $"?lng={cultureOverride ?? culture}";

        CardOutputDTO? dto = await GetAsync<CardOutputDTO?>($"/cards/{id}{urlParams}", ct);
        
        if (dto is null) 
            return null;
        
        Card card = dto.ToCard();
        
        return card;
    }

    public async Task<List<Card>> GetByBatchAsync(DeckRequest deck, string? cultureOverride = null, CancellationToken ct = default)
    {
        string culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        string urlParams = $"?lng={cultureOverride ?? culture}";

        List<CardOutputDTO> dtos = await PostAsync<List<CardOutputDTO>>($"/cards/batch{urlParams}", deck, ct);
        return dtos.ToCards();
    }
    
    #endregion
}