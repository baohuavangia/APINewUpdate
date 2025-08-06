using MenShop_Assignment.Datas;
using MenShop_Assignment.DTOs;
using MenShop_Assignment.Repositories.DiscountPriceRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MenShop_Assignment.APIControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiscountPriceController : ControllerBase
    {
        private readonly IDiscountPriceRepository _repo;

        public DiscountPriceController(IDiscountPriceRepository repo)
        {
            _repo = repo;
        }

        // 1. Lấy tất cả khuyến mãi
        [HttpGet]
        public async Task<IActionResult> GetAllDiscount()
        {
            var result = await _repo.GetAllDiscountPrice();
            return Ok(result);
        }

        // 2. Lấy khuyến mãi theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0) return BadRequest("Id không hợp lệ.");

            var result = await _repo.GetByIdDiscountPrice(id);
            if (result == null)
                return NotFound($"Không tìm thấy khuyến mãi với ID = {id}");

            return Ok(result);
        }

        // 3. Lấy tất cả sản phẩm áp dụng khuyến mãi theo DiscountId
        [HttpGet("detail/{discountId}")]
        public IActionResult GetAllDetailDiscount(int discountId)
        {
            if (discountId <= 0) return BadRequest("Id không hợp lệ.");

            var result = _repo.GetProductDetailsByDiscountId(discountId);
            if (result == null || !result.Any())
                return NotFound($"Không có sản phẩm nào áp dụng khuyến mãi với ID = {discountId}");

            return Ok(result);
        }

        // 4. Tạo mới khuyến mãi
        [HttpPost]
        public async Task<IActionResult> CreateDiscount([FromBody] CreateDiscountPriceDTO discount)
        {
            if (discount == null ||
                string.IsNullOrWhiteSpace(discount.Name) ||
                discount.StartTime >= discount.EndTime)
            {
                return BadRequest("Dữ liệu không hợp lệ: tên không được trống, thời gian không hợp lệ.");
            }
            var discountPrice = new DiscountPrice
            {
                Name = discount.Name,
                DiscountPercent = discount.DiscountPercent,
                StartTime = discount.StartTime,
                EndTime = discount.EndTime,
            };
            try
            {
                var result = await _repo.CreateDiscount(discountPrice);
                return result ? Ok(discountPrice) : BadRequest("Thêm Collection thất bại");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi máy chủ: {ex.Message}");

            }
        }

        // 5. Cập nhật khuyến mãi
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDiscount(int id, [FromBody] CreateDiscountPriceDTO discount)
        {

            if (id <= 0 || discount == null)
                return BadRequest("Dữ liệu không hợp lệ.");

            var check = await _repo.GetByIdDiscountPrice(id);
            if (check == null)
                return NotFound($"Không tìm thấy Collection với ID = {id}");

            var updated = new DiscountPrice
            {
                Id = id,
                Name = discount.Name,
                DiscountPercent = discount.DiscountPercent,
                StartTime = discount.StartTime,
                EndTime = discount.EndTime,
            };

            try
            {
                var result = await _repo.UpdateDiscount(updated);
                return result ? Ok("Cập nhật thành công") : BadRequest("Cập nhật thất bại.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi máy chủ: {ex.Message}");
            }

        }

        // 6. Xóa khuyến mãi
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDiscount(int id)
        {
            var deleted = await _repo.DeleteDiscount(id);
            if (!deleted) return BadRequest("Không thể xóa khuyến mãi (có thể đang có sản phẩm áp dụng).");

            return Ok("Xóa khuyến mãi thành công.");
        }

        [HttpPost("detail")]
        public async Task<IActionResult> CreateDiscountDetail([FromBody] CreateDiscountDetailDTO dto)
        {
            if (dto == null || dto.discountPriceId <= 0 || dto.productDetailIds == null || !dto.productDetailIds.Any())
                return BadRequest("Dữ liệu không hợp lệ.");

            var failedIds = new List<int>();

            foreach (var productDetailId in dto.productDetailIds)
            {
                // Kiểm tra nếu chi tiết đã tồn tại
                var isExists = (await _repo.GetDiscountDetailsByProductDetailId(productDetailId)).Any();
                if (isExists)
                {
                    failedIds.Add(productDetailId);
                    continue;
                }

                var detail = new DiscountPriceDetail
                {
                    discountPriceId = dto.discountPriceId,
                    productDetailId = productDetailId
                };

                try
                {
                    var result = await _repo.CreateDiscountDetail(detail);
                    if (!result)
                        failedIds.Add(productDetailId);
                }
                catch (DbUpdateException)
                {
                    failedIds.Add(productDetailId);
                }
            }

            if (failedIds.Any())
            {
                return StatusCode(207, new
                {
                    Message = "Một số sản phẩm đã có chương trình khuyến mãi hoặc lỗi khi thêm.",
                    FailedProductDetailIds = failedIds
                });
            }

            return Ok("Tất cả sản phẩm đã được thêm vào khuyến mãi.");
        }




        // 8. Cập nhật chi tiết sản phẩm khuyến mãi
        [HttpPut("detail/{id}")]
        public async Task<IActionResult> UpdateDiscountDetail(int id, [FromBody] UpdateDiscountDetailDTO detail)
        {
            if (id <= 0 || detail == null)
                return BadRequest("Dữ liệu không hợp lệ.");

            var oldCollectionId = await _repo.GetByIdDiscountDetailPrice(id);
           
            var update = new DiscountPriceDetail
            {
                Id = id,
                productDetailId = detail.productDetailId,
                discountPriceId = detail.discountPriceId
                
            };
            try
            {
                var result = await _repo.UpdateDiscountDetail(update);
                return result ? Ok(detail) : BadRequest("Sản phẩm đã có trong Collection.");
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Lỗi CSDL: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        // 9. Xóa chi tiết sản phẩm khuyến mãi
        [HttpDelete("detail/{id}")]
        public async Task<IActionResult> DeleteDiscountDetail(int id)
        {
            var deleted = await _repo.DeleteDiscountDetail(id);
            if (!deleted) return NotFound("Không thể xóa chi tiết.");

            return Ok("Xóa chi tiết thành công.");
        }
        // 10. Lấy chi tiết giảm giá theo ProductDetailId
        [HttpGet("detail/product/{productDetailId}")]
        public async Task<IActionResult> GetDiscountDetailByProductDetailId(int productDetailId)
        {
            if (productDetailId <= 0) return BadRequest("ProductDetailId không hợp lệ.");

            var result = await _repo.GetDiscountDetailsByProductDetailId(productDetailId);
            if (result == null || !result.Any())
                return NotFound($"Không tìm thấy giảm giá nào cho ProductDetailId = {productDetailId}");

            return Ok(result);
        }

    }
}
