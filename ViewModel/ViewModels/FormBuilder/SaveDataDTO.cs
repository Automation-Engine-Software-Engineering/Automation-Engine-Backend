using Microsoft.AspNetCore.Http;
using Entities.Models.TableBuilder;
using ViewModels.ViewModels.Entity;

namespace ViewModels.ViewModels.FormBuilder
{
    public class SaveDataDTO
    {
        public int id { get; set; } = 0;
        public string content { get; set; }
    }
}
