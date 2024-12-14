using System.Diagnostics;

namespace Lab6.Http.Common;

public class ProductItem
{
    public ProductItem()
    {
    }

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
