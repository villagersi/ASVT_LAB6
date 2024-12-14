using System.Net.Http.Json;
using Lab6.Http.Common;

public class ProductApiClient : IProductApi
{
    private readonly HttpClient httpClient;

    public ProductApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<bool> AddAsync(ProductItem newProduct)
    {
        var response = await httpClient.PostAsJsonAsync("Product", newProduct);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await httpClient.DeleteAsync($"Product/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<ProductItem[]> GetAllAsync()
    {
        var results = await httpClient.GetFromJsonAsync<ProductItem[]>("Product");
        return results?.ToArray() ?? Array.Empty<ProductItem>();
    }

    public async Task<ProductItem?> GetAsync(int id)
    {
        try
        {
            var result = await httpClient.GetFromJsonAsync<ProductItem?>($"Product/{id}");
            return result;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> UpdateAsync(int id, ProductItem updateProduct)
    {
        var response = await httpClient.PutAsJsonAsync($"Product/{id}", updateProduct);
        return response.IsSuccessStatusCode;
    }
}
