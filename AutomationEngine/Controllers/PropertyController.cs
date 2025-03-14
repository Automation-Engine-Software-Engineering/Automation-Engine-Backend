using DataLayer.Models.TableBuilder;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Services;
using Tools;
using ViewModels;
using ViewModels.ViewModels.Entity;

namespace AutomationEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropertyController : Controller
    {
        private readonly IPropertyService _propertyService;
        private readonly IEntityService _entityService;
        public PropertyController(IEntityService entityService, IPropertyService propertyService)
        {
            _entityService = entityService;
            _propertyService = propertyService;
        }

        // POST: api/entity/{entityId}/property/add  
        [HttpPost("entity/property/add")]
        public async Task<ResultViewModel> AddpropertyToEntity([FromBody] PropertyDto property)
        {
            //is valid model
            if (property.EntityId == 0)
                throw new CustomException<EntityProperty>(new ValidationDto<EntityProperty>(false, "Property", "CorruptedProperty", null), 500);

            if (property == null)
                throw new CustomException<EntityProperty>(new ValidationDto<EntityProperty>(false, "Property", "CorruptedProperty", null), 500);

            var entity = await _entityService.GetEntitiesByIdAsync(property.EntityId.Value);
            if (entity == null)
                throw new CustomException<Entity>(new ValidationDto<Entity>(false, "Entity", "CorruptedEntity", null), 500);

            var result = new EntityProperty(property.PropertyName, property.PropertyName, property.Description, property.AllowNull, property.DefaultValue, property.Type, entity);

            if (result.Id != 0)
                throw new CustomException<EntityProperty>(new ValidationDto<EntityProperty>(false, "Property", "CorruptedProperty", null), 500);

            var validationModel = await _propertyService.PropertyValidation(result);
            if (!validationModel.IsSuccess)
                throw new CustomException<EntityProperty>(validationModel, 500);

            result.PropertyName.IsValidateStringCommand();
            result.Description.IsValidateString();
            result.DefaultValue.IsValidateString();

            //transfer model
            result.Entity = entity;
            result.EntityId = property.EntityId.Value;

            //initial action
            await _propertyService.AddColumnToTableAsync(result);
            var saveResult = await _propertyService.SaveChangesAsync();
            if (!saveResult.IsSuccess)
                throw new CustomException<string>(saveResult, 500);

            return (new ResultViewModel { Data = result, Message = new ValidationDto<EntityProperty>(true, "Success", "Success", result).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // POST: api/property/update  
        [HttpPost("property/update")]
        public async Task<ResultViewModel> updatepropertyInEntity([FromBody] PropertyDto property)
        {
            //is valid model
            if (property == null)
                throw new CustomException<EntityProperty>(new ValidationDto<EntityProperty>(false, "Property", "CorruptedProperty", null), 500);

            var entity = await _entityService.GetEntitiesByIdAsync(property.EntityId.Value);
            if (entity == null)
                throw new CustomException<Entity>(new ValidationDto<Entity>(false, "Entity", "CorruptedEntity", null), 500);

            var result = new EntityProperty(property.PropertyName, property.PropertyName, property.Description, property.AllowNull, property.DefaultValue, property.Type, entity);

            if (entity.Id == 0)
                throw new CustomException<EntityProperty>(new ValidationDto<EntityProperty>(false, "Property", "CorruptedProperty", null), 500);
            
            result.Id = entity.Id;
            
            var validationModel = await _propertyService.PropertyValidation(result);
            if (!validationModel.IsSuccess)
                throw new CustomException<EntityProperty>(validationModel, 500);

            var fetchModel = await _propertyService.GetColumnByIdAsync(property.Id);
            if (fetchModel == null)
                throw new CustomException<EntityProperty>(new ValidationDto<EntityProperty>(false, "Property", "CorruptedProperty", null), 500);

            result.PropertyName.IsValidateStringCommand();

            //transfer moel
            fetchModel.PreviewName = result.PreviewName;
            fetchModel.PropertyName = result.PropertyName;
            fetchModel.Description = result.Description;
            fetchModel.AllowNull = result.AllowNull;
            fetchModel.DefaultValue = result.DefaultValue;
            fetchModel.Type = result.Type;

            //initial action
            await _propertyService.UpdateColumnInTableAsync(fetchModel);
            var saveResult = await _propertyService.SaveChangesAsync();
            if (!saveResult.IsSuccess)
                throw new CustomException<string>(saveResult, 500);

            return (new ResultViewModel { Data = fetchModel, Message = new ValidationDto<EntityProperty>(true, "Success", "Success", fetchModel).GetMessage(200), Status = true, StatusCode = 200 });
        }

      
        // GET: api/property/propertyId
        [HttpGet("{propertyId}")]
        public async Task<ResultViewModel> GetPropertyById(int propertyId)
        {
            if (propertyId == 0)
                throw new CustomException<EntityProperty>(new ValidationDto<EntityProperty>(false, "Form", "CorruptedProperty", null), 500);

            var column = await _propertyService.GetColumnByIdAsync(propertyId);
            if (column == null)
                throw new CustomException<EntityProperty>(new ValidationDto<EntityProperty>(false, "Form", "CorruptedProperty", null), 500);

            var validationModel = await _propertyService.PropertyValidation(column);
            if (!validationModel.IsSuccess)
                throw new CustomException<EntityProperty>(validationModel, 500);

            return (new ResultViewModel { Data = column, Message = new ValidationDto<EntityProperty>(true, "Success", "Success", column).GetMessage(200), Status = true, StatusCode = 200 });
        }
    }
}
