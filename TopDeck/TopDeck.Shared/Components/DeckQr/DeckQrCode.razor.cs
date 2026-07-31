using DeckSharing;
using Localizer;
using Microsoft.AspNetCore.Components;
using QRCoder;

namespace TopDeck.Shared.Components;

public class DeckQrCodeBase : ComponentBase
{
    #region Statements

    private const int DEFAULT_PIXELS_PER_MODULE = 12;

    [Parameter, EditorRequired] public required IReadOnlyList<DeckCardCode> Cards { get; set; } = [];
    [Parameter] public int PixelsPerModule { get; set; } = DEFAULT_PIXELS_PER_MODULE;

    [Inject] private ILocalizer _localizer { get; set; } = null!;

    protected string? DataUri { get; private set; }
    protected string AltText => _localizer.Localize("component.deckQr.alt.text", "Deck code to scan");

    protected override void OnParametersSet()
    {
        if (Cards.Count <= 0)
        {
            DataUri = null;
            return;
        }

        string payload = DeckCode.Encode(Cards);

        using QRCodeGenerator generator = new();
        using QRCodeData data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.H);

        byte[] png = new PngByteQRCode(data).GetGraphic(PixelsPerModule);

        DataUri = $"data:image/png;base64,{Convert.ToBase64String(png)}";
    }

    #endregion
}
