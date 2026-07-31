namespace DeckSharing;

public static class DeckCode
{
    #region Statements

    private const int TRAINER_OFFSET = 10_000_000;
    private const int CARD_IDENTITY_MODULO = 1_000_000;
    private const byte TRAILER_FIRST_BYTE = 0x01;
    private const byte TRAILER_SECOND_BYTE = 0x07;
    private const int IMAGE_NAME_F3_INDEX = 2;

    #endregion

    #region Methods

    public static string Encode(IEnumerable<DeckCardCode> cards)
    {
        List<DeckCardCode> all = cards.ToList();
        List<DeckCardCode> trainers = all.Where(c => c.IsTrainer).ToList();
        List<DeckCardCode> pokemon = all.Where(c => !c.IsTrainer).ToList();

        List<byte> bytes = [(byte)trainers.Count];

        foreach (DeckCardCode card in trainers)
            Add24(bytes, TRAINER_OFFSET + card.F3);

        bytes.Add((byte)pokemon.Count);

        foreach (DeckCardCode card in pokemon)
            Add24(bytes, card.F3);

        bytes.Add(TRAILER_FIRST_BYTE);
        bytes.Add(TRAILER_SECOND_BYTE);

        return Convert.ToBase64String(bytes.ToArray());
    }

    public static (List<int> Trainers, List<int> Pokemon) Decode(string base64)
    {
        byte[] raw = Convert.FromBase64String(base64);
        int index = 0;

        List<int> trainers = ReadList(raw, ref index);
        List<int> pokemon = ReadList(raw, ref index);

        return (trainers, pokemon);
    }

    public static int F3FromImage(string imageName)
    {
        return int.Parse(imageName.Split('_')[IMAGE_NAME_F3_INDEX]);
    }


    private static List<int> ReadList(byte[] raw, ref int index)
    {
        int count = raw[index++];
        List<int> values = new(count);

        for (int i = 0; i < count; i++)
        {
            int value = (raw[index] << 16) | (raw[index + 1] << 8) | raw[index + 2];
            index += 3;

            values.Add(value % CARD_IDENTITY_MODULO);
        }

        return values;
    }

    private static void Add24(List<byte> bytes, int value)
    {
        bytes.Add((byte)(value >> 16));
        bytes.Add((byte)(value >> 8));
        bytes.Add((byte)value);
    }

    #endregion
}
