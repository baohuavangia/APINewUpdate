using MenShop_Assignment.Datas;
using MenShop_Assignment.Mapper;
using MenShop_Assignment.Models;
using Microsoft.EntityFrameworkCore;

namespace MenShop_Assignment.Repositories.CollectionRepository
{
    public class CollectionRepository : ICollectionRepository
    {
        private readonly ApplicationDbContext _context;

        public CollectionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==== COLLECTION ====

        public async Task<List<CollectionViewModel>> GetAllCollection()
        {
            var collections = await _context.Collections
                .Include(c => c.Images)
                .Include(c => c.CollectionDetails)
                    .ThenInclude(cd => cd.Product)
                .ToListAsync();

            return collections.Select(CollectionMapper.ToCollectionViewModel).ToList();
        }

        public async Task<CollectionViewModel?> GetByIdCollection(int collectionId)
        {
            var collection = await _context.Collections
                .AsSplitQuery()
                .Include(c => c.Images)
                .Include(c => c.CollectionDetails)
                    .ThenInclude(cd => cd.Product)
                        .ThenInclude(p => p.Category)
                .Include(c => c.CollectionDetails)
                    .ThenInclude(cd => cd.Product)
                        .ThenInclude(p => p.ProductDetails)
                            .ThenInclude(pd => pd.Color)
                .Include(c => c.CollectionDetails)
                    .ThenInclude(cd => cd.Product)
                        .ThenInclude(p => p.ProductDetails)
                            .ThenInclude(pd => pd.Size)
                .Include(c => c.CollectionDetails)
                    .ThenInclude(cd => cd.Product)
                        .ThenInclude(p => p.ProductDetails)
                            .ThenInclude(pd => pd.Fabric)
                .FirstOrDefaultAsync(c => c.CollectionId == collectionId);

            if (collection == null) return null;

            return CollectionMapper.ToCollectionViewModel(collection);
        }

        public async Task<bool> AddCollection(Collection collection)
        {
            try
            {
                _context.Collections.Add(collection);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (DbUpdateException)
            {
                // Log lỗi nếu cần
                return false;
            }
        }

        public async Task<bool> UpdateCollection(Collection updatedCollection)
        {
            try
            {
                var existing = await _context.Collections.FindAsync(updatedCollection.CollectionId);
                if (existing == null) return false;

                existing.CollectionName = updatedCollection.CollectionName;
                existing.Description = updatedCollection.Description;
                existing.StartTime = updatedCollection.StartTime;
                existing.EndTime = updatedCollection.EndTime;
                existing.Status = updatedCollection.Status;

                _context.Collections.Update(existing);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }

        public async Task<bool> DeleteCollection(int collectionId)
        {
            try
            {
                var collection = await _context.Collections
                    .Include(c => c.CollectionDetails)
                    .Include(c => c.Images)
                    .FirstOrDefaultAsync(c => c.CollectionId == collectionId);

                if (collection == null) return false;

                if (collection.CollectionDetails != null && collection.CollectionDetails.Any())
                    return false;

                _context.Collections.Remove(collection);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }

        // ==== COLLECTION DETAIL ====

        public async Task<List<CollectionDetailsViewModel>> GetCollectionDetailsByCollectionId(int collectionId)
        {
            var details = await _context.CollectionDetails
                .Include(cd => cd.Product)
                .Where(cd => cd.CollectionId == collectionId)
                .ToListAsync();

            return details.Select(CollectionMapper.ToProductColectionViewModel).ToList();
        }

        public async Task<bool> AddDetail(CollectionDetail detail)
        {
            try
            {
                bool exists = await _context.CollectionDetails
                    .AnyAsync(cd => cd.CollectionId == detail.CollectionId && cd.ProductId == detail.ProductId);

                if (exists) return false;

                _context.CollectionDetails.Add(detail);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }

        public async Task<bool> UpdateDetail(CollectionDetail detail)
        {
            try
            {
                var existing = await _context.CollectionDetails.FindAsync(detail.CollectionDetailId);
                if (existing == null) return false;

                existing.ProductId = detail.ProductId;
                existing.CollectionId = detail.CollectionId;

                _context.CollectionDetails.Update(existing);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }

        public async Task<bool> DeleteDetail(int detailId)
        {
            try
            {
                var detail = await _context.CollectionDetails.FindAsync(detailId);
                if (detail == null) return false;

                _context.CollectionDetails.Remove(detail);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }

        public async Task<bool> UpdateCollectionStatus(int collectionId, bool newStatus)
        {
            try
            {
                var collection = await _context.Collections.FindAsync(collectionId);
                if (collection == null) return false;

                collection.Status = newStatus;
                _context.Collections.Update(collection);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }

    }
}
