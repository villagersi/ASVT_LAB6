using Lab6.Http.Common;

internal class Program
{
    private static object _locker = new object();

    public static async Task Main(string[] args)
    {
        var httpClient = new HttpClient()
        {
            BaseAddress = new Uri("http://localhost:5214/api/")
        };

        var productApiClient = new ProductApiClient(httpClient);

        await ManageProducts(productApiClient);
    }

    private static async Task ManageProducts(IProductApi productApi)
    {
        PrintMenu();

        while (true)
        {
            var key = Console.ReadKey(true);

            PrintMenu();

            if (key.Key == ConsoleKey.D1)
            {
                var products = await productApi.GetAllAsync();
                Console.WriteLine($"| Id    |     Name        | Price  | Quantity | Active |");
                foreach (var product in products)
                {
                    Console.WriteLine($"| {product.Id,5} | {product.Name,15} | {product.Price,6:C} | {product.Quantity,8} | {product.Active,8} |");
                }
            }

            if (key.Key == ConsoleKey.D2)
            {
                Console.Write("Enter product id: ");
                var productIdString = Console.ReadLine();
                int.TryParse(productIdString, out var productId);
                var product = await productApi.GetAsync(productId);
                Console.WriteLine($"Id={product?.Id}, Name={product?.Name}, Price={product?.Price:C}, Quantity={product?.Quantity}, Active={product?.Active}");
            }

            if (key.Key == ConsoleKey.D3) // Создание нового продукта
            {
                Console.Write("Введите название продукта: ");
                var name = Console.ReadLine() ?? "Без названия";

                Console.Write("Введите цену продукта: ");
                decimal price;
                while (!decimal.TryParse(Console.ReadLine(), out price) || price < 0)
                {
                    Console.Write("Пожалуйста, введите корректную положительную цену: ");
                }

                Console.Write("Введите количество продукта: ");
                int quantity;
                while (!int.TryParse(Console.ReadLine(), out quantity) || quantity < 0)
                {
                    Console.Write("Пожалуйста, введите корректное положительное количество: ");
                }

                Console.Write("Активен ли продукт? (да/нет): ");
                var activeInput = Console.ReadLine()?.ToLower();
                bool active = activeInput == "да" || activeInput == "yes";

                var newProduct = new ProductItem(
                    id: 0, // ID будет установлен на сервере
                    name: name,
                    price: price,
                    quantity: quantity,
                    active: active
                );

                var addResult = await productApi.AddAsync(newProduct);
                Console.WriteLine(addResult ? "Продукт добавлен." : "Ошибка добавления продукта.");
            }


            if (key.Key == ConsoleKey.D4) // Обновление продукта
            {
                Console.Write("Enter product id to update: ");
                var productIdString = Console.ReadLine();
                int.TryParse(productIdString, out var productId);

                // Получаем текущие данные продукта
                var existingProduct = await productApi.GetAsync(productId);

                if (existingProduct != null)
                {
                    // Запрашиваем новые данные
                    Console.Write("Enter new product name (leave empty to keep current): ");
                    var newName = Console.ReadLine();
                    if (string.IsNullOrEmpty(newName))
                    {
                        newName = existingProduct.Name;
                    }

                    Console.Write("Enter new price (leave empty to keep current): ");
                    var priceInput = Console.ReadLine();
                    decimal newPrice = existingProduct.Price;
                    if (!string.IsNullOrEmpty(priceInput))
                    {
                        decimal.TryParse(priceInput, out newPrice);
                    }

                    Console.Write("Enter new quantity (leave empty to keep current): ");
                    var quantityInput = Console.ReadLine();
                    int newQuantity = existingProduct.Quantity;
                    if (!string.IsNullOrEmpty(quantityInput))
                    {
                        int.TryParse(quantityInput, out newQuantity);
                    }

                    // Создаем обновленный продукт
                    var updatedProduct = new ProductItem(
                        id: productId,
                        name: newName,
                        price: newPrice,
                        quantity: newQuantity,
                        active: existingProduct.Active // Сохраняем статус активности
                    );

                    // Обновляем продукт
                    var updateResult = await productApi.UpdateAsync(productId, updatedProduct);
                    Console.WriteLine(updateResult ? "Updated successfully" : "Update failed");
                }
                else
                {
                    Console.WriteLine("Product not found.");
                }
            }

            if (key.Key == ConsoleKey.D5) // Удаление продукта
            {
                Console.Write("Enter product id to delete: ");
                var productIdString = Console.ReadLine();
                int.TryParse(productIdString, out var productId);

                // Удаляем продукт
                var deleteResult = await productApi.DeleteAsync(productId);
                Console.WriteLine(deleteResult ? "Deleted successfully" : "Delete failed");
            }

            if (key.Key == ConsoleKey.Escape)
            {
                break;
            }
        }

        Console.ReadKey();
    }

    private static void PrintMenu()
    {
        lock (_locker)
        {
            Console.WriteLine("1 - Get all products");
            Console.WriteLine("2 - Get product by id");
            Console.WriteLine("3 - Create product");
            Console.WriteLine("4 - Update product");
            Console.WriteLine("5 - Delete product");
            Console.WriteLine("-------");
        }
    }
}
