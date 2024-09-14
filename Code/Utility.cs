namespace Chat_App.Code;

public static class Utility
{
    public static bool Null(object model)
    {
        if (model == null)
        {
            return true;
        }
        if (model is string)
        {
            return string.IsNullOrEmpty((string)model);
        }
        if (model is List<string>)
        {
            return ((List<string>)model).Count < 1;
        }
        if (model is int)
        {
            return (int)model == 0;
        }
        return false;
    }
}
