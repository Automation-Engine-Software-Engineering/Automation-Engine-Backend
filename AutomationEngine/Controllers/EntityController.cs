using AutomationEngine.ControllerAttributes;
using Entities.Models.FormBuilder;
using Entities.Models.TableBuilder;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SqlServer.Server;
using Services;
using Swashbuckle.AspNetCore.Annotations;
using Tools.TextTools;
using ViewModels;
using ViewModels.ViewModels.Entity;

namespace AutomationEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [CheckAccess]
    public class EntityController : ControllerBase
    {
        private readonly IEntityService _entityService;
        private readonly IPropertyService _propertyService;
        private readonly IFormService _formService;

        public EntityController(IEntityService entityService, IPropertyService propertyService, IFormService formService)
        {
            _entityService = entityService;
            _propertyService = propertyService;
            _formService = formService;
        }

        // POST: api/entity/create  
        [HttpPost("create")]
        public async Task<ResultViewModel<Entity?>> CreateEntity(int? formId, [FromBody] EntityDto entity)
        {
            if (entity == null)
                throw new CustomException("Entity", "CorruptedEntity");

            //transfer model
            var result = new Entity(entity.PreviewName, entity.TableName, entity.Description, new List<EntityProperty>(), new List<Form>());

            //is validation model
            if (entity.Id != 0)
                throw new CustomException("Entity", "CorruptedEntity");

            var validationModel = _entityService.EntityValidation(result);
            if (!validationModel.IsSuccess)
                throw validationModel;

            result.TableName.IsValidStringCommand();

            //initial action
            if (formId == null)
            {
                await _entityService.CreateEntityAsync(result);
            }
            else
            {
                var form = await _formService.GetFormByIdAsync(formId.Value);
                if (form == null)
                    throw new CustomException("Form", "FormNotfound", result);

                await _entityService.CreateEntityAsync(formId.Value, result);
            }
            await _entityService.SaveChangesAsync();

            return new ResultViewModel<Entity?>(result);
        }

        // POST: api/entity/update  
        [HttpPost("update")]
        public async Task<ResultViewModel<Entity?>> UpdateEntity([FromBody] EntityDto entity)
        {
            if (entity == null)
                throw new CustomException("Entity", "CorruptedEntity");

            var result = new Entity(entity.PreviewName, entity.TableName, entity.Description, new List<EntityProperty>(), new List<Form>());

            //is validation model
            if (entity.Id == 0)
                throw new CustomException("Entity", "CorruptedEntity");

            result.Id = entity.Id;
            var validationModel = _entityService.EntityValidation(result);
            if (!validationModel.IsSuccess)
                throw validationModel;

            var fetchEntity = await _entityService.GetEntitiesByIdAsync(entity.Id);
            if (fetchEntity == null)
                throw new CustomException("Entity", "EntityNotFound");

            result.TableName.IsValidStringCommand();

            //initial action
            await _entityService.UpdateEntityAsync(result);
            await _entityService.SaveChangesAsync();

            return new ResultViewModel<Entity?> (result);
        }

        // POST: api/entity/remove  
        [HttpPost("remove")]
        public async Task<ResultViewModel<Entity?>> RemoveEntity(int entityId)
        {
            //is validation model
            if (entityId == 0)
                throw new CustomException("Entity", "EntityNotFound");

            var fetchEntity = await _entityService.GetEntitiesByIdAsync(entityId);
            if (fetchEntity == null)
                throw new CustomException("Entity", "EntityNotFound");

            var validationModel = _entityService.EntityValidation(fetchEntity);
            if (!validationModel.IsSuccess)
                throw validationModel;

            //initial action
            await _entityService.RemoveEntityAsync(entityId);
            await _entityService.SaveChangesAsync();

            return new ResultViewModel<Entity?>(fetchEntity);
        }

        // GET: api/entity/all  
        [HttpGet("all")]
        [SwaggerOperation(Summary = "", Description = "set formId and search null if you don't want to filter result")]
        public async Task<ResultViewModel<IEnumerable<Entity>?>> GetAllEntities(int pageSize, int pageNumber, int? formId, string? search)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            var entities = await _entityService.GetAllEntitiesAsync(pageSize, pageNumber, search, formId);

            //is valid data
            if ((((pageSize * pageNumber) - entities.TotalCount) > pageSize) && (pageSize * pageNumber) > entities.TotalCount)
                throw new CustomException("Form", "CorruptedEntity", entities);
            return new ResultViewModel<IEnumerable<Entity>?> { Data = entities.Data, ListNumber = entities.ListNumber, ListSize = entities.ListSize, TotalCount = entities.TotalCount };
        }

        // GET: api/entity/{id}  
        [HttpGet("{entityId}")]
        public async Task<ResultViewModel<Entity?>> GetEntity(int entityId)
        {
            //is validation model
            if (entityId == 0)
                throw new CustomException("Entity", "EntityNotFound");

            //initial action
            var entities = await _entityService.GetEntitiesByIdAsync(entityId);
            var fetchEntity = await _entityService.GetEntitiesByIdAsync(entityId);
            if (fetchEntity == null)
                throw new CustomException("Entity", "EntityNotFound");
            var validationModel = _entityService.EntityValidation(fetchEntity);
            if (!validationModel.IsSuccess)
                throw validationModel;

            return new ResultViewModel<Entity?>(entities);
        }


        //// GET: api/entity/{id}/value  
        //[HttpGet("entity/{entityId}/value")]
        //public async Task<ResultViewModel> GetEntityValue(int entityId, int pageSize, int pageNumber)
        //{
        //    if (pageSize > 100)
        //        pageSize = 100;
        //    if (pageNumber < 1)
        //        pageNumber = 1;

        //    //is validation model
        //    if (entityId == 0)
        //        throw new CustomException("Entity", "EntityNotFound");

        //    //var entities = await _entityService.GetEntitiesByIdAsync(entityId);

        //    //initial action
        //    var fetchEntity = await _entityService.GetEntitiesByIdAsync(entityId);
        //    if (fetchEntity == null)
        //        throw new CustomException("Entity", "EntityNotFound");

        //    var validationModel = await _entityService.EntityValidation(fetchEntity);
        //    if (!validationModel.IsSuccess)
        //        throw new CustomException<Entity>(validationModel, 500);

        //    //initial action
        //    var result = new EntityValueDto();
        //    if (fetchEntity.Properties.Count == 0)
        //        throw new CustomException("Property", "PropertyNotFound");

        //    var header = fetchEntity.Properties.Select(x => x.PreviewName).ToList();
        //    //var dtoListHeader = new ListDto<string>(header, header.Count, header.Count, 1);
        //    result.Header = header;
        //    var properties = await _propertyService.GetColumnValuesByIdAsync(, pageSize, pageNumber);

        //    //is valid data
        //    if ((((pageSize * pageNumber) - properties.TotalCount) > pageSize) && (pageSize * pageNumber) > properties.TotalCount)
        //        throw new CustomException<ListDto<Dictionary<string, object>>>(new CustomException<ListDto<Dictionary<string, object>>>(false, "Entity", "CorruptedEntity", properties);

        //    result.Body = properties.Data;
        //    return (new ResultViewModel { ListNumber = properties.ListNumber, ListSize = properties.ListSize, TotalCount = properties.TotalCount, Data = result)EntityValueDto
        //}


        // GET: api/entity/{entityName}/property  
        [HttpGet("{entityId}/property")]
        public async Task<ResultViewModel<IEnumerable<EntityProperty>?>> GetAllPropertiesFromEntity(int entityId, int pageSize, int pageNumber)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            if (entityId == 0)
                throw new CustomException("Property", "CorruptedProperty");

            var entity = await _entityService.GetEntitiesByIdAsync(entityId);
            if (entity == null)
                throw new CustomException("Entity", "CorruptedEntity");

            var columns = await _propertyService.GetAllColumnByEntityIdAsync(entityId, pageSize, pageNumber);

            //is valid data
            if ((((pageSize * pageNumber) - columns.TotalCount) > pageSize) && (pageSize * pageNumber) > columns.TotalCount)
                throw new CustomException("Property", "CorruptedProperty", columns);

            return new ResultViewModel<IEnumerable<EntityProperty>?> { Data = columns.Data, ListNumber = columns.ListNumber, ListSize = columns.ListSize, TotalCount = columns.TotalCount };
        }

        // GET: api/property/entityId/value
        [HttpGet("{entityId}/property/value")]
        public async Task<ResultViewModel<IEnumerable<Dictionary<string, object>>>> GetAllPropertiesValueFromEntity(int entityId, int pageSize, int pageNumber)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            if (entityId == 0)
                throw new CustomException("Entity", "CorruptedEntity");

            var entity = await _entityService.GetEntitiesByIdAsync(entityId);
            if (entity == null)
                throw new CustomException("Entity", "CorruptedEntity");

            var columns = await _propertyService.GetAllColumnValuesByEntityIdAsync(entityId, pageSize, pageNumber);

            //is valid data
            if ((((pageSize * pageNumber) - columns.TotalCount) > pageSize) && (pageSize * pageNumber) > columns.TotalCount)
                throw new CustomException("Property", "CorruptedProperty", columns);

            return new ResultViewModel<IEnumerable<Dictionary<string, object>>> { ListNumber = columns.ListNumber, ListSize = columns.ListSize, TotalCount = columns.TotalCount, Data = columns.Data };
        }

    }
}