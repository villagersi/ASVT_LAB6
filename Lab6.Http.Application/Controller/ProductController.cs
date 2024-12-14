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
