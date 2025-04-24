using DataLayer.DbContext;
using Entities.Models.MainEngine;
using Entities.Models.TableBuilder;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace Services
{
    public interface IEntityRelationService
    {
        Task InsertRangeEntityRelation(List<Entity_EntityRelation> Entity_EntityRelations);
        Task ReplaceEntityRelationsByEntityId(int EntityId, List<int> EntityIds);
        Task<ValidationDto<string>> SaveChangesAsync();
        Task<ListDto<Entity_EntityRelation>> GetEntityRelationByParentId(int parentId, int pageSize, int pageNumber);
    }

    public class Entity_EntityRelationService : IEntityRelationService
    {
        private readonly Context _context;

        public Entity_EntityRelationService(Context context)
        {
            _context = context;
        }

        public async Task InsertRangeEntityRelation(List<Entity_EntityRelation> Entity_EntityRelations)
        {
            await _context.Entity_EntityRelation.AddRangeAsync(Entity_EntityRelations);
        }

        public async Task ReplaceEntityRelationsByEntityId(int EntityId, List<int> EntityIds)
        {
            var relations = await _context.Entity_EntityRelation.Where(x => x.ParentId == EntityId).ToListAsync();
            _context.Entity_EntityRelation.RemoveRange(relations);

            var newRelation = EntityIds.Select(x => new Entity_EntityRelation
            {
                ParentId = EntityId,
                ChildId = x
            }).ToList();
            await InsertRangeEntityRelation(newRelation);
        }
        public async Task<ValidationDto<string>> SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                return new ValidationDto<string>(true, "Success", "ChangesSaved", null);
            }
            catch (Exception ex)
            {
                return new ValidationDto<string>(false, "Error", "SaveFailed", ex.Message);
            }
        }
        public async Task<ListDto<Entity_EntityRelation>> GetEntityRelationByParentId(int parentId, int pageSize, int pageNumber)
        {
            var query = _context.Entity_EntityRelation.Where(x => x.ParentId == parentId);
            var count = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new ListDto<Entity_EntityRelation>(items, count, pageSize, pageNumber);
        }
    }
}