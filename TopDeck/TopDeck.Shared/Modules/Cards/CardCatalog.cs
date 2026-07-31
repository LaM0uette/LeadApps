using System.Globalization;
using System.Text;

namespace Cards;

public class CardCatalog
{
    #region Statements

    private const string CSV_PATH = "_content/TopDeck.Shared/data/cards.csv";
    private const string TRAINER_TYPE = "Trainer";
    private const string PROMO_SHORT_PREFIX = "P-";
    private const string PROMO_LONG_PREFIX = "PROMO-";
    private const int COLUMN_COUNT = 10;
    private const int TYPE_COLUMN = 0;
    private const int F3_COLUMN = 1;
    private const int NAME_COLUMN = 2;
    private const int SET_COLUMN = 3;
    private const int NUMBER_COLUMN = 4;

    public bool IsLoaded { get; private set; }
    public IReadOnlyList<CardInfo> Cards => _cards;

    private readonly SemaphoreSlim _loadLock = new(1, 1);
    private readonly List<CardInfo> _cards = [];
    private readonly Dictionary<string, CardInfo> _bySetAndNumber = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, CardInfo> _byName = new(StringComparer.OrdinalIgnoreCase);

    #endregion

    #region Methods

    public async Task EnsureLoadedAsync(HttpClient http, CancellationToken ct = default)
    {
        if (IsLoaded)
            return;

        await _loadLock.WaitAsync(ct);

        try
        {
            if (IsLoaded)
                return;

            string csv = await http.GetStringAsync(CSV_PATH, ct);
            Parse(csv);

            IsLoaded = true;
        }
        catch (Exception)
        {
            _cards.Clear();
            _bySetAndNumber.Clear();
            _byName.Clear();
        }
        finally
        {
            _loadLock.Release();
        }
    }

    public CardInfo? Find(string set, int number)
    {
        return _bySetAndNumber.GetValueOrDefault(BuildKey(set, number));
    }

    public CardInfo? Find(string name)
    {
        return _byName.GetValueOrDefault(name.Trim());
    }

    public bool IsTrainer(CardInfo card)
    {
        return string.Equals(card.Type, TRAINER_TYPE, StringComparison.OrdinalIgnoreCase);
    }


    private void Parse(string csv)
    {
        string[] lines = csv.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim('\r', ' ');

            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] fields = SplitCsvLine(line);

            if (fields.Length < COLUMN_COUNT)
                continue;

            if (!int.TryParse(fields[F3_COLUMN], NumberStyles.Integer, CultureInfo.InvariantCulture, out int f3))
                continue;

            if (!int.TryParse(fields[NUMBER_COLUMN], NumberStyles.Integer, CultureInfo.InvariantCulture, out int number))
                continue;

            CardInfo card = new(
                fields[TYPE_COLUMN].Trim(),
                f3,
                fields[NAME_COLUMN].Trim(),
                fields[SET_COLUMN].Trim(),
                number);

            _cards.Add(card);
            _bySetAndNumber.TryAdd(BuildKey(card.Set, card.Number), card);
            _byName.TryAdd(card.Name, card);
        }
    }

    private static string[] SplitCsvLine(string line)
    {
        List<string> fields = [];
        StringBuilder current = new();
        bool inQuotes = false;

        foreach (char c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (c == ',' && !inQuotes)
            {
                fields.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(c);
        }

        fields.Add(current.ToString());

        return fields.ToArray();
    }

    private static string BuildKey(string set, int number)
    {
        return $"{NormalizeSet(set)}:{number}";
    }

    private static string NormalizeSet(string set)
    {
        string normalized = set.Trim().ToUpperInvariant();

        if (!normalized.StartsWith(PROMO_SHORT_PREFIX, StringComparison.Ordinal))
            return normalized;

        return $"{PROMO_LONG_PREFIX}{normalized[PROMO_SHORT_PREFIX.Length..]}";
    }

    #endregion
}
