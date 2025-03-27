using DataLayer.Models.FormBuilder;
using DataLayer.Models.TableBuilder;
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
    [Authorize(Roles = "")]
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
        public async Task<ResultViewModel> CreateEntity(int? formId, [FromBody] EntityDto entity)
        {
            if (entity == null)
                throw new CustomException<Entity>(new ValidationDto<Entity>(false, "Entity", "CorruptedEntity", null), 500);

            //transfer model
            var result = new Entity(entity.PreviewName, entity.TableName, entity.Description, new List<EntityProperty>(), new List<Form>());

            //is validation model
            if (entity.Id != 0)
                throw new CustomException<Entity>(new ValidationDto<Entity>(false, "Entity", "CorruptedEntity", null), 500);

            var validationModel = await _entityService.EntityValidation(result);
            if (!validationModel.IsSuccess)
                throw new CustomException<Entity>(validationModel, 500);

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
                    throw new CustomException<Entity>(new ValidationDto<Entity>(false, "Form", "CorruptedNotfound", result), 500);

                await _entityService.CreateEntityAsync(formId.Value, result);
            }

            var saveResult = await _entityService.SaveChangesAsync();
            if (!saveResult.IsSuccess)
                throw new CustomException<string>(saveResult, 500);

            return (new ResultViewModel { Data = result, Message = new ValidationDto<Entity>(true, "Success", "Success", result).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // POST: api/entity/update  
        [HttpPost("update")]
        public async Task<ResultViewModel> UpdateEntity([FromBody] EntityDto entity)
        {
            if (entity == null)
                throw new CustomException<Entity>(new ValidationDto<Entity>(false, "Entity", "CorruptedEntity", null), 500);

            var result = new Entity(entity.PreviewName, entity.TableName, entity.Description, new List<EntityProperty>(), new List<Form>());

            //is validation model
            if (entity.Id == 0)
                throw new CustomException<Entity>(new ValidationDto<Entity>(false, "Entity", "CorruptedEntity", null), 500);

            result.Id = entity.Id;
            var validationModel = await _entityService.EntityValidation(result);
            if (!validationModel.IsSuccess)
                throw new CustomException<Entity>(validationModel, 500);

            var fetchEntity = await _entityService.GetEntitiesByIdAsync(entity.Id);
            if (fetchEntity == null)
                throw new CustomException<Entity>(new ValidationDto<Entity>(false, "Entity", "CorruptedEntityNotFound", null), 500);

            result.TableName.IsValidStringCommand();

            //initial action
            await _entityService.UpdateEntityAsync(result);
            var saveResult = await _entityService.SaveChangesAsync();
            if (!saveResult.IsSuccess)
                throw new CustomException<string>(saveResult, 500);

            return (new ResultViewModel { Data = result, Message = new ValidationDto<Entity>(true, "Success", "Success", result).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // POST: api/entity/remove  
        [HttpPost("remove")]
        public async Task<ResultViewModel> RemoveEntity(int entityId)
        {
            //is validation model
            if (entityId == 0)
                throw new CustomException<Entity>(new ValidationDto<Entity>(false, "Entity", "CorruptedEntityNotFound", null), 500);

            var fetchEntity = await _entityService.GetEntitiesByIdAsync(entityId);
            if (fetchEntity == null)
                throw new CustomException<Entity>(new ValidationDto<Entity>(false, "Entity", "CorruptedEntityNotFound", null), 500);

            var validationModel = await _entityService.EntityValidation(fetchEntity);
            if (!validationModel.IsSuccess)
                throw new CustomException<Entity>(validationModel, 500);

            //initial action
            await _entityService.RemoveEntityAsync(entityId);
            var saveResult = await _entityService.SaveChangesAsync();
            if (!saveResult.IsSuccess)
                throw new CustomException<string>(saveResult, 500);

            return (new ResultViewModel { Data = fetchEntity, Message = new ValidationDto<Entity>(true, "Success", "Success", fetchEntity).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // GET: api/entity/all  
        [HttpGet("all")]
        [SwaggerOperation(Summary = "", Description = "set formId and search null if you don't want to filter result")]
        public async Task<ResultViewModel> GetAllEntities(int pageSize, int pageNumber, int? formId, string? search)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            var entities = await _entityService.GetAllEntitiesAsync(pageSize, pageNumber, search, formId);

            //is valid data
            if ((((pageSize * pageNumber) - entities.TotalCount) > pageSize) && (pageSize * pageNumber) > entities.TotalCount)
                throw new CustomException<ListDto<Entity>>(new ValidationDto<ListDto<Entity>>(false, "Form", "CorruptedEntity", entities), 500);
            return (new ResultViewModel { Data = entities.Data, ListNumber = entities.ListNumber, ListSize = entities.ListSize, TotalCount = entities.TotalCount, Message = new ValidationDto<ListDto<Entity>>(true, "Success", "Success", entities).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // GET: api/entity/{id}  
        [HttpGet("{entityId}")]
        public async Task<ResultViewModel> GetEntity(int entityId)
        {
            //is validation model
            if (entityId == 0)
                throw new CustomException<Entity>(new ValidationDto<Entity>(false, "Entity", "CorruptedEntityNotFound", null), 500);

            //initial action
            var entities = await _entityService.GetEntitiesByIdAsync(entityId);
            var fetchEntity = await _entityService.GetEntitiesByIdAsync(entityId);
            if (fetchEntity == null)
                throw new CustomException<Entity>(new ValidationDto<Entity>(false, "Entity", "CorruptedEntityNotFound", null), 500);
            var validationModel = await _entityService.EntityValidation(fetchEntity);
            if (!validationModel.IsSuccess)
                throw new CustomException<Entity>(validationModel, 500);

            return (new ResultViewModel { Data = entities, Message = new ValidationDto<Entity>(true, "Success", "Success", entities).GetMessage(200), Status = true, StatusCode = 200 });
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
        //        throw new CustomException<Entity>(new ValidationDto<Entity>(false, "Entity", "CorruptedEntityNotFound", null), 500);

        //    //var entities = await _entityService.GetEntitiesByIdAsync(entityId);

        //    //initial action
        //    var fetchEntity = await _entityService.GetEntitiesByIdAsync(entityId);
        //    if (fetchEntity == null)
        //        throw new CustomException<Entity>(new ValidationDto<Entity>(false, "Entity", "CorruptedEntityNotFound", null), 500);

        //    var validationModel = await _entityService.EntityValidation(fetchEntity);
        //    if (!validationModel.IsSuccess)
        //        throw new CustomException<Entity>(validationModel, 500);

        //    //initial action
        //    var result = new EntityValueDto();
        //    if (fetchEntity.Properties.Count == 0)
        //        throw new CustomException<Entity>(new ValidationDto<Entity>(false, "Property", "PropertyNotFound", null), 500);

        //    var header = fetchEntity.Properties.Select(x => x.PreviewName).ToList();
        //    //var dtoListHeader = new ListDto<string>(header, header.Count, header.Count, 1);
        //    result.Header = header;
        //    var properties = await _propertyService.GetColumnValuesByIdAsync(, pageSize, pageNumber);

        //    //is valid data
        //    if ((((pageSize * pageNumber) - properties.TotalCount) > pageSize) && (pageSize * pageNumber) > properties.TotalCount)
        //        throw new CustomException<ListDto<Dictionary<string, object>>>(new ValidationDto<ListDto<Dictionary<string, object>>>(false, "Entity", "CorruptedEntity", properties), 500);

        //    result.Body = properties.Data;
        //    return (new ResultViewModel { ListNumber = properties.ListNumber, ListSize = properties.ListSize, TotalCount = properties.TotalCount, Data = result, Message = new ValidationDto<EntityValueDto>(true, "Success", "Success", result).GetMessage(200), Status = true, StatusCode = 200 });
        //}


        // GET: api/entity/{entityName}/property  
        [HttpGet("{entityId}/property")]
        public async Task<ResultViewModel> GetAllperopertiesFromEntity(int entityId, int pageSize, int pageNumber)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            if (entityId == 0)
                throw new CustomException<EntityProperty>(new ValidationDto<EntityProperty>(false, "Property", "CorruptedProperty", null), 500);

            var entity = await _entityService.GetEntitiesByIdAsync(entityId);
            if (entity == null)
                throw new CustomException<Entity>(new ValidationDto<Entity>(false, "Entity", "CorruptedEntity", null), 500);

            var columns = await _propertyService.GetAllColumnByEntityIdAsync(entityId, pageSize, pageNumber);

            //is valid data
            if ((((pageSize * pageNumber) - columns.TotalCount) > pageSize) && (pageSize * pageNumber) > columns.TotalCount)
                throw new CustomException<ListDto<EntityProperty>>(new ValidationDto<ListDto<EntityProperty>>(false, "Property", "CorruptedProperty", columns), 500);

            return (new ResultViewModel { Data = columns.Data, ListNumber = columns.ListNumber, ListSize = columns.ListSize, TotalCount = columns.TotalCount, Message = new ValidationDto<ListDto<EntityProperty>>(true, "Success", "Success", columns).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // GET: api/property/entityId/value
        [HttpGet("{entityId}/property/value")]
        public async Task<ResultViewModel> GetAllperopertiesValueFromEntity(int entityId, int pageSize, int pageNumber)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            if (entityId == 0)
                throw new CustomException<EntityProperty>(new ValidationDto<EntityProperty>(false, "Entity", "CorruptedEntity", null), 500);

            var entity = await _entityService.GetEntitiesByIdAsync(entityId);
            if (entity == null)
                throw new CustomException<Entity>(new ValidationDto<Entity>(false, "Entity", "CorruptedEntity", null), 500);

            var columns = await _propertyService.GetAllColumnValuesByEntityIdAsync(entityId, pageSize, pageNumber);

            //is valid data
            if ((((pageSize * pageNumber) - columns.TotalCount) > pageSize) && (pageSize * pageNumber) > columns.TotalCount)
                throw new CustomException<ListDto<Dictionary<string, object>>>(new ValidationDto<ListDto<Dictionary<string, object>>>(false, "Property", "CorruptedProperty", columns), 500);

            return (new ResultViewModel { ListNumber = columns.ListNumber, ListSize = columns.ListSize, TotalCount = columns.TotalCount, Data = columns.Data, Message = new ValidationDto<ListDto<Dictionary<string, object>>>(true, "Success", "Success", columns).GetMessage(200), Status = true, StatusCode = 200 });
        }

    }
}