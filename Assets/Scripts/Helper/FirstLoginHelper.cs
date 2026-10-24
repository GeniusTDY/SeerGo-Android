
public static class FirstLoginHelper 
{
    private static bool _isFirstLogin = false;
    public static void SetStatus(bool status)
    {
        _isFirstLogin = status;
    }

    internal static bool GetStatus()
    {
        return _isFirstLogin;
    }
}
