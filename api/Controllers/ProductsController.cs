using Microsoft.AspNetCore.Mvc;
using Restaurant.Api.Dtos;
using Restaurant.Api.Services;

namespace Restaurant.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(IProductService products) : ControllerBase
{
    [HttpGet("groups")]
    public async Task<ActionResult<IReadOnlyList<ProductGroupDto>>> GetGroups()
    {
        return Ok(await products.GetGroupsAsync());
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetProducts([FromQuery] int? groupId)
    {
        return Ok(await products.GetProductsAsync(groupId));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var product = await products.GetProductAsync(id);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct(ProductRequest request)
    {
        var product = await products.CreateProductAsync(request);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(int id, ProductRequest request)
    {
        var product = await products.UpdateProductAsync(id, request);
        return product is null ? NotFound() : Ok(product);
    }
}
