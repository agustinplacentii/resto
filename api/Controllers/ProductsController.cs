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

    [HttpPost("groups")]
    public async Task<ActionResult<ProductGroupDto>> CreateGroup(ProductGroupRequest request)
    {
        try
        {
            var group = await products.CreateGroupAsync(request);
            return Created($"/api/products/groups/{group.Id}", group);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
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
        try
        {
            var product = await products.CreateProductAsync(request);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(int id, ProductRequest request)
    {
        var product = await products.UpdateProductAsync(id, request);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost("{id:int}/stock-discounts")]
    public async Task<ActionResult<ProductDto>> DiscountStock(int id, StockAdjustmentRequest request)
    {
        try
        {
            var product = await products.DiscountStockAsync(id, request);
            return product is null ? NotFound() : Ok(product);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:int}/stock-additions")]
    public async Task<ActionResult<ProductDto>> AddStock(int id, StockAdjustmentRequest request)
    {
        try
        {
            var product = await products.AddStockAsync(id, request);
            return product is null ? NotFound() : Ok(product);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
