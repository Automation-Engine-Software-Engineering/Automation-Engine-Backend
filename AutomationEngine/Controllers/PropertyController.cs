using DataLayer.Models.TableBuilder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Services;
using ViewModels;
using ViewModels.ViewModels.Entity;

namespace AutomationEngine.Controllers
{
    public class PropertyController : Controller
    {
        private readonly IPropertyService _propertyService;

        public PropertyController(IPropertyService propertyService)
        {
            _propertyService = propertyService;
        }

        // POST: api/entity/{entityName}/peroperty/add  
        [HttpPost("{entityId}/peroperty/add")]
        public async Task<ResultViewModel> AddPeropertyToEntity(int entityId, [FromBody] PropertyDto property)
        {
            var result = new EntityProperty()
            {
                PreviewName = property.PreviewName,
                PropertyName = property.PropertyName,
                SizeHeight = property.SizeHeight,
                SizeWidth = property.SizeWidth,
                EntityId = entityId,
                DefaultValue = property.DefaultValue,
                AllowNull = property.AllowNull,
                Type = property.Type
            };

            await _propertyService.AddColumnToTableAsync(entityId,result);
            await _propertyService.SaveChangesAsync();
            return (new ResultViewModel { Data = result, Message = "عملیات با موفقیت انجام شد", Status = true });
        }

        // POST: api/entity/{entityName}/peroperty/edit  
        [HttpPost("peroperty/update")]
        public async Task<ResultViewModel> updatePeropertyInEntity([FromBody] PropertyDto property)
        {
            var result = new EntityProperty()
            {
                Id = property.Id,
                PreviewName = property.PreviewName,
                PropertyName = property.PropertyName,
                SizeHeight = property.SizeHeight,
                SizeWidth = property.SizeWidth,
                DefaultValue = property.DefaultValue,
                AllowNull = property.AllowNull,
                Type = property.Type
            };
            await _propertyService.UpdatePeropertyInTableAsync(result);
            await _propertyService.SaveChangesAsync();
            return (new ResultViewModel { Data = result, Message = "عملیات با موفقیت انجام شد", Status = true });
        }

        // GET: api/entity/{entityName}/peroperty  
        [HttpGet("{entityId}/peroperty")]
        public async Task<ResultViewModel> GetAllperopertiesFromEntity(int entityId)
        {
            var columns = await _propertyService.GetAllColumnAsyncByEntityId(entityId);
            return (new ResultViewModel { Data = columns, Message = "عملیات با موفقیت انجام شد", Status = true });
        }

        [HttpGet("{entityId}/value")]
        public async Task<ResultViewModel> GetAllperopertiesValueFromEntity(int entityId)
        {
            var columns = await _propertyService.GetColumnValuesAsyncById(entityId);
            return (new ResultViewModel { Data = columns, Message = "عملیات با موفقیت انجام شد", Status = true });
        }

        // GET: api/entity/{entityName}/peroperty  
        [HttpGet("peroperty/{peropertyId}")]
        public async Task<ResultViewModel> GetperopertiesFromEntityById(int peropertyId)
        {
            var columns = await _propertyService.GetColumnAsyncById(peropertyId);
            return (new ResultViewModel { Data = columns, Message = "عملیات با موفقیت انجام شد", Status = true });
        }
    }
}
