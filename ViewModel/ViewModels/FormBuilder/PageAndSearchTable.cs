using Microsoft.AspNetCore.Http;
using Entities.Models.TableBuilder;
using ViewModels.ViewModels.Entity;

namespace ViewModels.ViewModels.FormBuilder
{
    public class TablePageination
    {
        public string Id { get; set; }
        public int PageNumber { get; set; } = 1 ;
    }

    public class TableSearch
    {
        public string Id { get; set; }
        public string SearchValue { get; set; } = "" ;
        public string SearchElement { get; set; } = "" ;
    }

    public class TableInput
    {
       public IEnumerable<TablePageination>? TablePagination { get; set; }
        public IEnumerable<TableSearch>? TableSearches { get; set; }
    }
}
