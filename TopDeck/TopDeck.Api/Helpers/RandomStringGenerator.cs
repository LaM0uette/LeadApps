using System.Security.Cryptography;

namespace Helpers.Generators;

public static class RandomStringGeneratorHelper
{
    #region Statements

    private const string _allowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    #endregion

    #region Methods

    public static string Generate(int length = 6)
    {
        byte[] buffer = new byte[length];
        RandomNumberGenerator.Fill(buffer);

        char[] result = new char[length];
        
        for (int i = 0; i < length; i++)
        {
            result[i] = _allowedCharacters[buffer[i] % _allowedCharacters.Length];
        }

        return new string(result);
    }

    #endregion
}