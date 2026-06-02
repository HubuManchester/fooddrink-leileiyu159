namespace FoodDrinkApp.Services;

public static class SpeechService
{
    private static CancellationTokenSource? currentSpeech;
    private static Locale? cachedEnglishLocale;
    private static Locale? cachedFallbackLocale;
    private static bool localesLoaded;

    /// <summary>
    /// Check whether any TTS voice is available on this device.
    /// Call this once (e.g. on app startup) to warm the locale cache.
    /// </summary>
    public static async Task<bool> IsAvailableAsync()
    {
        await EnsureLocalesAsync();
        return cachedEnglishLocale is not null || cachedFallbackLocale is not null;
    }

    public static async Task SpeakAsync(string text)
    {
        Stop();
        await EnsureLocalesAsync();

        // Prefer English; fall back to any available voice
        var locale = cachedEnglishLocale ?? cachedFallbackLocale;

        if (locale is null)
            throw new InvalidOperationException(
                "No text-to-speech voice is installed on this device. " +
                "On Android, open Settings → System → Languages & input → Text-to-speech output, " +
                "then install voice data for your preferred language.");

        currentSpeech = new CancellationTokenSource();
        var options = new SpeechOptions
        {
            Volume = 0.9f,
            Pitch = 1.05f,
            Locale = locale
        };

        try
        {
            await TextToSpeech.Default.SpeakAsync(text, options, currentSpeech.Token);
        }
        catch (OperationCanceledException)
        {
            // intentionally swallowed — user pressed Stop
        }
    }

    public static async Task SpeakChineseAsync(string text)
    {
        Stop();
        await EnsureLocalesAsync();

        var locale = cachedFallbackLocale ?? cachedEnglishLocale;

        if (locale is null)
            throw new InvalidOperationException(
                "No text-to-speech voice is installed on this device. " +
                "On Android, open Settings → System → Languages & input → Text-to-speech output, " +
                "then install voice data for your preferred language.");

        currentSpeech = new CancellationTokenSource();
        var options = new SpeechOptions
        {
            Volume = 0.9f,
            Pitch = 1.08f,
            Locale = locale
        };

        try
        {
            await TextToSpeech.Default.SpeakAsync(text, options, currentSpeech.Token);
        }
        catch (OperationCanceledException)
        {
            // intentionally swallowed — user pressed Stop
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

    private static async Task EnsureLocalesAsync()
    {
        if (localesLoaded)
            return;

        try
        {
            var locales = await TextToSpeech.Default.GetLocalesAsync();

            if (locales is not null)
            {
                foreach (var locale in locales)
                {
                    var lang = locale.Language ?? string.Empty;

                    if (cachedEnglishLocale is null &&
                        lang.StartsWith("en", StringComparison.OrdinalIgnoreCase))
                    {
                        cachedEnglishLocale = locale;
                    }

                    if (cachedFallbackLocale is null &&
                        (lang.StartsWith("zh", StringComparison.OrdinalIgnoreCase) ||
                         lang.StartsWith("cmn", StringComparison.OrdinalIgnoreCase)))
                    {
                        cachedFallbackLocale = locale;
                    }
                }

                // If no Chinese voice either, pick the first available voice as ultimate fallback
                cachedFallbackLocale ??= locales.FirstOrDefault();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"TTS locale discovery failed: {ex.Message}");
        }

        localesLoaded = true;
    }
}
