namespace Helpers.Auth0;

public static class Auth0SubHelper
{
    public static bool TryParse(string? sub, out string provider, out string id)
    {
        provider = id = string.Empty;
        
        if (string.IsNullOrWhiteSpace(sub)) 
            return false;
        
        int i = sub.LastIndexOf('|');
        
        if (i < 0 || i == sub.Length - 1) 
            return false;
        
        provider = sub[..i];
        id = sub[(i + 1)..];
        
        return true;
    }
}