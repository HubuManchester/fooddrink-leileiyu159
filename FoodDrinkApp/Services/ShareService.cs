using FoodDrinkApp.Models;

namespace FoodDrinkApp.Services;

public static class ShareService
{
    public static async Task ShareFoodItemAsync(FoodItem item)
    {
        var text = $"NutriBite 食光营养助手\n\n" +
                   $"{item.Name}\n" +
                   $"Category: {item.Category}\n" +
                   $"Calories: {item.Calories} kcal\n" +
                   $"Protein: {item.Protein}g | Carbs: {item.Carbs}g | Fat: {item.Fat}g\n" +
                   $"{item.Description}\n" +
                   $"Allergy: {item.AllergyNote}";

        await Share.Default.RequestAsync(new ShareTextRequest
        {
            Text = text,
            Title = $"Share: {item.Name}"
        });
    }

    public static async Task ShareNutritionReportAsync(IReadOnlyList<FoodItem> items)
    {
        if (items.Count == 0)
            return;

        var totalCalories = items.Sum(i => i.Calories);
        var totalProtein = items.Sum(i => i.Protein);
        var totalCarbs = items.Sum(i => i.Carbs);
        var totalFat = items.Sum(i => i.Fat);

        var report = "NutriBite 食光营养助手 - Nutrition Report\n\n" +
                     $"Total items: {items.Count}\n" +
                     $"Total calories: {totalCalories} kcal\n" +
                     $"Total protein: {totalProtein}g\n" +
                     $"Total carbs: {totalCarbs}g\n" +
                     $"Total fat: {totalFat}g\n\n" +
                     "Items:\n";

        foreach (var item in items)
            report += $"- {item.Name} ({item.Category}): {item.Calories} kcal\n";

        await Share.Default.RequestAsync(new ShareTextRequest
        {
            Text = report,
            Title = "NutriBite Nutrition Report"
        });
    }
}
