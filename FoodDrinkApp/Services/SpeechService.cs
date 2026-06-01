namespace FoodDrinkApp.Services;

public static class SpeechService
{
    private static CancellationTokenSource? currentSpeech;

    public static async Task SpeakAsync(string text)
    {
        Stop();

        currentSpeech = new CancellationTokenSource();
        var options = new SpeechOptions
        {
            Volume = 0.9f,
            Pitch = 1.05f,
            Locale = await FindEnglishLocaleAsync()
        };

        try
        {
            await TextToSpeech.Default.SpeakAsync(text, options, currentSpeech.Token);
        }
        catch (OperationCanceledException)
        {
        }
    }

    public static async Task SpeakChineseAsync(string text)
    {
        Stop();

        currentSpeech = new CancellationTokenSource();
        var options = new SpeechOptions
        {
            Volume = 0.9f,
            Pitch = 1.08f,
            Locale = await FindChineseLocaleAsync()
        };

        try
        {
            await TextToSpeech.Default.SpeakAsync(text, options, currentSpeech.Token);
        }
        catch (OperationCanceledException)
        {
        }
    }

    public static void Stop()
    {
        if (currentSpeech is null)
            return;

        currentSpeech.Cancel();
        currentSpeech.Dispose();
        currentSpeech = null;
    }

    private static async Task<Locale?> FindEnglishLocaleAsync()
    {
        var locales = await TextToSpeech.Default.GetLocalesAsync();
        return locales.FirstOrDefault(locale => locale.Language.StartsWith("en", StringComparison.OrdinalIgnoreCase));
    }

    private static async Task<Locale?> FindChineseLocaleAsync()
    {
        var locales = await TextToSpeech.Default.GetLocalesAsync();
        return locales.FirstOrDefault(locale =>
            locale.Language.StartsWith("zh", StringComparison.OrdinalIgnoreCase) ||
            locale.Language.StartsWith("cmn", StringComparison.OrdinalIgnoreCase));
    }
}
