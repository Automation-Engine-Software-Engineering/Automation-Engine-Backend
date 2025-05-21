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
                throw new CustomException("Property", "CorruptedProperty");

            if (property == null)
                throw new CustomException("Property", "CorruptedProperty");

            var entity = await _entityService.GetEntitiesByIdAsync(property.EntityId);
            if (entity == null)
                throw new CustomException("Entity", "CorruptedEntity");

			if (!Enum.TryParse(property.Type, true, out PropertyType propertyType))
				throw new CustomException("Property", "CorruptedProperty");


			var result = new EntityProperty(property.PreviewName, property.PropertyName, property.Description, property.DefaultValue, propertyType, entity);

            if (result.Id != 0)
                throw new CustomException("Property", "CorruptedProperty");

            var validationModel = _propertyService.PropertyValidation(result);
            if (!validationModel.IsSuccess) throw validationModel;

            result.PropertyName.IsValidStringCommand();
            result.Description.IsValidString();
            result.DefaultValue.IsValidString();

            //transfer model
            result.Entity = entity;
            result.EntityId = property.EntityId;

            //initial action
            await _propertyService.AddColumnToTableAsync(result);
            await _propertyService.SaveChangesAsync();

            return new ResultViewModel<EntityProperty?>(result);
        }

        // POST: api/property/update  
        [HttpPost("update")]
        public async Task<ResultViewModel<EntityProperty>> updatePropertyInEntity([FromBody] PropertyDto property)
        {
            //is valid model
            if (property == null)
                throw new CustomException("Property", "CorruptedProperty");

            var entity = await _entityService.GetEntitiesByIdAsync(property.EntityId);
            if (entity == null)
                throw new CustomException("Entity", "CorruptedEntity");

			if (!Enum.TryParse(property.Type, true, out PropertyType propertyType))
				throw new CustomException("Property", "CorruptedProperty");

			var result = new EntityProperty(property.PropertyName, property.PropertyName, property.Description, property.DefaultValue, propertyType, entity);

            if (entity.Id == 0)
                throw new CustomException("Property", "CorruptedProperty");

            result.Id = entity.Id;

            var validationModel = _propertyService.PropertyValidation(result);
            if (!validationModel.IsSuccess)
                throw validationModel;

            var fetchModel = await _propertyService.GetColumnByIdAsync(property.Id);
            if (fetchModel == null)
                throw new CustomException("Property", "CorruptedProperty");

            result.PropertyName.IsValidStringCommand();

            //transfer moel
            fetchModel.PreviewName = result.PreviewName;
            fetchModel.PropertyName = result.PropertyName;
            fetchModel.Description = result.Description;
            fetchModel.DefaultValue = result.DefaultValue;
            fetchModel.Type = result.Type;

            //initial action
            await _propertyService.UpdateColumnInTableAsync(fetchModel);
            await _propertyService.SaveChangesAsync();

            return new ResultViewModel<EntityProperty>(fetchModel);
        }

        // POST: api/property/remove  
        [HttpPost("remove")]
        public async Task<ResultViewModel<EntityProperty?>> removePropertyInEntity([FromBody] int propertyId)
        {
            //is valid model
            if (propertyId == 0)
                throw new CustomException("Property", "CorruptedProperty");

            var property = await _propertyService.GetColumnByIdAsync(propertyId);
            if (property == null)
                throw new CustomException("Property", "CorruptedProperty");

            var validationModel = _propertyService.PropertyValidation(property);
            if (!validationModel.IsSuccess)
                throw validationModel;

            //initial action
            await _propertyService.RemoveColumnByIdAsync(property.Id);
            await _propertyService.SaveChangesAsync();

            return new ResultViewModel<EntityProperty?>(property);
        }

        // GET: api/property/propertyId
        [HttpGet("{propertyId}")]
        public async Task<ResultViewModel<EntityProperty?>> GetPropertyById(int propertyId)
        {
            if (propertyId == 0)
                throw new CustomException("Form", "CorruptedProperty");

            var column = await _propertyService.GetColumnByIdAsync(propertyId);
            if (column == null)
                throw new CustomException("Form", "CorruptedProperty");

            var validationModel = _propertyService.PropertyValidation(column);
            if (!validationModel.IsSuccess)
                throw validationModel;

            return new ResultViewModel<EntityProperty?>(column);
        }


        // GET: api/property/All
        [HttpGet("all")]
        public async Task<ResultViewModel<IEnumerable<EntityProperty>>> GetAllProperty(int pageSize, int pageNumber)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            var column = await _propertyService.GetAllColumnsAsync(pageSize, pageNumber);

            return new ResultViewModel<IEnumerable<EntityProperty>>(column.Data);
        }

    }
}
