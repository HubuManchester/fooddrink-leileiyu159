using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp;

[QueryProperty(nameof(ItemId), "id")]
public partial class EditItemPage : ContentPage
{
    private FoodItem? currentItem;

    public EditItemPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
    }

    public string ItemId
    {
        set => _ = LoadItemAsync(value);
    }

    private async Task LoadItemAsync(string id)
    {
        currentItem = await FoodCatalogService.GetByIdAsync(id);
        if (currentItem is null)
        {
            await DisplayAlert("Not found", "The record could not be loaded for editing.", "OK");
            await Shell.Current.GoToAsync("..");
            return;
        }

        NameEntry.Text = currentItem.Name;
        CategoryPicker.SelectedItem = currentItem.Category;
        DescriptionEditor.Text = currentItem.Description;
        CaloriesEntry.Text = currentItem.Calories.ToString();
        ProteinEntry.Text = currentItem.Protein.ToString();
        CarbsEntry.Text = currentItem.Carbs.ToString();
        FatEntry.Text = currentItem.Fat.ToString();
        AllergyEntry.Text = currentItem.AllergyNote;
    }

    private async void OnUpdateClicked(object? sender, EventArgs e)
    {
        if (currentItem is null)
            return;

        try
        {
            var validationMessage = ValidateForm(out var calories, out var protein, out var carbs, out var fat);
            if (validationMessage is not null)
            {
                ShowValidation(validationMessage);
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(250));
                return;
            }

            currentItem.Name = NameEntry.Text!.Trim();
            currentItem.Category = CategoryPicker.SelectedItem?.ToString() ?? "Snack";
            currentItem.Description = DescriptionEditor.Text!.Trim();
            currentItem.Calories = calories;
            currentItem.Protein = protein;
            currentItem.Carbs = carbs;
            currentItem.Fat = fat;
            currentItem.AllergyNote = string.IsNullOrWhiteSpace(AllergyEntry.Text)
                ? "No allergy note provided."
                : AllergyEntry.Text.Trim();
            currentItem.Tags = $"{NameEntry.Text} {CategoryPicker.SelectedItem} {DescriptionEditor.Text}";

            await FoodCatalogService.UpdateAsync(currentItem);
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            SemanticScreenReader.Announce("Food record updated.");

            await DisplayAlert("Updated", "The record has been updated successfully.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            ShowValidation($"The record could not be updated: {ex.Message}");
        }
    }

    private string? ValidateForm(out int calories, out int protein, out int carbs, out int fat)
    {
        calories = protein = carbs = fat = 0;

        if (string.IsNullOrWhiteSpace(NameEntry.Text))
            return "Please enter a food or drink name.";

        if (CategoryPicker.SelectedIndex < 0)
            return "Please choose a category.";

        if (string.IsNullOrWhiteSpace(DescriptionEditor.Text))
            return "Please add a short description.";

        return TryReadNumber(CaloriesEntry.Text, "calories", out calories)
            ?? TryReadNumber(ProteinEntry.Text, "protein", out protein)
            ?? TryReadNumber(CarbsEntry.Text, "carbs", out carbs)
            ?? TryReadNumber(FatEntry.Text, "fat", out fat);
    }

    private static string? TryReadNumber(string? value, string fieldName, out int number)
    {
        if (int.TryParse(value, out number) && number >= 0)
            return null;

        return $"Please enter a valid non-negative number for {fieldName}.";
    }

    private void ShowValidation(string message)
    {
        ValidationLabel.Text = message;
        ValidationPanel.IsVisible = true;
        SemanticScreenReader.Announce(message);
    }
}
