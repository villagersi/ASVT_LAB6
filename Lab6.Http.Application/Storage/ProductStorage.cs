using System.Collections.Concurrent;
using Lab6.Http.Common;

class ProductStorage : StorageBase<ProductItem>, IProductApi
{
    private static readonly Dictionary<int, ProductItem> defaultData
        = new Dictionary<int, ProductItem>()
        {
            [1] = new ProductItem(1, "Товар 1", 100.00m, 10, true),
            [2] = new ProductItem(2, "Товар 2", 150.50m, 5, true),
            [3] = new ProductItem(3, "Товар 3", 200.00m, 20, true),
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
