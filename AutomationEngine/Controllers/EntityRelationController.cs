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
                throw new CustomException<EntityRelationInputDto>(new ValidationDto<EntityRelationInputDto>(false, "EntityRelation", "CorruptedEntityRelation", entityRelation), 500);

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
        [HttpGet("all/{parentId}")]
        public async Task<ResultViewModel> GetAllEntityRelation(int parentId, int pageSize, int pageNumber)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            var forms = await _entityRelationService.GetEntityRelationById(parentId, pageSize, pageNumber);

            //is valid data
            if ((((pageSize * pageNumber) - forms.TotalCount) > pageSize) && (pageSize * pageNumber) > forms.TotalCount)
                throw new CustomException<ListDto<IsAccessModel>>(new ValidationDto<ListDto<IsAccessModel>>(false, "Form", "CorruptedInvalidPage", forms), 500);

            return (new ResultViewModel { Data = forms.Data, ListNumber = forms.ListNumber, ListSize = forms.ListSize, TotalCount = forms.TotalCount, Message = new ValidationDto<ListDto<IsAccessModel>>(true, "Success", "Success", forms).GetMessage(200), Status = true, StatusCode = 200 });
        }

    }
}
