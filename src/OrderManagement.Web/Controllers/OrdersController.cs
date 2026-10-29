using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application;
using OrderManagement.Domain;

namespace OrderManagement.Web.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController(IMediator mediator, DiscountStrategyResolver discounts) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedOrdersResponse>(StatusCodes.Status200OK)]
    public Task<PagedOrdersResponse> GetOrders(
        [FromQuery] string? search,
        [FromQuery] string? sortBy,
        [FromQuery] bool descending = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var specification = new OrderSpecification(search, sortBy, descending, page, pageSize);
        return mediator.Send(new GetOrdersQuery(specification), cancellationToken);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> GetOrder(Guid id, CancellationToken cancellationToken)
    {
        var order = await mediator.Send(new GetOrderQuery(id), cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderResponse>> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var order = await mediator.Send(new CreateOrderCommand(request), cancellationToken);
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch (DomainValidationException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Order validation failed", Detail = exception.Message });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid discount strategy", Detail = exception.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<OrderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> UpdateOrder(
        Guid id,
        [FromBody] UpdateOrderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var order = await mediator.Send(new UpdateOrderCommand(id, request), cancellationToken);
            return order is null ? NotFound() : Ok(order);
        }
        catch (DomainValidationException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Order validation failed", Detail = exception.Message });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid discount strategy", Detail = exception.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteOrder(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await mediator.Send(new DeleteOrderCommand(id), cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("discounts")]
    [ProducesResponseType<IReadOnlyCollection<DiscountOptionResponse>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyCollection<DiscountOptionResponse>> GetDiscounts() =>
        Ok(discounts.All.Select(strategy => new DiscountOptionResponse(strategy.Code, strategy.Name)));

    [HttpPost("discounts/preview")]
    [ProducesResponseType<DiscountPreviewResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<DiscountPreviewResponse> PreviewDiscount(
        [FromBody] DiscountPreviewRequest request)
    {
        try
        {
            var strategy = discounts.Resolve(request.Strategy);
            var discount = strategy.CalculateDiscount(request.Subtotal);
            return Ok(new DiscountPreviewResponse(
                request.Subtotal,
                discount,
                request.Subtotal - discount,
                strategy.Code,
                strategy.Name));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid discount strategy", Detail = exception.Message });
        }
    }
}

public sealed record DiscountOptionResponse(string Code, string Name);
public sealed record DiscountPreviewResponse(
    decimal Subtotal,
    decimal Discount,
    decimal Total,
    string Strategy,
    string StrategyName);

public sealed record DiscountPreviewRequest(
    [param: System.ComponentModel.DataAnnotations.Required] string Strategy,
    [param: System.ComponentModel.DataAnnotations.Range(typeof(decimal), "0.01", "100000000")] decimal Subtotal);
