using System.Resources;
using System.Reflection;

namespace FoodDrinkApp.Services;

public static class LocalizationService
{
    private static readonly ResourceManager ResourceManager = new(
        "FoodDrinkApp.Resources.Strings.AppResources",
        typeof(LocalizationService).Assembly);

    public static string GetString(string key)
    {
        try
        {
            return ResourceManager.GetString(key) ?? key;
        }
        catch
        {
            return key;
        }
    }

    public static string AppName => GetString("AppName");
    public static string AppSubtitle => GetString("AppSubtitle");
    public static string TabFoods => GetString("TabFoods");
    public static string TabHardware => GetString("TabHardware");
    public static string TabSettings => GetString("TabSettings");
}
