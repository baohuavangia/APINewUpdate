using MenShop_Assignment.Datas;
using MenShop_Assignment.DTOs;
using MenShop_Assignment.Models;
using MenShop_Assignment.Repositories.CollectionRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MenShop_Assignment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CollectionsController : ControllerBase
    {
        private readonly ICollectionRepository _repo;

        public CollectionsController(ICollectionRepository repo)
        {
            _repo = repo;
        }

        // ===== COLLECTION =====

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _repo.GetAllCollection();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            if (id <= 0) return BadRequest("Id không hợp lệ.");

            var result = await _repo.GetByIdCollection(id);
            if (result == null)
                return NotFound($"Không tìm thấy Collection với ID = {id}");

            return Ok(result);
        }

        [HttpPost("CreateCollection")]
        public async Task<IActionResult> AddCollection([FromBody] CollectionCreateDto dto)
        {
            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.CollectionName) ||
                dto.StartTime >= dto.EndTime)
            {
                return BadRequest("Dữ liệu không hợp lệ: tên không được trống, thời gian không hợp lệ.");
            }

            var collection = new Collection
            {
                CollectionName = dto.CollectionName,
                Description = dto.Description,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Status = dto.Status
            };

            try
            {
                var result = await _repo.AddCollection(collection);
                return result ? Ok(collection) : BadRequest("Thêm Collection thất bại");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi máy chủ: {ex.Message}");
            }
        }

        [HttpPut("update-collection/{id}")]
        public async Task<IActionResult> UpdateCollection(int id, [FromBody] CollectionCreateDto dto)
        {
            if (id <= 0 || dto == null)
                return BadRequest("Dữ liệu không hợp lệ.");

            var check = await _repo.GetByIdCollection(id);
            if (check == null)
                return NotFound($"Không tìm thấy Collection với ID = {id}");

            var updated = new Collection
            {
                CollectionId = id,
                CollectionName = dto.CollectionName,
                Description = dto.Description,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Status = dto.Status
            };

            try
            {
                var result = await _repo.UpdateCollection(updated);
                return result ? Ok("Cập nhật thành công") : BadRequest("Cập nhật thất bại.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi máy chủ: {ex.Message}");
            }
        }

        [HttpDelete("delete-collection/{id}")]
        public async Task<IActionResult> DeleteCollection(int id)
        {
            if (id <= 0)
                return BadRequest(new ApiResponseModel<string>(false, "Id không hợp lệ.", null, 400));

            var check = await _repo.GetByIdCollection(id);
            if (check == null)
                return NotFound(new ApiResponseModel<string>(false, $"Không tìm thấy Collection với ID = {id}", null, 404));

            var result = await _repo.DeleteCollection(id);
            if (result)
                return Ok(new ApiResponseModel<string>(true, "Đã xoá Collection", null, 200));

            return BadRequest(new ApiResponseModel<string>(false, "Không thể xoá Collection do đã có CollectionDetail.", null, 400));
        }

        // ===== COLLECTION DETAIL =====

        [HttpGet("{collectionId}/details")]
        public async Task<IActionResult> GetDetails(int collectionId)
        {
            if (collectionId <= 0)
                return BadRequest("CollectionId không hợp lệ.");

            var collection = await _repo.GetByIdCollection(collectionId);
            if (collection == null)
                return NotFound("Collection không tồn tại.");

            var details = await _repo.GetCollectionDetailsByCollectionId(collectionId);
            return Ok(details);
        }

        [HttpPost("add-details")]
        public async Task<IActionResult> AddDetail([FromBody] CollectionDetailCreateDto dto)
        {
            var detail = new CollectionDetail
            {
                CollectionId = dto.CollectionId,
                ProductId = dto.ProductId
            };

            try
            {
                var result = await _repo.AddDetail(detail);
                return result ? Ok(detail) : BadRequest("Sản phẩm đã có trong Collection.");
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Lỗi CSDL: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        [HttpPut("details/{detailId}")]
        public async Task<IActionResult> UpdateDetail(int detailId, [FromBody] CollectionDetailCreateDto dto)
        {
            if (detailId <= 0 || dto == null || dto.ProductId <= 0)
                return BadRequest("Dữ liệu không hợp lệ.");

            var oldCollectionId = await GetCollectionIdByDetailId(detailId);
            if (oldCollectionId == 0)
                return NotFound($"Không tìm thấy CollectionDetail với ID = {detailId}");

            var detail = new CollectionDetail
            {
                CollectionDetailId = detailId,
                CollectionId = oldCollectionId,
                ProductId = dto.ProductId
            };

            try
            {
                var result = await _repo.UpdateDetail(detail);
                return result ? Ok("Cập nhật chi tiết thành công") : NotFound("Không tìm thấy CollectionDetail.");
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Lỗi CSDL: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        [HttpDelete("details/{detailId}")]
        public async Task<IActionResult> DeleteDetail(int detailId)
        {
            if (detailId <= 0)
                return BadRequest(new ApiResponseModel<string>(false, "Chi tiết ID không hợp lệ.", null, 400));

            var result = await _repo.DeleteDetail(detailId);
            if (result)
                return Ok(new ApiResponseModel<string>(true, "Đã xoá chi tiết", null, 200));

            return NotFound(new ApiResponseModel<string>(false, "Không tìm thấy chi tiết để xoá.", null, 404));
        }


        // Helper: Lấy CollectionId từ CollectionDetailId
        private async Task<int> GetCollectionIdByDetailId(int detailId)
        {
            var allCollections = await _repo.GetAllCollection();

            var detail = allCollections
                .SelectMany(c => c.CollectionDetails)
                .FirstOrDefault(d => d.CollectionDetailId == detailId);

            return detail?.CollectionId ?? 0;
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDTO dto)
        {
            if (id <= 0)
                return BadRequest("ID không hợp lệ.");

            var exists = await _repo.GetByIdCollection(id);
            if (exists == null)
                return NotFound($"Không tìm thấy Collection với ID = {id}");

            var result = await _repo.UpdateCollectionStatus(id, dto.Status);
            return result
                ? Ok(new ApiResponseModel<string>(true, "Cập nhật trạng thái thành công", null, 200))
                : StatusCode(500, new ApiResponseModel<string>(false, "Cập nhật thất bại", null, 500));
        }

    }
}
