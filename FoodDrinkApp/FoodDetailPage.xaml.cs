using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp;

[QueryProperty(nameof(ItemId), "id")]
public partial class FoodDetailPage : ContentPage
{
    private FoodItem? currentItem;

    public FoodDetailPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
    }

    protected override void OnDisappearing()
    {
        SpeechService.Stop();
        base.OnDisappearing();
    }

    public string ItemId
    {
        set => _ = LoadItemAsync(value);
    }

    private async Task LoadItemAsync(string id)
    {
        currentItem = await FoodCatalogService.GetByIdAsync(id);
        BindingContext = currentItem;
        RenderItem();
    }

    private void RenderItem()
    {
        if (currentItem is null)
        {
            NameLabel.Text = "Record not found";
            DescriptionLabel.Text = "The selected food or drink could not be loaded.";
            return;
        }

        NameLabel.Text = currentItem.Name;
        CategoryLabel.Text = currentItem.Category;
        CaloriesLabel.Text = currentItem.CaloriesLabel;
        MacroLabel.Text = currentItem.MacroSummary;
        DescriptionLabel.Text = currentItem.Description;
        AllergyLabel.Text = currentItem.AllergyNote;
        SemanticProperties.SetDescription(NameLabel, currentItem.AccessibleSummary);
    }

    private async void OnSpeakClicked(object? sender, EventArgs e)
    {
        if (currentItem is null)
        {
            await DisplayAlert("Missing record", "There is no nutrition summary to read.", "OK");
            return;
        }

        try
        {
            if (!await SpeechService.IsAvailableAsync())
            {
                await DisplayAlert(
                    "TTS not available",
                    "No text-to-speech voice found on this device.\n\n" +
                    "On Android, go to:\nSettings → System → Languages & input → Text-to-speech output → " +
                    "install voice data (e.g. Google TTS or manufacturer engine).",
                    "OK");
                return;
            }

            await SpeechService.SpeakAsync(currentItem.AccessibleSummary);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Text to speech unavailable", ex.Message, "OK");
        }
    }

    private void OnStopSpeechClicked(object? sender, EventArgs e)
    {
        SpeechService.Stop();
        SemanticScreenReader.Announce("Reading stopped.");
    }

    private async void OnEditClicked(object? sender, EventArgs e)
    {
        if (currentItem is null)
            return;

        await Shell.Current.GoToAsync($"{nameof(EditItemPage)}?id={Uri.EscapeDataString(currentItem.Id)}");
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (currentItem is null)
            return;

        var confirm = await DisplayAlert(
            "Confirm delete",
            $"Are you sure you want to delete \"{currentItem.Name}\"? This cannot be undone.",
            "Delete", "Cancel");

        if (!confirm)
            return;

        try
        {
            await FoodCatalogService.DeleteAsync(currentItem.Id);
            HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
            SemanticScreenReader.Announce($"Deleted {currentItem.Name}.");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Delete failed", ex.Message, "OK");
        }
    }

    private async void OnShareClicked(object? sender, EventArgs e)
    {
        if (currentItem is null)
            return;

        try
        {
            await ShareService.ShareFoodItemAsync(currentItem);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Share failed", ex.Message, "OK");
        }
    }

    private async void OnVibrateClicked(object? sender, EventArgs e)
    {
        try
        {
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(500));
            HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
            await DisplayAlert("Reminder", "Vibration feedback has been triggered.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Vibration unavailable", ex.Message, "OK");
        }
    }
}
