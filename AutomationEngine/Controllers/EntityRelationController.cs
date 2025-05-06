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
        public async Task<ResultViewModel<Entity_EntityRelation>> CreateEntityRelation([FromBody] EntityRelationInputDto entityRelation)
        {
            if (entityRelation == null)
                throw new CustomException("EntityRelation", "CorruptedEntityRelation", null);

            var parentEntityExist = await _entityService.IsEntityExistAsync(entityRelation.Id);

            var childEntityExist = await _entityService.IsEntityExistAsync(entityRelation.Id);

            if (!parentEntityExist || !childEntityExist)
                throw new CustomException("EntityRelation", "CorruptedEntityRelation", entityRelation);

            var result = new Entity_EntityRelation
            {
                ChildId = entityRelation.ChildId,
                ParentId = entityRelation.ParentId
            };


            await _entityRelationService.InsertRangeEntityRelation([result]);
            await _entityRelationService.SaveChangesAsync();
            return new ResultViewModel<Entity_EntityRelation>(result);
        }

        // POST: api/form/create  
        [HttpPost("create/allByEntityId/{entityId}")]
        public async Task<ResultViewModel<object>> CreateEntityAllByEntityId([FromBody] List<int> EntityIds, int entityId)
        {
            if (EntityIds == null)
                throw new CustomException("EntityRelation", "CorruptedEntityRelation");

            await _entityRelationService.ReplaceEntityRelationsByEntityId(entityId, EntityIds);
            await _entityRelationService.SaveChangesAsync();
            return new ResultViewModel<object>();

        }


        // GET: api/form/all  
        [HttpGet("all/{parentId}")]
        public async Task<ResultViewModel<IEnumerable<IsAccessModel>>> GetAllEntityRelation(int parentId, int pageSize, int pageNumber)
        {
            if (pageSize > 100)
                pageSize = 100;
            if (pageNumber < 1)
                pageNumber = 1;

            var forms = await _entityRelationService.GetEntityRelationById(parentId, pageSize, pageNumber);

            //is valid data
            if ((((pageSize * pageNumber) - forms.TotalCount) > pageSize) && (pageSize * pageNumber) > forms.TotalCount)
                throw new CustomException("Form", "CorruptedInvalidPage");

            return new ResultViewModel<IEnumerable<IsAccessModel>> { Data = forms.Data, ListNumber = forms.ListNumber, ListSize = forms.ListSize, TotalCount = forms.TotalCount};
        }

    }
}
