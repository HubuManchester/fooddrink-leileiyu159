using FoodDrinkApp.Models;
using SQLite;

namespace FoodDrinkApp.Services;

public static class DatabaseService
{
    private static SQLiteAsyncConnection? _database;

    private static async Task<SQLiteAsyncConnection> GetDatabaseAsync()
    {
        if (_database is not null)
            return _database;

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "nutribite.db3");
        _database = new SQLiteAsyncConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

        await _database.CreateTableAsync<FoodItemEntity>();
        return _database;
    }

    public static async Task<List<FoodItemEntity>> GetAllAsync()
    {
        var db = await GetDatabaseAsync();
        return await db.Table<FoodItemEntity>().ToListAsync();
    }

    public static async Task<FoodItemEntity?> GetByIdAsync(string id)
    {
        var db = await GetDatabaseAsync();
        return await db.Table<FoodItemEntity>().FirstOrDefaultAsync(i => i.Id == id);
    }

    public static async Task<int> SaveAsync(FoodItemEntity item)
    {
        var db = await GetDatabaseAsync();
        var existing = await db.Table<FoodItemEntity>().FirstOrDefaultAsync(i => i.Id == item.Id);
        if (existing is not null)
        {
            item.LocalId = existing.LocalId;
            return await db.UpdateAsync(item);
        }

        return await db.InsertAsync(item);
    }

    public static async Task<int> DeleteAsync(string id)
    {
        var db = await GetDatabaseAsync();
        var item = await db.Table<FoodItemEntity>().FirstOrDefaultAsync(i => i.Id == id);
        if (item is not null)
            return await db.DeleteAsync(item);
        return 0;
    }

    public static async Task<List<FoodItemEntity>> SearchAsync(string? query)
    {
        var all = await GetAllAsync();

        if (string.IsNullOrWhiteSpace(query))
            return all.OrderBy(i => i.Name).ToList();

        var normalised = query.Trim();
        return all
            .Where(i =>
                (i.Name?.Contains(normalised, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (i.Category?.Contains(normalised, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (i.Description?.Contains(normalised, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (i.Tags?.Contains(normalised, StringComparison.OrdinalIgnoreCase) ?? false))
            .OrderBy(i => i.Name)
            .ToList();
    }
}

[Table("FoodItems")]
public class FoodItemEntity
{
    [PrimaryKey, AutoIncrement]
    public int LocalId { get; set; }

    [MaxLength(64), Unique]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(64)]
    public string Category { get; set; } = string.Empty;

    [MaxLength(512)]
    public string Description { get; set; } = string.Empty;

    public int Calories { get; set; }
    public int Protein { get; set; }
    public int Carbs { get; set; }
    public int Fat { get; set; }

    [MaxLength(256)]
    public string AllergyNote { get; set; } = string.Empty;

    [MaxLength(256)]
    public string Tags { get; set; } = string.Empty;

    public static FoodItemEntity FromModel(FoodItem item) => new()
    {
        Id = item.Id,
        Name = item.Name,
        Category = item.Category,
        Description = item.Description,
        Calories = item.Calories,
        Protein = item.Protein,
        Carbs = item.Carbs,
        Fat = item.Fat,
        AllergyNote = item.AllergyNote,
        Tags = item.Tags
    };

    public FoodItem ToModel() => new()
    {
        Id = Id,
        Name = Name,
        Category = Category,
        Description = Description,
        Calories = Calories,
        Protein = Protein,
        Carbs = Carbs,
        Fat = Fat,
        AllergyNote = AllergyNote,
        Tags = Tags
    };
}
