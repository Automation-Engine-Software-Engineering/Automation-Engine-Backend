using Microsoft.AspNetCore.Mvc;
using Services;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ViewModels;

namespace AutomationEngine.Controllers// Replace with your actual namespace  
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntityController : ControllerBase
    {
        private readonly IEntityService _entityService;

        public EntityController(IEntityService entityService)
        {
            _entityService = entityService;
        }

        // POST: api/entity/create  
        [HttpPost("create")]
        public async Task<IActionResult> CreateEntity([FromBody] string entityName)
        {
            if (string.IsNullOrWhiteSpace(entityName) || !IsValidIdentifier(entityName))
            {
                return BadRequest("Invalid entity name.");
            }

            try
            {
                await _entityService.CreateEntityAsync(entityName);
                return CreatedAtAction(nameof(GetAllColumnsFromEntity), new { entityName }, entityName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/entity/edit  
        [HttpPost("edit")]
        public async Task<IActionResult> EditEntity([FromBody] (string oldEntityName, string newEntityName) request)
        {
            if (string.IsNullOrWhiteSpace(request.oldEntityName) || !IsValidIdentifier(request.oldEntityName) ||
                string.IsNullOrWhiteSpace(request.newEntityName) || !IsValidIdentifier(request.newEntityName))
            {
                return BadRequest("Invalid entity names.");
            }

            try
            {
                await _entityService.EditEntityAsync(request.oldEntityName, request.newEntityName);
                return NoContent(); // 204 No Content  
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Entity '{request.oldEntityName}' not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/entity/delete  
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteEntity([FromBody] string entityName)
        {
            if (string.IsNullOrWhiteSpace(entityName) || !IsValidIdentifier(entityName))
            {
                return BadRequest("Invalid entity name.");
            }

            try
            {
                await _entityService.DeleteEntityAsync(entityName);
                return NoContent(); // 204 No Content  
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Entity '{entityName}' not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/entity/all  
        [HttpGet("all")]
        public async Task<ActionResult<List<string>>> GetAllEntities()
        {
            try
            {
                var entities = await _entityService.GetAllEntitiesAsync();
                return Ok(entities); // 200 OK  
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/entity/{entityName}/columns/add  
        [HttpPost("{entityName}/columns/add")]
        public async Task<IActionResult> AddColumnToEntity(string entityName, [FromBody] List<Column> columns)
        {
            if (string.IsNullOrWhiteSpace(entityName) || !IsValidIdentifier(entityName) || columns == null || columns.Count == 0)
            {
                return BadRequest("Invalid entity name or column definition.");
            }

            foreach (var column in columns)
            {
                if (!IsValidIdentifier(column.Name))
                {
                    return BadRequest($"Invalid column name: {column.Name}");
                }
                // Optionally, you can validate the column type as well (e.g., check against a list of allowed types).  
            }

            try
            {
                await _entityService.AddColumnToTableAsync(entityName, columns);
                return NoContent(); // 204 No Content  
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Entity '{entityName}' not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/entity/{entityName}/columns/edit  
        [HttpPost("{entityName}/columns/edit")]
        public async Task<IActionResult> EditColumnInEntity(string entityName, [FromBody] (string oldColumnName, string newColumnName, string newColumnType) columnInfo)
        {
            if (string.IsNullOrWhiteSpace(entityName) || !IsValidIdentifier(entityName) ||
                string.IsNullOrWhiteSpace(columnInfo.oldColumnName) || !IsValidIdentifier(columnInfo.oldColumnName) ||
                string.IsNullOrWhiteSpace(columnInfo.newColumnName) || !IsValidIdentifier(columnInfo.newColumnName))
            {
                return BadRequest("Invalid names or types provided.");
            }

            try
            {
                await _entityService.EditColumnInTableAsync(entityName, columnInfo.oldColumnName, columnInfo.newColumnName, columnInfo.newColumnType);
                return NoContent(); // 204 No Content  
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Entity '{entityName}' or column '{columnInfo.oldColumnName}' not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/entity/{entityName}/columns/delete  
        [HttpPost("{entityName}/columns/delete")]
        public async Task<IActionResult> DeleteColumnFromEntity(string entityName, [FromBody] string columnName)
        {
            if (string.IsNullOrWhiteSpace(entityName) || !IsValidIdentifier(entityName) ||
                string.IsNullOrWhiteSpace(columnName) || !IsValidIdentifier(columnName))
            {
                return BadRequest("Invalid entity or column name.");
            }

            try
            {
                await _entityService.EditColumnInTableAsync(entityName, columnName, "", "");
                return NoContent(); // 204 No Content  
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Entity '{entityName}' or column '{columnName}' not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/entity/{entityName}/columns  
        [HttpGet("{entityName}/columns")]
        public async Task<ActionResult<List<string>>> GetAllColumnsFromEntity(string entityName)
        {
            if (string.IsNullOrWhiteSpace(entityName) || !IsValidIdentifier(entityName))
            {
                return BadRequest("Invalid entity name.");
            }

            try
            {
                var columns = await _entityService.GetAllColumnsAsync(entityName);
                return Ok(columns); // 200 OK  
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Entity '{entityName}' not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Helper method to validate entity names and column names  
        private bool IsValidIdentifier(string name)
        {
            // Regex to allow only letters, numbers, and underscores and ensure it doesn't start with a number  
            return Regex.IsMatch(name, @"^[A-Za-z_][A-Za-z0-9_]*$");
        }
    }
}