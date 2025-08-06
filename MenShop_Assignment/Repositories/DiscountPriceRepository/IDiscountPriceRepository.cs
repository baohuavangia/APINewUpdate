using MenShop_Assignment.Datas;
using MenShop_Assignment.Models;

namespace MenShop_Assignment.Repositories.DiscountPriceRepository
{
    public interface IDiscountPriceRepository
    {
        Task<bool> CreateDiscount(DiscountPrice discountPrice);
        Task<bool> CreateDiscountDetail(DiscountPriceDetail discountPriceDetail);
        Task<bool> DeleteDiscount(int Id);
        Task<bool> DeleteDiscountDetail(int Id);
        Task<List<DiscountPriceViewModel>> GetAllDiscountPrice();
        Task<DiscountPriceDetailViewModel> GetByIdDiscountDetailPrice(int Id);
        Task<DiscountPriceViewModel> GetByIdDiscountPrice(int Id);
        Task<List<DiscountPriceDetailViewModel>> GetDiscountDetailsByProductDetailId(int productDetailId);
        List<DiscountPriceDetailViewModel> GetProductDetailsByDiscountId(int discountPriceId);
        Task<bool> UpdateDiscount(DiscountPrice discountPrice);
        Task<bool> UpdateDiscountDetail(DiscountPriceDetail discountPrice);
    }
}