using FoodDrinkApp.Services;

namespace FoodDrinkApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
        await LoadFoodItemsAsync(SearchFoodBar.Text);
    }

    private async Task LoadFoodItemsAsync(string? query = null)
    {
        var items = await FoodCatalogService.SearchAsync(query);
        FoodCollection.ItemsSource = items;
        UpdateStats(items);
    }

    private void UpdateStats(IReadOnlyList<Models.FoodItem> items)
    {
        StatsItemCountLabel.Text = $"{items.Count} items";
        StatsCaloriesLabel.Text = $"{items.Sum(i => i.Calories)} kcal";
        StatsProteinLabel.Text = $"{items.Sum(i => i.Protein)}g";
        StatsCarbsLabel.Text = $"{items.Sum(i => i.Carbs)}g";
    }

    private async void OnShareReportClicked(object? sender, EventArgs e)
    {
        try
        {
            var items = await FoodCatalogService.SearchAsync(null);
            await ShareService.ShareNutritionReportAsync(items);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Share failed", ex.Message, "OK");
        }
    }

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AddItemPage));
    }

    private async void OnDetailsClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string id)
        {
            await Shell.Current.GoToAsync($"{nameof(FoodDetailPage)}?id={Uri.EscapeDataString(id)}");
        }
    }

    private async void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        await LoadFoodItemsAsync(e.NewTextValue);
    }

    private async void OnSearchButtonPressed(object? sender, EventArgs e)
    {
        await LoadFoodItemsAsync(SearchFoodBar.Text);
    }

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        await LoadFoodItemsAsync(SearchFoodBar.Text);
        FoodRefreshView.IsRefreshing = false;
        SemanticScreenReader.Announce($"Food and drink list refreshed. Current source: {FoodCatalogService.DataSourceDescription}.");
    }

    private async void OnSwipeEditInvoked(object? sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is string id)
            await Shell.Current.GoToAsync($"{nameof(EditItemPage)}?id={Uri.EscapeDataString(id)}");
    }

    private async void OnSwipeDeleteInvoked(object? sender, EventArgs e)
    {
        if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is string id)
        {
            var item = await FoodCatalogService.GetByIdAsync(id);
            var confirm = await DisplayAlert(
                "Confirm delete",
                $"Delete \"{item?.Name ?? "this item"}\"? This cannot be undone.",
                "Delete", "Cancel");

            if (confirm)
            {
                await FoodCatalogService.DeleteAsync(id);
                HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
                await LoadFoodItemsAsync(SearchFoodBar.Text);
                SemanticScreenReader.Announce("Item deleted.");
            }
        }
    }
}
