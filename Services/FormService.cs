using DataLayer.Context;
using DataLayer.Models.FormBuilder;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Services
{
    public interface IFormService
    {
        Task CreateFormAsync(Form form);
        Task EditFormAsync(Form form);
        Task DeleteFormAsync(Form form);
        Task<Form> GetFormAsync(int formId);
        Task<List<Form>> GetAllFormsAsync();
        Task UpdateFormBodyAsync(int formId, string htmlContent);
        Task InsertFieldValueAsync(string tableName, string fieldName, string value);
        Task SaveChangesAsync();
    }

    public class FormService : IFormService
    {
        private readonly Context _context;
        private readonly DynamicDbContext _dynamicDbContext;

        public FormService(Context context, DynamicDbContext dynamicDbContext)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dynamicDbContext = dynamicDbContext;
        }

        public async Task CreateFormAsync(Form form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));
            await _context.Form.AddAsync(form);
        }

        public async Task EditFormAsync(Form form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));
            _context.Form.Update(form);
            await SaveChangesAsync(); // Save after edit  
        }

        public async Task DeleteFormAsync(Form form)
        {
            if (form == null) throw new ArgumentNullException(nameof(form));
            _context.Form.Remove(form);
            await SaveChangesAsync(); // Save after delete  
        }

        public async Task<Form> GetFormAsync(int formId)
        {
            return await _context.Form.FirstOrDefaultAsync(x => x.Id == formId)
                   ?? throw new KeyNotFoundException($"Form with ID {formId} not found.");
        }

        public async Task<List<Form>> GetAllFormsAsync()
        {
            return await _context.Form.ToListAsync();
        }

        public async Task UpdateFormBodyAsync(int formId, string htmlContent)
        {
            // Retrieve the form from the database  
            var form = await _context.Form.FindAsync(formId);
            if (form == null)
            {
                throw new KeyNotFoundException($"Form with ID '{formId}' not found.");
            }

            // Update the HtmlFormBody field  
            form.HtmlFormBody = htmlContent;

            // Save changes to the database  
            await SaveChangesAsync();
        }

        public async Task InsertFieldValueAsync(string tableName, string fieldName, string value)
        {
            // Validate if the table exists before proceeding  
            var tableExists = await TableExistsAsync(tableName);
            if (!tableExists)
            {
                throw new Exception($"Table '{tableName}' does not exist.");
            }

            // Perform dynamic insert operation  
            var query = $"INSERT INTO {tableName} ({fieldName}) VALUES ({value})";

            await _dynamicDbContext.ExecuteSqlRawAsync(query);
        }

        // Method that checks if a table exists in the database  
        private async Task<bool> TableExistsAsync(string tableName)
        {
            var query = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName";

            // Use the DbConnection and DbCommand from the DbContext  
            using (var connection = _dynamicDbContext.Database.GetDbConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = query;
                    command.Parameters.Add(new SqlParameter("@TableName", tableName));

                    // Execute the query and read the result  
                    var result = await command.ExecuteScalarAsync();
                    var count = Convert.ToInt32(result); // Convert the result to an integer  

                    return count > 0; // Return true if the table exists  
                }
            }
        }
        // Optional: Method to retrieve all table names  
        public async Task<List<string>> GetAllTableNamesAsync()
        {
            var tableNames = new List<string>();

            var query = @"  
            SELECT TABLE_NAME  
            FROM INFORMATION_SCHEMA.TABLES  
            WHERE TABLE_TYPE = 'BASE TABLE'";

            await using (var command = _dynamicDbContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                await _dynamicDbContext.Database.OpenConnectionAsync();
                await using (var result = await command.ExecuteReaderAsync())
                {
                    while (await result.ReadAsync())
                    {
                        tableNames.Add(result.GetString(0));
                    }
                }
            }

            return tableNames;
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}