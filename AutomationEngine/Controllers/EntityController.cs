using DataLayer.Models.FormBuilder;
using DataLayer.Models.TableBuilder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using Services;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ViewModels;
using ViewModels.ViewModels.Entity;

namespace AutomationEngine.Controllers
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
        public async Task<ResultViewModel> CreateEntity(int? formId, [FromBody] EntityDto entity)
        {
            var result = new Entity()
            {
                PreviewName = entity.PreviewName,
                TableName = entity.TableName,
                Description = entity.Description
            };

            if (formId == null)
            {
                await _entityService.CreateEntityAsync(result);
            }
            else
            {
                await _entityService.CreateEntityAsync(formId.Value, result);
            }
            await _entityService.SaveChangesAsync();
            return (new ResultViewModel { Data = entity, Message = "عملیات با موفقیت انجام شد", Status = true });
        }

        // POST: api/entity/update  
        [HttpPost("update")]
        public async Task<ResultViewModel> UpdateEntity([FromBody] EntityDto entity)
        {
            var result = new Entity()
            {
                Id = entity.Id,
                PreviewName = entity.PreviewName,
                TableName = entity.TableName,
                Description = entity.Description
            };

            await _entityService.UpdateEntityAsync(result);
            await _entityService.SaveChangesAsync();
            return (new ResultViewModel { Data = entity, Message = "عملیات با موفقیت انجام شد", Status = true });
        }

        // POST: api/entity/delete  
        [HttpPost("remove")]
        public async Task<ResultViewModel> RemoveEntity(int entityId)
        {
            await _entityService.RemoveEntityAsync(entityId);
            await _entityService.SaveChangesAsync();
            return (new ResultViewModel { Data = null , Message = "عملیات با موفقیت انجام شد", Status = true });
        }

        // GET: api/entity/all  
        [HttpGet("all")]
        public async Task<ResultViewModel> GetAllEntities(int? formId)
        {
            var entities = await _entityService.GetAllEntitiesAsync();
            return (new ResultViewModel { Data = entities, Message = "عملیات با موفقیت انجام شد", Status = true });
        }

        // GET: api/form/{id}  
        [HttpGet("{enntityId}")]
        public async Task<ResultViewModel> GetEntity(int enntityId)
        {
            var entities = await _entityService.GetEntitiesByIdAsync(enntityId);
            return (new ResultViewModel { Data = entities, Message = "عملیات با موفقیت انجام شد", Status = true });
        }
    }
}