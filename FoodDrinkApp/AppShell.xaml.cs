namespace FoodDrinkApp;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(AddItemPage), typeof(AddItemPage));
		Routing.RegisterRoute(nameof(FoodDetailPage), typeof(FoodDetailPage));
		Routing.RegisterRoute(nameof(EditItemPage), typeof(EditItemPage));
	}
}
