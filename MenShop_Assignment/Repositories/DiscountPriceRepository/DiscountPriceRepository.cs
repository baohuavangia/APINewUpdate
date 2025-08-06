using MenShop_Assignment.Datas;
using MenShop_Assignment.DTOs;
using MenShop_Assignment.Mapper;
using MenShop_Assignment.Models;
using Microsoft.EntityFrameworkCore;

namespace MenShop_Assignment.Repositories.DiscountPriceRepository
{
    public class DiscountPriceRepository : IDiscountPriceRepository
    {
        private readonly ApplicationDbContext _context;

        public DiscountPriceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DiscountPriceViewModel>> GetAllDiscountPrice()
        {
            var discountPrice = await _context.DiscountPrices.ToListAsync();
            return discountPrice.Select(DiscountPriceMapper.ToDiscountPriceViewModel).ToList();
        }

        public async Task<DiscountPriceViewModel> GetByIdDiscountPrice(int Id)
        {
            var discountPrice = await _context.DiscountPrices.Where(x => x.Id == Id).FirstOrDefaultAsync();
            return DiscountPriceMapper.ToDiscountPriceViewModel(discountPrice);
        }

        public async Task<bool> CreateDiscount(DiscountPrice discountPrice)
        {
            try
            {
                _context.DiscountPrices.Add(discountPrice);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> UpdateDiscount(DiscountPrice discountPrice)
        {
            try
            {
                var existing = await _context.DiscountPrices.FindAsync(discountPrice.Id);
                if (existing == null) return false;
                {
                    existing.Name = discountPrice.Name;
                    existing.DiscountPercent = discountPrice.DiscountPercent;
                    existing.StartTime = discountPrice.StartTime;
                    existing.EndTime = discountPrice.EndTime;

                    _context.DiscountPrices.Update(existing);
                    return await _context.SaveChangesAsync() > 0;
                }

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> DeleteDiscount(int Id)
        {
            try
            {
                var discountPrice = await _context.DiscountPrices.Include(c => c.discountPriceDetails).FirstOrDefaultAsync(c => c.Id == Id);
                if (discountPrice == null) return false;

                if (discountPrice.discountPriceDetails != null && discountPrice.discountPriceDetails.Any())
                    return false;

                _context.DiscountPrices.Remove(discountPrice);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public async Task<DiscountPriceDetailViewModel> GetByIdDiscountDetailPrice(int Id)
        {
            var discountPrice = await _context.DiscountPriceDetails.Where(x => x.Id == Id).FirstOrDefaultAsync();
            return DiscountPriceMapper.ToDiscountDetailById(discountPrice);
        }
        public List<DiscountPriceDetailViewModel> GetProductDetailsByDiscountId(int discountPriceId)
        {
            var discountDetails = _context.DiscountPriceDetails
                .Where(d => d.discountPriceId == discountPriceId)
                .Include(d => d.ProductDetail)
                    .ThenInclude(p => p.Product)
                .Include(d => d.ProductDetail)
                    .ThenInclude(p => p.Color)
                .Include(d => d.ProductDetail)
                    .ThenInclude(p => p.Size)
                .Include(d => d.ProductDetail)
                    .ThenInclude(p => p.Fabric)
                .ToList();

            var result = discountDetails
                .Select(DiscountPriceMapper.ToDiscountById)
                .ToList();

            return result;
        }
        public async Task<bool> CreateDiscountDetail(DiscountPriceDetail discountPriceDetail)
        {
            try
            {
                // Kiểm tra nếu productDetailId đã tồn tại trong bảng DiscountPriceDetails
                var isExisted = await _context.DiscountPriceDetails
                    .AnyAsync(x => x.productDetailId == discountPriceDetail.productDetailId);

                if (isExisted)
                {
                    // Chi tiết sản phẩm đã có khuyến mãi → không cho thêm mới
                    return false;
                }

                _context.DiscountPriceDetails.Add(discountPriceDetail);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                // Ghi log nếu cần
                return false;
            }
        }
        public async Task<bool> UpdateDiscountDetail(DiscountPriceDetail discountPrice)
        {
            try
            {
                var existing = await _context.DiscountPriceDetails.FindAsync(discountPrice.Id);
                if (existing == null) return false;
                {
                    existing.discountPriceId = discountPrice.discountPriceId;
                    existing.productDetailId = discountPrice.productDetailId;


                    _context.DiscountPriceDetails.Update(existing);
                    return await _context.SaveChangesAsync() > 0;
                }

            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public async Task<bool> DeleteDiscountDetail(int Id)
        {
            try
            {
                var discountPrice = await _context.DiscountPriceDetails.FindAsync(Id);
                if (discountPrice == null) return false;

                _context.DiscountPriceDetails.Remove(discountPrice);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public async Task<List<DiscountPriceDetailViewModel>> GetDiscountDetailsByProductDetailId(int productDetailId)
        {
            var details = await _context.DiscountPriceDetails
                .Where(d => d.productDetailId == productDetailId)
                .Include(d => d.DiscountPrice) // nếu bạn muốn hiển thị thông tin khuyến mãi
                .Include(d => d.ProductDetail)
                    .ThenInclude(p => p.Product)
                .Include(d => d.ProductDetail)
                    .ThenInclude(p => p.Color)
                .Include(d => d.ProductDetail)
                    .ThenInclude(p => p.Size)
                .Include(d => d.ProductDetail)
                    .ThenInclude(p => p.Fabric)
                .ToListAsync();

            return details.Select(DiscountPriceMapper.ToDiscountById).ToList();
        }

    }
}
