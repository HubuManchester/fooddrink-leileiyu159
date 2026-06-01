using System.Net.Http.Json;
using System.Text.Json;
using FoodDrinkApp.Models;

namespace FoodDrinkApp.Services;

public static class FoodCatalogService
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(12)
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly List<FoodItem> LocalFallbackItems =
    [
        new()
        {
            Name = "Berry Yogurt Bowl",
            Category = "Breakfast",
            Description = "Greek yogurt with mixed berries, oats, and a small drizzle of honey.",
            Calories = 340,
            Protein = 24,
            Carbs = 42,
            Fat = 8,
            AllergyNote = "Contains dairy and gluten.",
            Tags = "healthy breakfast yogurt berries"
        },
        new()
        {
            Name = "Chicken Brown Rice Box",
            Category = "Lunch",
            Description = "Grilled chicken breast with brown rice, spinach, cucumber, and lemon dressing.",
            Calories = 520,
            Protein = 38,
            Carbs = 58,
            Fat = 14,
            AllergyNote = "No common allergens recorded.",
            Tags = "meal prep protein lunch"
        },
        new()
        {
            Name = "Iced Matcha Latte",
            Category = "Drink",
            Description = "Matcha, milk, and ice. A lower-sugar version is recommended.",
            Calories = 180,
            Protein = 8,
            Carbs = 22,
            Fat = 6,
            AllergyNote = "Contains dairy unless plant-based milk is selected.",
            Tags = "drink caffeine matcha latte"
        },
        new()
        {
            Name = "Tomato Wholegrain Pasta",
            Category = "Dinner",
            Description = "Wholegrain pasta with tomato sauce, basil, and roasted vegetables.",
            Calories = 610,
            Protein = 18,
            Carbs = 92,
            Fat = 16,
            AllergyNote = "Contains gluten.",
            Tags = "vegetarian dinner pasta"
        },
        new()
        {
            Name = "红豆薏米粥",
            Category = "Breakfast",
            Description = "Red bean and coix seed congee, lightly sweetened with rock sugar. A traditional Chinese breakfast.",
            Calories = 280,
            Protein = 12,
            Carbs = 52,
            Fat = 2,
            AllergyNote = "No common allergens recorded.",
            Tags = "Chinese breakfast congee healthy"
        },
        new()
        {
            Name = "番茄鸡蛋面",
            Category = "Lunch",
            Description = "Hand-pulled noodles with scrambled eggs and fresh tomato broth. Garnished with spring onion.",
            Calories = 450,
            Protein = 18,
            Carbs = 62,
            Fat = 14,
            AllergyNote = "Contains egg and gluten.",
            Tags = "Chinese noodles lunch comfort food"
        }
    ];

    private static List<FoodItem> cachedItems = [];
    private static bool isInitialised;

    public static bool LastLoadUsedMockApi { get; private set; }
    public static string DataSourceDescription => LastLoadUsedMockApi ? "mockapi.io (remote)" : "local database";

    public static async Task<IReadOnlyList<FoodItem>> SearchAsync(string? query)
    {
        await EnsureInitialisedAsync();

        var normalised = query?.Trim();
        var items = string.IsNullOrWhiteSpace(normalised)
            ? cachedItems
            : cachedItems
                .Where(item =>
                    item.Name.Contains(normalised, StringComparison.OrdinalIgnoreCase) ||
                    item.Category.Contains(normalised, StringComparison.OrdinalIgnoreCase) ||
                    item.Description.Contains(normalised, StringComparison.OrdinalIgnoreCase) ||
                    item.Tags.Contains(normalised, StringComparison.OrdinalIgnoreCase))
                .ToList();

        return items.OrderBy(item => item.Name).ToList();
    }

    public static async Task<FoodItem?> GetByIdAsync(string id)
    {
        await EnsureInitialisedAsync();

        if (MockApiConfig.IsConfigured)
        {
            try
            {
                var item = await HttpClient.GetFromJsonAsync<FoodItem>(
                    $"{MockApiConfig.EndpointUrl.TrimEnd('/')}/{Uri.EscapeDataString(id)}",
                    JsonOptions);

                if (item is not null)
                    return item;
            }
            catch
            {
            }
        }

        return cachedItems.FirstOrDefault(item => item.Id == id);
    }

    public static async Task<FoodItem> AddAsync(FoodItem item)
    {
        await EnsureInitialisedAsync();

        if (MockApiConfig.IsConfigured)
        {
            try
            {
                var response = await HttpClient.PostAsJsonAsync(MockApiConfig.EndpointUrl, item, JsonOptions);
                response.EnsureSuccessStatusCode();

                var created = await response.Content.ReadFromJsonAsync<FoodItem>(JsonOptions);
                if (created is not null)
                {
                    await DatabaseService.SaveAsync(FoodItemEntity.FromModel(created));
                    cachedItems.Add(created);
                    return created;
                }
            }
            catch
            {
            }
        }

        await DatabaseService.SaveAsync(FoodItemEntity.FromModel(item));
        cachedItems.Add(item);
        return item;
    }

    public static async Task<bool> UpdateAsync(FoodItem item)
    {
        await EnsureInitialisedAsync();

        if (MockApiConfig.IsConfigured)
        {
            try
            {
                var response = await HttpClient.PutAsJsonAsync(
                    $"{MockApiConfig.EndpointUrl.TrimEnd('/')}/{Uri.EscapeDataString(item.Id)}",
                    item, JsonOptions);
                response.EnsureSuccessStatusCode();
            }
            catch
            {
            }
        }

        await DatabaseService.SaveAsync(FoodItemEntity.FromModel(item));
        var index = cachedItems.FindIndex(i => i.Id == item.Id);
        if (index >= 0)
            cachedItems[index] = item;

        return true;
    }

    public static async Task<bool> DeleteAsync(string id)
    {
        await EnsureInitialisedAsync();

        if (MockApiConfig.IsConfigured)
        {
            try
            {
                await HttpClient.DeleteAsync(
                    $"{MockApiConfig.EndpointUrl.TrimEnd('/')}/{Uri.EscapeDataString(id)}");
            }
            catch
            {
            }
        }

        await DatabaseService.DeleteAsync(id);
        cachedItems.RemoveAll(i => i.Id == id);
        return true;
    }

    private static async Task EnsureInitialisedAsync()
    {
        if (isInitialised)
            return;

        try
        {
            var dbItems = await DatabaseService.GetAllAsync();
            if (dbItems.Count > 0)
            {
                cachedItems = dbItems.Select(e => e.ToModel()).ToList();
                isInitialised = true;
                return;
            }
        }
        catch
        {
        }

        if (MockApiConfig.IsConfigured)
        {
            try
            {
                var items = await HttpClient.GetFromJsonAsync<List<FoodItem>>(MockApiConfig.EndpointUrl, JsonOptions);
                if (items is { Count: > 0 })
                {
                    cachedItems = items;
                    foreach (var item in items)
                        await DatabaseService.SaveAsync(FoodItemEntity.FromModel(item));
                    LastLoadUsedMockApi = true;
                    isInitialised = true;
                    return;
                }
            }
            catch
            {
            }
        }

        foreach (var item in LocalFallbackItems)
            await DatabaseService.SaveAsync(FoodItemEntity.FromModel(item));

        cachedItems = new List<FoodItem>(LocalFallbackItems);
        LastLoadUsedMockApi = false;
        isInitialised = true;
    }
}
