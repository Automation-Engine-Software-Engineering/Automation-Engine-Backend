using Entities.Models.FormBuilder;
using Microsoft.AspNetCore.Mvc;
using ViewModels.ViewModels.FormBuilder;
using ViewModels;
using Entities.Models.Workflows;
using Services;
using ViewModels.ViewModels.Workflow;
using AutomationEngine.ControllerAttributes;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using Microsoft.IdentityModel.Tokens;
using Tools.TextTools;
using Entities.Models.MainEngine;
using Tools.AuthoraizationTools;
using Entities.Models.Enums;
using ViewModels.ViewModels.EntityDtos;
using Entities.Models.TableBuilder;

namespace AutomationEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [CheckAccess]
    public class EntityRelationController : Controller
    {
        private readonly IEntityRelationService _entityRelationService;
        private readonly TokenGenerator _tokenGenerator;
        private readonly IEntityService _entityService;
        public EntityRelationController(IEntityRelationService entityRelationService, TokenGenerator tokenGenerator, IEntityService entityService)
        {
            _entityRelationService = entityRelationService;
            _tokenGenerator = tokenGenerator;
            _entityService = entityService;
        }

        // POST: api/form/create  
        [HttpPost("create")]
        public async Task<ResultViewModel> CreateEntityRelation([FromBody] EntityRelationInputDto entityRelation)
        {
            if (entityRelation == null)
                throw new CustomException<Entity_EntityRelation>(new ValidationDto<Entity_EntityRelation>(false, "EntityRelation", "CorruptedEntityRelation", null), 500);

            var parentEntityExist = await _entityService.IsEntityExistAsync(entityRelation.Id);

            var childEntityExist = await _entityService.IsEntityExistAsync(entityRelation.Id);

            if(!parentEntityExist || !childEntityExist)
                throw new CustomException<Entity_EntityRelation>(new ValidationDto<Entity_EntityRelation>(false, "EntityRelation", "CorruptedEntityRelation", result), 500);

            var result = new Entity_EntityRelation
            {
                ChildId = entityRelation.ChildId,
                ParentId = entityRelation.ParentId
            };

          
            await _entityRelationService.InsertRangeEntityRelation([result]);
            var validationModel = await _entityRelationService.SaveChangesAsync();
            if (!validationModel.IsSuccess)
                throw new CustomException<string>(validationModel, 500);


            await _entityRelationService.SaveChangesAsync();
            return (new ResultViewModel { Data = result, Message = new ValidationDto<Entity_EntityRelation>(true, "Success", "Success", result).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // POST: api/form/create  
        [HttpPost("create/allByEntityId/{entityId}")]
        public async Task<ResultViewModel> CreateEntityAllByEntityId([FromBody] List<int> EntityIds, int entityId)
        {
            if (EntityIds == null)
                throw new CustomException<Entity_EntityRelation>(new ValidationDto<Entity_EntityRelation>(false, "EntityRelation", "CorruptedEntityRelation", null), 500);

            var users = new List<Entity_EntityRelation>();

            await _entityRelationService.ReplaceEntityRelationsByEntityId(entityId, EntityIds);
            await _entityRelationService.SaveChangesAsync();
            return (new ResultViewModel { Data = users, Message = new ValidationDto<List<Entity_EntityRelation>>(true, "Success", "Success", users).GetMessage(200), Status = true, StatusCode = 200 });
        }

       
        // GET: api/form/all  
        [HttpGet("all")]
        public async Task<ResultViewModel> GetAllEntityRelation(int pageSize, int pageNumber)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            var forms = await _entityRelationService.ReplaceEntityRelationsByEntityId(pageSize, pageNumber);

            //is valid data
            if ((((pageSize * pageNumber) - forms.TotalCount) > pageSize) && (pageSize * pageNumber) > forms.TotalCount)
                throw new CustomException<ListDto<Entity_EntityRelation>>(new ValidationDto<ListDto<Entity_EntityRelation>>(false, "Form", "CorruptedInvalidPage", forms), 500);

            return (new ResultViewModel { Data = forms.Data, ListNumber = forms.ListNumber, ListSize = forms.ListSize, TotalCount = forms.TotalCount, Message = new ValidationDto<ListDto<Role_Entity>>(true, "Success", "Success", forms).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // GET: api/form/{id}  
        [HttpGet("{entityRelationId}")]
        public async Task<ResultViewModel> GetEntityRelation(int entityRelationId)
        {                        //is validation model
            if (entityRelationId == 0)
                throw new CustomException<int>(new ValidationDto<int>(false, "EntityWorkflow", "CorruptedEntityWorkflow", entityRelationId), 500);

            //initial action
            var EntityRelation = await _entityRelationService.(entityRelationId);
            if (EntityRelation == null)
                throw new CustomException<Role_Entity>(new ValidationDto<Role_Entity>(false, "EntityWorkflow", "CorruptedEntityWorkflow", EntityRelation), 500);

            var validationModel = await _entityRelationService.EntityRelationValidation(EntityRelation);
            if (!validationModel.IsSuccess)
                throw new CustomException<Role_Entity>(validationModel, 500);

            var form = await _entityRelationService.GetEntityRelationById(entityRelationId);
            return (new ResultViewModel { Data = form, Message = new ValidationDto<Role_Entity>(true, "Success", "Success", form).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // GET: api/form/{id}  
        [HttpGet("entityRelationById")]
        public async Task<ResultViewModel> GetEntityRelationBuEntityId(int pageSize, int pageNumber)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            var claims = await HttpContext.Authorize();

            if (claims.EntityId == 0)
                throw new CustomException<int>(new ValidationDto<int>(false, "EntityWorkflow", "CorruptedEntityWorkflow", claims.EntityId), 500);

            //initial action
            var EntityRelation = await _entityRelationService.GetEntityRelationByEntityId(claims.EntityId, pageSize, pageNumber);
            if (EntityRelation == null)
                throw new CustomException<ListDto<Role_Entity>>(new ValidationDto<ListDto<Role_Entity>>(false, "EntityWorkflow", "CorruptedEntityWorkflow", EntityRelation), 500);

            //is valid data
            if ((((pageSize * pageNumber) - EntityRelation.TotalCount) > pageSize) && (pageSize * pageNumber) > EntityRelation.TotalCount)
                throw new CustomException<ListDto<Role_Entity>>(new ValidationDto<ListDto<Role_Entity>>(false, "Form", "CorruptedInvalidPage", EntityRelation), 500);

            return (new ResultViewModel { Data = EntityRelation.Data, ListNumber = EntityRelation.ListNumber, ListSize = EntityRelation.ListSize, TotalCount = EntityRelation.TotalCount, Message = new ValidationDto<ListDto<Role_Entity>>(true, "Success", "Success", EntityRelation).GetMessage(200), Status = true, StatusCode = 200 });
        }

        // GET: api/form/all  
        [HttpGet("user")]
        public async Task<ResultViewModel> GetAllEntityRelationAndEntity(int entityId, int pageSize, int pageNumber)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            ListDto<IsAccessModel> forms = await _entityService.GetAllEntityForRoleAccess(entityId, pageSize, pageNumber);

            //is valid data
            if ((((pageSize * pageNumber) - forms.TotalCount) > pageSize) && (pageSize * pageNumber) > forms.TotalCount)
                throw new CustomException<ListDto<IsAccessModel>>(new ValidationDto<ListDto<IsAccessModel>>(false, "Form", "CorruptedInvalidPage", forms), 500);

            return (new ResultViewModel { Data = forms.Data, ListNumber = forms.ListNumber, ListSize = forms.ListSize, TotalCount = forms.TotalCount, Message = new ValidationDto<ListDto<IsAccessModel>>(true, "Success", "Success", forms).GetMessage(200), Status = true, StatusCode = 200 });
        }
    }
}
