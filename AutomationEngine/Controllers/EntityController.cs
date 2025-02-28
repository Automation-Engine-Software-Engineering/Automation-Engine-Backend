using DataLayer.Models.FormBuilder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Services;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ViewModels;

namespace AutomationEngine.Controllers// Replace with your actual namespace  
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntityController : ControllerBase
    {
        private readonly IEntityService _entityService;

        public EntityController(IEntityService entityService)
        {
            _entityService = entityService;
        }

        // POST: api/entity/create  
        [HttpPost("create")]
        public async Task<ResultViewModel> CreateEntity([FromBody] Entity entity)
        {
            if (entity == null)
                throw new ArgumentNullException("عنصر یافت نشد");

            await _entityService.CreateEntityAsync(entity);
            return (new ResultViewModel { Data = entity, Message = "عملیات با موفقیت انجام شد", Status = true });
        }

        // POST: api/entity/edit  
        [HttpPost("update")]
        public async Task<ResultViewModel> UpdateEntity([FromBody] Entity entity)
        {
            if (entity == null)
                throw new ArgumentNullException("عنصر یافت نشد");

            await _entityService.UpdateEntityAsync(entity);
            return (new ResultViewModel { Data = entity, Message = "عملیات با موفقیت انجام شد", Status = true });
        }

        // POST: api/entity/delete  
        [HttpPost("remove")]
        public async Task<ResultViewModel> RemoveEntity([FromBody] int entityId)
        {
            if (entityId == null)
                throw new ArgumentNullException("عنصر یافت نشد");

            var result = await _entityService.GetEntitiesByIdAsync(entityId);
            await _entityService.RemoveEntityAsync(result);
            return (new ResultViewModel { Data = result, Message = "عملیات با موفقیت انجام شد", Status = true });
        }

        // GET: api/entity/all  
        [HttpGet("all")]
        public async Task<ResultViewModel> GetAllEntities()
        {
            var entities = await _entityService.GetAllEntitiesAsync();
            return (new ResultViewModel { Data = entities, Message = "عملیات با موفقیت انجام شد", Status = true });
        }

        // POST: api/entity/{entityName}/peroperty/add  
        [HttpPost("{entityName}/peroperty/add")]
        public async Task<ResultViewModel> AddPeropertyToEntity( int entityId, [FromBody] List<Peroperty> peroperties)
        {
            if (entityId == null || peroperties == null)
                throw new ArgumentNullException("عنصر یافت نشد");

            var result = await _entityService.GetEntitiesByIdAsync(entityId);
            await _entityService.AddColumnToTableAsync(result, peroperties);
            return (new ResultViewModel { Data = result, Message = "عملیات با موفقیت انجام شد", Status = true });
        }

        // POST: api/entity/{entityName}/peroperty/edit  
        [HttpPost("{entityName}/peroperty/edit")]
        public async Task<ResultViewModel> EditperopertyInEntity([FromBody] Peroperty peroperty)
        {
            if (peroperty == null)
                throw new ArgumentNullException("عنصر یافت نشد");

            await _entityService.UpdatePeropertyInTableAsync(peroperty);
            return (new ResultViewModel { Data = peroperty, Message = "عملیات با موفقیت انجام شد", Status = true });
        }

        // POST: api/entity/{entityName}/peroperty/delete  
        //[HttpPost("{entityName}/peroperty/remove")]
        //public async Task<ResultViewModel> removeperopertyFromEntity(string entityName, [FromBody] string columnName)
        //{
        //    await _entityService.();
        //    return (new ResultViewModel { Data = peroperty, Message = "عملیات با موفقیت انجام شد", Status = true });
        //}

        // GET: api/entity/{entityName}/peroperty  
        [HttpGet("{entityName}/peroperty")]
        public async Task<ResultViewModel> GetAllperopertiesFromEntity(int entityId)
        {
            if (entityId == null)
                throw new ArgumentNullException("عنصر یافت نشد");

            var columns = await _entityService.GetAllColumnValuesAsync(entityId);
            return (new ResultViewModel { Data = columns, Message = "عملیات با موفقیت انجام شد", Status = true });
        }

        // GET: api/entity/{entityName}/peroperty  
        [HttpGet("{entityName}/peroperty/{peropertyId}")]
        public async Task<ResultViewModel> GetAllperopertiesFromEntityById(int peropertyId)
        {
            if (peropertyId == null)
                throw new ArgumentNullException("عنصر یافت نشد");

            var columns = await _entityService.GetColumnValuesAsync(peropertyId);
            return (new ResultViewModel { Data = columns, Message = "عملیات با موفقیت انجام شد", Status = true });
        }

    }
}