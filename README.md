# Кассовое Приложение (Cash Register Application)

## Описание

Это кассовое приложение предназначено для управления товарами в точках продаж. Приложение позволяет добавлять, обновлять, удалять и получать информацию о товарах. Оно использует API для взаимодействия с базой данных товаров и предоставляет удобный интерфейс для пользователей.

## Функциональность

- **Управление товарами**: Добавление, обновление, удаление и получение информации о товарах.
- **Интерфейс командной строки**: Удобный интерфейс для взаимодействия с пользователем через консоль.
- **Поддержка различных операций**: Возможность выполнять операции с товарами, такие как создание новых продуктов и их обновление.

## Структура проекта

### Классы

#### ProductItem

Класс `ProductItem` представляет собой модель товара с основными атрибутами:

```c#
using System.Diagnostics;

namespace Lab6.Http.Common;

public class ProductItem
{
    public ProductItem() { }

    public ProductItem(int id, string name, decimal price, int quantity, bool active)
    {
        Id = id;
        Name = name;
        Price = price;
        Quantity = quantity;
        Active = active;
    }

    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; } // Цена товара

    public int Quantity { get; set; } // Количество товара

    public bool Active { get; set; } // Статус активности товара
}
```

##### Описание свойств:
Id: Уникальный идентификатор товара.
Name: Название товара.
Price: Цена товара.
Quantity: Количество товара на складе.
Active: Статус активности товара (активен/неактивен).

#### ProductStorage

Класс `ProductStorage` отвечает за хранение и управление товарами в приложении. Он реализует интерфейс `IProductApi`, предоставляя методы для работы с продуктами.

##### Основные функции

- **Добавление продуктов**: Метод `AddAsync` позволяет добавлять новые продукты в хранилище.
- **Удаление продуктов**: Метод `DeleteAsync` позволяет удалять продукты по их уникальному идентификатору.
- **Получение всех продуктов**: Метод `GetAllAsync` возвращает массив всех продуктов.
- **Получение продукта по ID**: Метод `GetAsync` возвращает продукт по его уникальному идентификатору.
- **Обновление продукта**: Метод `UpdateAsync` позволяет обновлять информацию о существующем продукте.

```c#
using System.Collections.Concurrent;
using Lab6.Http.Common;

class ProductStorage : StorageBase<ProductItem>, IProductApi
{
    private static readonly Dictionary<int, ProductItem> defaultData
        = new Dictionary<int, ProductItem>()
        {
             = new ProductItem(1, "Товар 1", 100.00m, 10, true),
             = new ProductItem(2, "Товар 2", 150.50m, 5, true),
             = new ProductItem(3, "Товар 3", 200.00m, 20, true),
        };

    private static ConcurrentDictionary<int, ProductItem> productRepository
        = new ConcurrentDictionary<int, ProductItem>();

    private static int _lastId;

    public ProductStorage(IDataSerializer<ProductItem[]> dataSerializer)
        : base(Path.Combine(
            "Data",
            dataSerializer.SerializerType,
            $"{nameof(ProductItem)}.{dataSerializer.SerializerType}"
        ),
        dataSerializer)
    {
        var readData = ReadAsync().Result;

        productRepository = readData?.Any() == true
            ? new ConcurrentDictionary<int, ProductItem>(
                readData.ToDictionary(r => r.Id, r => r))
            : new ConcurrentDictionary<int, ProductItem>(defaultData);

        _lastId = productRepository.Count + 1;
    }

    public async Task<bool> AddAsync(ProductItem newProduct)
    {
        newProduct.Id = _lastId;

        if (productRepository.ContainsKey(_lastId))
        {
            return false;
        }

        var result = productRepository.TryAdd(_lastId, newProduct);

        if (!result)
        {
            return false;
        }

        Interlocked.Increment(ref _lastId);

        result = await WriteAsync(productRepository.Values.ToArray());

        return result;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (!productRepository.ContainsKey(id))
        {
            return false;
        }

        var result = productRepository.Remove(id, out var _);
        if (!result)
        {
            return false;
        }

        result = await WriteAsync(productRepository.Values.ToArray());

        return result;
    }

    public Task<ProductItem[]> GetAllAsync()
    {
        return Task.FromResult(productRepository.Values.ToArray());
    }

    public Task<ProductItem?> GetAsync(int id)
    {
        if (!productRepository.ContainsKey(id))
        {
            return Task.FromResult<ProductItem?>(null);
        }

        return Task.FromResult<ProductItem?>(productRepository[id]);
    }

    public async Task<bool> UpdateAsync(int id, ProductItem updateProduct)
    {
        if (!productRepository.ContainsKey(id))
        {
            return false;
        }

        var result = productRepository.TryUpdate(id, updateProduct, productRepository[id]);
        if (!result)
        {
            return false;
        }

        result = await WriteAsync(productRepository.Values.ToArray());

        return result;
    }
}
```
##### Описание методов
AddAsync(ProductItem newProduct):
Добавляет новый продукт в хранилище. Устанавливает ID продукта и сохраняет его в репозитории. Возвращает true, если добавление прошло успешно; иначе — false.
DeleteAsync(int id):
Удаляет продукт из хранилища по его уникальному идентификатору. Возвращает true, если удаление прошло успешно; иначе — false.
GetAllAsync():
Возвращает массив всех продуктов в хранилище.
GetAsync(int id):
Возвращает продукт по его уникальному идентификатору или null, если продукт не найден.
UpdateAsync(int id, ProductItem updateProduct):
Обновляет информацию о продукте по его уникальному идентификатору. Возвращает true, если обновление прошло успешно; иначе — false.

### API
IProductApi
 Интерфейс IProductApi определяет методы для работы с продуктами:
```c#
using System.Threading.Tasks;

namespace Lab6.Http.Common;

public interface IProductApi 
{
    Task<bool> AddAsync(ProductItem newProduct);
    Task<bool> DeleteAsync(int id);
    Task<bool> UpdateAsync(int id, ProductItem newProduct);
    Task<ProductItem?> GetAsync(int id);
    Task<ProductItem[]> GetAllAsync();
}
```
### Контроллеры
ProductController
Контроллер ProductController обрабатывает HTTP-запросы для управления продуктами:
```c#
using Lab6.Http.Common;
using Microsoft.AspNetCore.Mvc;

namespace Lab6.Http.Application.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductApi productApi;

        public ProductController(IProductApi productApi)
        {
            this.productApi = productApi;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductItem>> GetAsync(int id)
        {
            var product = await productApi.GetAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpGet]
        public async Task<ActionResult<ProductItem[]>> GetAllAsync()
        {
            var products = await productApi.GetAllAsync();
            if (products?.Any() != true)
            {
                return NotFound();
            }
            return Ok(products);
        }

        [HttpPost]
        public async Task<ActionResult<ProductItem>> PostAsync([FromBody] ProductItem product)
        {
            // Добавляем новый продукт
            var result = await productApi.AddAsync(product);
            if (!result)
            {
                return BadRequest("Error creating product.");
            }
            // Возвращаем созданный продукт с правильным маршрутом
            return CreatedAtAction("Get", new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> PutAsync(int id, [FromBody] ProductItem product)
        {
            var result = await productApi.UpdateAsync(id, product);
            if (!result)
            {
                return BadRequest("Error updating product.");
            }
            return Ok(product);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            var result = await productApi.DeleteAsync(id);
            if (!result)
            {
                return BadRequest("Error deleting product.");
            }
            return NoContent();
        }
    }
}
```
### Интерфейс пользователя
Пример создания нового продукта в консольном приложении:
```c#
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
```
## Работа программы
![first](https://raw.githubusercontent.com/villagersi/ASVT_LAB6/4609b0ab9a167fc22a724a0238df7e64ae39f128/ScreenShot1.png)
![second](https://raw.githubusercontent.com/villagersi/ASVT_LAB6/4609b0ab9a167fc22a724a0238df7e64ae39f128/ScreenShot2.png)
## Установка
Склонируйте репозиторий на локальную машину:
```bash
git clone https://github.com/ваш_репозиторий.git
```
Откройте проект в вашей IDE (например, Visual Studio или Rider).
Убедитесь, что все зависимости установлены и проект собран.
Запустите приложение и следуйте инструкциям на экране.
Использование
После запуска приложения вы сможете взаимодействовать с ним через консоль. Доступные команды:
Получить все продукты
Получить продукт по ID
Создать новый продукт
Обновить существующий продукт
Удалить продукт по ID
Для создания нового продукта вам будет предложено ввести название, цену, количество и статус активности.
