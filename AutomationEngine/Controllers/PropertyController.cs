using AutomationEngine.ControllerAttributes;
using Entities.Models.Enums;
using Entities.Models.TableBuilder;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Services;
using Tools.TextTools;
using ViewModels;
using ViewModels.ViewModels.Entity;

namespace AutomationEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [CheckAccess]
    public class PropertyController : Controller
    {
        private readonly IPropertyService _propertyService;
        private readonly IEntityService _entityService;
        public PropertyController(IEntityService entityService, IPropertyService propertyService)
        {
            _entityService = entityService;
            _propertyService = propertyService;
        }
        //TODO : محدودیت تغییر رمز
        //TODO : نوع خروجی مشخص شود
        //TODO : Ctor مدل ها تغییر یابد

        // POST: api/entity/{entityId}/property/add  
        [HttpPost("add")]
        public async Task<ResultViewModel<EntityProperty?>> AddPropertyToEntity([FromBody] PropertyDto property)
        {
            //is valid model
            if (property.EntityId == 0)
                throw new CustomException<EntityProperty>(new ValidationDto<EntityProperty>(false, "Property", "CorruptedProperty", null), 500);

            if (property == null)
                throw new CustomException<EntityProperty>(new ValidationDto<EntityProperty>(false, "Property", "CorruptedProperty", null), 500);

            var entity = await _entityService.GetEntitiesByIdAsync(property.EntityId);
            if (entity == null)
                throw new CustomException<Entity>(new ValidationDto<Entity>(false, "Entity", "CorruptedEntity", null), 500);

			if (!Enum.TryParse(property.Type, true, out PropertyType propertyType))
				throw new CustomException<EntityProperty>(new ValidationDto<EntityProperty>(false, "Property", "CorruptedProperty", null), 500);


			var result = new EntityProperty(property.PropertyName, property.PropertyName, property.Description, property.DefaultValue, propertyType, entity);

            if (result.Id != 0)
                throw new CustomException<EntityProperty>(new ValidationDto<EntityProperty>(false, "Property", "CorruptedProperty", null), 500);

            var validationModel = _propertyService.PropertyValidation(result);
            if (!validationModel.IsSuccess)
                throw new CustomException<EntityProperty>(validationModel, 500);

            result.PropertyName.IsValidStringCommand();
            result.Description.IsValidString();
            result.DefaultValue.IsValidString();

            //transfer model
            result.Entity = entity;
            result.EntityId = property.EntityId;

            //initial action
            await _propertyService.AddColumnToTableAsync(result);
            var saveResult = await _propertyService.SaveChangesAsync();
            if (!saveResult.IsSuccess)
                throw new CustomException<string>(saveResult, 500);

            return (new ResultViewModel<EntityProperty?> { Data = result, Message = new ValidationDto<EntityProperty>(true, "Success", "Success", result).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // POST: api/property/update  
        [HttpPost("update")]
        public async Task<ResultViewModel<EntityProperty>> updatePropertyInEntity([FromBody] PropertyDto property)
        {
            //is valid model
            if (property == null)
                throw new CustomException<EntityProperty>(new ValidationDto<EntityProperty>(false, "Property", "CorruptedProperty", null), 500);

            var entity = await _entityService.GetEntitiesByIdAsync(property.EntityId);
            if (entity == null)
                throw new CustomException<Entity>(new ValidationDto<Entity>(false, "Entity", "CorruptedEntity", null), 500);

			if (!Enum.TryParse(property.Type, true, out PropertyType propertyType))
				throw new CustomException<EntityProperty>(new ValidationDto<EntityProperty>(false, "Property", "CorruptedProperty", null), 500);

			var result = new EntityProperty(property.PropertyName, property.PropertyName, property.Description, property.DefaultValue, propertyType, entity);

            if (entity.Id == 0)
                throw new CustomException<EntityProperty>(new ValidationDto<EntityProperty>(false, "Property", "CorruptedProperty", null), 500);

            result.Id = entity.Id;

            var validationModel = _propertyService.PropertyValidation(result);
            if (!validationModel.IsSuccess)
                throw new CustomException<EntityProperty>(validationModel, 500);

            var fetchModel = await _propertyService.GetColumnByIdAsync(property.Id);
            if (fetchModel == null)
                throw new CustomException<EntityProperty>(new ValidationDto<EntityProperty>(false, "Property", "CorruptedProperty", null), 500);

            result.PropertyName.IsValidStringCommand();

            //transfer moel
            fetchModel.PreviewName = result.PreviewName;
            fetchModel.PropertyName = result.PropertyName;
            fetchModel.Description = result.Description;
            fetchModel.DefaultValue = result.DefaultValue;
            fetchModel.Type = result.Type;

            //initial action
            await _propertyService.UpdateColumnInTableAsync(fetchModel);
            var saveResult = await _propertyService.SaveChangesAsync();
            if (!saveResult.IsSuccess)
                throw new CustomException<string>(saveResult, 500);

            return (new ResultViewModel<EntityProperty> { Data = fetchModel, Message = new ValidationDto<EntityProperty>(true, "Success", "Success", fetchModel).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // POST: api/property/remove  
        [HttpPost("remove")]
        public async Task<ResultViewModel<EntityProperty?>> removePropertyInEntity([FromBody] int propertyId)
        {
            //is valid model
            if (propertyId == 0)
                throw new CustomException<EntityProperty>(new ValidationDto<EntityProperty>(false, "Property", "CorruptedProperty", null), 500);

            var property = await _propertyService.GetColumnByIdAsync(propertyId);
            if (property == null)
                throw new CustomException<EntityProperty>(new ValidationDto<EntityProperty>(false, "Property", "CorruptedProperty", null), 500);

            var validationModel = _propertyService.PropertyValidation(property);
            if (!validationModel.IsSuccess)
                throw new CustomException<EntityProperty>(validationModel, 500);

            //initial action
            await _propertyService.RemoveColumnByIdAsync(property.Id);
            var saveResult = await _propertyService.SaveChangesAsync();
            if (!saveResult.IsSuccess)
                throw new CustomException<string>(saveResult, 500);

            return (new ResultViewModel<EntityProperty?> { Data = property, Message = new ValidationDto<EntityProperty>(true, "Success", "Success", property).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // GET: api/property/propertyId
        [HttpGet("{propertyId}")]
        public async Task<ResultViewModel<EntityProperty?>> GetPropertyById(int propertyId)
        {
            if (propertyId == 0)
                throw new CustomException<EntityProperty>(new ValidationDto<EntityProperty>(false, "Form", "CorruptedProperty", null), 500);

            var column = await _propertyService.GetColumnByIdAsync(propertyId);
            if (column == null)
                throw new CustomException<EntityProperty>(new ValidationDto<EntityProperty>(false, "Form", "CorruptedProperty", null), 500);

            var validationModel = _propertyService.PropertyValidation(column);
            if (!validationModel.IsSuccess)
                throw new CustomException<EntityProperty>(validationModel, 500);

            return (new ResultViewModel<EntityProperty?> { Data = column, Message = new ValidationDto<EntityProperty>(true, "Success", "Success", column).GetMessage(200), Status = true, StatusCode = 200 });
        }


        // GET: api/property/All
        [HttpGet("all")]
        public async Task<ResultViewModel<IEnumerable<EntityProperty>>> GetAllProperty(int pageSize, int pageNumber)
        {
            var column = await _propertyService.GetAllColumnsAsync(pageSize, pageNumber);

            //is valid data
            if ((((pageSize * pageNumber) - column.TotalCount) > pageSize) && (pageSize * pageNumber) > column.TotalCount)
                throw new CustomException<ListDto<EntityProperty>>(new ValidationDto<ListDto<EntityProperty>>(false, "Form", "CorruptedEntity", column), 500);

            return (new ResultViewModel<IEnumerable<EntityProperty>> { Data = column.Data, Message = new ValidationDto<ListDto<EntityProperty>>(true, "Success", "Success", column).GetMessage(200), Status = true, StatusCode = 200 });
        }

    }
}
