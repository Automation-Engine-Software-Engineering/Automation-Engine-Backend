using DataLayer.Context;
using DataLayer.Models.FormBuilder;
using DataLayer.Models.TableBuilder;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using Microsoft.EntityFrameworkCore;
using Tools.TextTools;

namespace Services
{
    public interface IFormService
    {
        Task CreateFormAsync(Form form);
        Task UpdateFormAsync(Form form);
        Task RemoveFormAsync(int formId);
        Task<Form?> GetFormByIdAsync(int formId);
        Task<Form?> GetFormByIdIncEntityAsync(int formId);
        Task<Form?> GetFormByIdIncEntityIncPropertyAsync(int formId);
        Task<ListDto<Form>> GetAllFormsAsync(int pageSize, int pageNumber);
        Task UpdateFormBodyAsync(int formId, string htmlContent);
        Task SetTheParameter(List<(int entityId, int propertyId, object value)> data);
        Task<ValidationDto<Form>> FormValidationAsync(Form form);
        Task<ValidationDto<string>> SaveChangesAsync();
        Task<bool> IsFormExistAsync(int formId);
        Task AddEntitiesToFormAsync(int formId, List<int> entityIds);
    }

    public class FormService : IFormService
    {
        private readonly Context _context;
        private readonly DynamicDbContext _dynamicDbContext;
        public FormService(Context context, DynamicDbContext dynamicDbContext)
        {
            _context = context;
            _dynamicDbContext = dynamicDbContext;
        }

        public async Task CreateFormAsync(Form form)
        {
            await _context.Form.AddAsync(form);
        }

        public async Task UpdateFormAsync(Form form)
        {
            //initialize model
            var fetchModel = await _context.Form.FirstAsync(x => x.Id == form.Id);

            //transfer model
            fetchModel.Name = form.Name;
            fetchModel.SizeMinHeight = form.SizeMinHeight;
            fetchModel.IsAutoHeight = form.IsAutoHeight;
            fetchModel.SizeWidth = form.SizeWidth;
            fetchModel.BackgroundImgPath = form.BackgroundImgPath;
            fetchModel.BackgroundColor = form.BackgroundColor;
            fetchModel.Description = form.Description;

            _context.Form.Update(fetchModel);
        }

        public async Task RemoveFormAsync(int formId)
        {
            //initialize model
            var fetchModel = await _context.Form.FirstAsync(x => x.Id == formId);

            //remove form
            _context.Form.Remove(fetchModel);
        }

        public async Task<Form?> GetFormByIdAsync(int formId)
        {
            //initialize model
            var fetchModel = await _context.Form.FindAsync(formId);

            //return model
            return fetchModel;
        }
        public async Task<Form?> GetFormByIdIncEntityAsync(int formId)
        {
            //initialize model
            var fetchModel = await _context.Form.Include(x => x.Entities).FirstOrDefaultAsync(x => x.Id == formId);

            //return model
            return fetchModel;
        }
        public async Task<Form?> GetFormByIdIncEntityIncPropertyAsync(int formId)
        {
            //initialize model
            var fetchModel = await _context.Form.Include(x => x.Entities).ThenInclude(x => x.Properties).FirstOrDefaultAsync(x => x.Id == formId);

            //return model
            return fetchModel;
        }
        public async Task AddEntitiesToFormAsync(int formId, List<int> entityIds)
        {
            // Find the Form
            var form = await _context.Form
                .Include(f => f.Entities) // Load related entities
                .FirstOrDefaultAsync(f => f.Id == formId);

            if (form == null)
                throw new CustomException<Form>(new ValidationDto<Form>(false, "Form", "FormNotFound", form), 404);

            // Find the Entities based on the provided list of IDs
            var entities = await _context.Entity
                .Where(e => entityIds.Contains(e.Id))
                .ToListAsync();

            // Add new Entities that are not already in the form's entity list
            var newEntities = entities
                .Where(e => !form.Entities.Any(existing => existing.Id == e.Id))
                .ToList();

            if (newEntities.Any())
                form.Entities.AddRange(newEntities); // Add the new entities

            // Remove Entities that are not in the provided entityIds
            form.Entities = form.Entities
                .Where(e => entityIds.Contains(e.Id))
                .ToList(); // Retain only entities matching the provided IDs

        }

        public async Task<bool> IsFormExistAsync(int formId)
        {
            //check model exist
            var isExist = await _context.Form.AnyAsync(x => x.Id == formId);

            //return model
            return isExist;
        }
        public async Task<ListDto<Form>> GetAllFormsAsync(int pageSize, int pageNumber)
        {
            //create query
            var query = _context.Form;

            //get Value and count
            var count = query.Count();
            var result = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new ListDto<Form>(result, count, pageSize, pageNumber);
        }

        public async Task UpdateFormBodyAsync(int formId, string htmlContent)
        {
            //initialize model
            var fetchModel = await _context.Form.FirstAsync(x => x.Id == formId);

            //transfer model
            fetchModel.HtmlFormBody = htmlContent;

            await UpdateFormAsync(fetchModel);
        }

        public async Task SetTheParameter(List<(int entityId, int propertyId, object value)> data)
        {
            var entityIds = new List<int>();
            data.Select(x => x.entityId).ToList().ForEach(x =>
            {
                if (entityIds.Any(xx => xx == x))
                {
                    entityIds.Add(x);
                }
            });

            entityIds.ForEach(x =>
            {
                var entity = _context.Entity.FirstOrDefaultAsync(xx => xx.Id == x);

                var properties = data.Where(x => x.entityId == entity.Id).ToList();
                var propertiesValues = properties.Select(x => _context.Property.FirstOrDefaultAsync(xx => xx.Id == x.propertyId)).ToList();
                var propertiesQuery = "";
                propertiesValues.ForEach(x =>
                {
                    propertiesQuery += x.Result.PropertyName + " ,";
                });
                propertiesQuery += ".";
                propertiesQuery.Replace(",.", "");

                var propertiesQueryValue = "";
                data.Where(x => x.entityId == x.entityId).ToList().ForEach(xx =>
                {
                    propertiesQueryValue += xx.value.ToString() + " ,";
                });
                propertiesQueryValue += ".";
                propertiesQueryValue.Replace(",.", "");

                var query = $"INSERT INTO {entity.Result.TableName} ({propertiesQuery})" +
                $" VALUES ({propertiesQueryValue});";

                _dynamicDbContext.ExecuteSqlRawAsync(query);
            });
        }

        public async Task<ValidationDto<Form>> FormValidationAsync(Form form)
        {
            if (form == null) return new ValidationDto<Form>(false, "Form", "CorruptedForm", form);
            if (form.Name == null || !form.Name.IsValidString()) return new ValidationDto<Form>(false, "Form", "CorruptedFormName", form);
            if (form.SizeWidth == 0) return new ValidationDto<Form>(false, "Form", "CorruptedFormSize", form);
            if (form.SizeMinHeight == 0 ^ form.IsAutoHeight) return new ValidationDto<Form>(false, "Form", "CorruptedFormSize", form);
            return new ValidationDto<Form>(true, "Success", "Success", form);
        }

        // public async Task<Form> GetFormpreview(int formId)
        // {
        //     //the sample input
        //     //<div id="main">
        //     // <p> بسمه تعالی </p>
        //     // <p> نام و نام خانوادگی </p>
        //     // <input type="text" id="10" />
        //     // <select id="11"> <select>
        //     //<table id="12" data-table="maghalat" data-filter="name = ali" data-clumns="name,id"><table>
        //     //</div>

        //     //the sample output
        //     //<div id="main">
        //     // <p> بسمه تعالی </p>
        //     // <p> نام و نام خانوادگی </p>
        //     //<i class="fas fa-clock"></i>
        //     // <input type="text" id="10" value="name" tooltype="name fild" required/>
        //     //<span>pleas file the input</span>
        //     //<span>invalid data</span>
        //     // <select id="11">
        //     //<option>ali-kazemi<option>
        //     // <select>
        //     //<table id="12" data-table="maghalat" data-filter="name = ali" data-clumns="name,id"><table>
        //     //</div>

        //     var form = await GetFormByIdAsync(formId);

        // }

        public async Task<ValidationDto<string>> SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                return new ValidationDto<string>(true, "Success", "Success", null);
            }
            catch (Exception ex)
            {
                return new ValidationDto<string>(false, "Form", "CorruptedForm", ex.Message);
            }
        }
    }
}
