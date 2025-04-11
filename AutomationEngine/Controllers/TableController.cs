using Microsoft.AspNetCore.Mvc;
using Services;
using Entities.Models.TableBuilder;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;
using ViewModels;
using Microsoft.IdentityModel.Tokens;
using ViewModels.ViewModels.Table;
using AutomationEngine.ControllerAttributes;

namespace AutomationEngine.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [CheckAccess]
    public class TableController : ControllerBase
    {
        //private readonly IPropertyService _propertyService;

        //public TableController(IPropertyService tableService)
        //{
        //    _propertyService = tableService;
        //}

        // [HttpGet(nameof(GetTable))]
        // public async Task<ResultViewModel> GetTable([FromQuery]TableInputDto tableInput)
        // {
        //     var properties = await _propertyService.GetColumnValuesAsyncById(tableInput.Id);
            
        //     var result = new TableDto()
        //     {
               
        //     };

        //     await _propertyService.SaveChangesAsync();
        //     return (new ResultViewModel { Data = result, Message = "عملیات با موفقیت انجام شد.", Status = true, StatusCode = 200 });
        // }
    }
}