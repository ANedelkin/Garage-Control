using GarageControl.Core.Contracts;
using Microsoft.EntityFrameworkCore;
using GarageControl.Core.ViewModels;
using GarageControl.Core.ViewModels.Orders;
using GarageControl.Core.ViewModels.Jobs;
using GarageControl.Core.ViewModels.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GarageControl.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IPdfExportService _pdfExportService;

        public OrderController(IOrderService orderService, IPdfExportService pdfExportService)
        {
            _orderService = orderService;
            _pdfExportService = pdfExportService;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            try
            {
                var orders = await _orderService.GetOrdersAsync(User.GetWorkshopId());
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveOrders()
        {
            try
            {
                var orders = await _orderService.GetOrdersAsync(User.GetWorkshopId(), isArchived: false);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("archived")]
        public async Task<IActionResult> GetArchivedOrders()
        {
            try
            {
                var orders = await _orderService.GetOrdersAsync(User.GetWorkshopId(), isArchived: true);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderVM model)
        {
            try
            {
                var result = await _orderService.CreateOrderAsync(User.GetUserId(), User.GetWorkshopId(), model);
                if (result is MethodResponseVM resp && !resp.Success)
                {
                    return BadRequest(new { message = resp.Message });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{orderId}/invoice")]
        public async Task<IActionResult> GetInvoicePdf(string orderId)
        {
            var order = await _orderService.GetOrderInvoiceByIdAsync(orderId);
            if (order == null)
                return NotFound("Order not found.");

            var workshopId = User.GetWorkshopId();
            order.InvoiceNumber = await _orderService.GenerateInvoiceAsync(orderId, workshopId);

            var pdfBytes = await _pdfExportService.GenerateInvoicePdfAsync(order);

            return File(pdfBytes, "application/pdf", $"Invoice_{order.InvoiceNumber}.pdf");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(string id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id, User.GetWorkshopId());
                if (order == null) return NotFound();
                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(string id, [FromBody] UpdateOrderVM model)
        {
            try
            {
                var result = await _orderService.UpdateOrderAsync(User.GetUserId(), id, User.GetWorkshopId(), model);
                if (result is MethodResponseVM resp && !resp.Success)
                {
                    return BadRequest(new { message = resp.Message });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(string id)
        {
            try
            {
                var result = await _orderService.DeleteOrderAsync(User.GetUserId(), id, User.GetWorkshopId());
                if (result is MethodResponseVM resp && !resp.Success)
                {
                    return BadRequest(new { message = resp.Message });
                }
                return Ok(result);
            }
            catch (DbUpdateException)
            {
                return Conflict(new { message = "This order is referenced by other records and cannot be deleted." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
