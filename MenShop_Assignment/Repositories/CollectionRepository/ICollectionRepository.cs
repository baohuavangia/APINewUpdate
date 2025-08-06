using MenShop_Assignment.Datas;
using MenShop_Assignment.Models;

namespace MenShop_Assignment.Repositories.CollectionRepository
{
    public interface ICollectionRepository
    {
        Task<bool> AddCollection(Collection collection);
        Task<bool> AddDetail(CollectionDetail detail);
        Task<bool> DeleteCollection(int collectionId);
        Task<bool> DeleteDetail(int detailId);
        Task<List<CollectionViewModel>> GetAllCollection();
        Task<CollectionViewModel?> GetByIdCollection(int collectionId);
        Task<List<CollectionDetailsViewModel>> GetCollectionDetailsByCollectionId(int collectionId);
        Task<bool> UpdateCollection(Collection updatedCollection);
        Task<bool> UpdateDetail(CollectionDetail detail);
        Task<bool> UpdateCollectionStatus(int collectionId, bool newStatus);
    }
}