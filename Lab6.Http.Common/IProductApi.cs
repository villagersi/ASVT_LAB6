namespace Lab6.Http.Common;

public interface IProductApi 
{
    Task<bool> AddAsync(ProductItem newProduct);

    Task<bool> DeleteAsync(int id);

    Task<bool> UpdateAsync(int id, ProductItem newProduct);

    Task<ProductItem?> GetAsync(int id);

    Task<ProductItem[]> GetAllAsync();
}
