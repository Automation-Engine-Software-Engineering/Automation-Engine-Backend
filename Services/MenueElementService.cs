using DataLayer.DbContext;
using Entities.Models.MainEngine;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;
using ViewModels.ViewModels.RoleDtos;

namespace Services
{
    public interface IMenueElementService
    {
        Task InsertMenueElement(MenueElement menueElement);
        Task UpdateMenueElement(MenueElement menueElement);
        Task DeleteMenueElement(int Id);
        Task<List<MenueElementDTO>> GetMenueElementByRoleId(int roleId);
        Task<MenueElement> GetMenueElementById(int id);
        Task<ListDto<MenueElement>> GetAllMenueElement(int pageSize, int pageNumber);
        Task<ValidationDto<string>> SaveChangesAsync();
    }

    public class MenueElementService : IMenueElementService
    {
        private readonly DataLayer.DbContext.Context _context;

        public MenueElementService(DataLayer.DbContext.Context context)
        {
            _context = context;
        }
        public async Task InsertMenueElement(MenueElement menueElement)
        {
            await _context.MenueElements.AddAsync(menueElement);
        }
        public async Task<MenueElement> GetMenueElementById(int id)
        {
            var roleUser = await _context.MenueElements.FirstOrDefaultAsync(x => x.Id == id);

            return roleUser;
        }
        public async Task UpdateMenueElement(MenueElement menueElement)
        {
            var existingRoleUser = await _context.MenueElements.FirstOrDefaultAsync(x => x.Id == menueElement.Id);

            existingRoleUser.Name = menueElement.Name;
            existingRoleUser.MenueType = menueElement.MenueType;
            existingRoleUser.RoleId = menueElement.RoleId;
            existingRoleUser.ParentMenueElemntId = menueElement.ParentMenueElemntId;
            existingRoleUser.WorkflowId = menueElement.WorkflowId;
            _context.MenueElements.Update(existingRoleUser);
        }

        public async Task DeleteMenueElement(int Id)
        {
            var roleUser = await _context.MenueElements.FirstOrDefaultAsync(x => x.Id == Id);
            _context.MenueElements.Remove(roleUser);
        }

        public async Task<List<MenueElementDTO>> GetMenueElementByRoleId(int roleId)
        {
            var query = _context.MenueElements.Include(x => x.workflow).Where(x => x.RoleId == roleId);
            var items = await query.ToListAsync();

            var menueElement = BuildTree(items);
            return menueElement;
        }

        public List<MenueElementDTO> BuildTree(List<MenueElement> elements)
        {
            var elementMap = elements.ToDictionary(e => e.Id, e => new MenueElementDTO
            {
                Name = e.Name,
                MenueType = e.MenueType,
                workflow = e.workflow,
                childs = new List<MenueElementDTO>()
            });

            var rootElements = new List<MenueElementDTO>();

            foreach (var element in elements)
            {
                var dto = elementMap[element.Id];
                if (element.ParentMenueElemntId.HasValue && elementMap.ContainsKey(element.ParentMenueElemntId.Value))
                {
                    var parentDto = elementMap[element.ParentMenueElemntId.Value];
                    parentDto.childs.Add(dto);
                }
                else
                {
                    rootElements.Add(dto);
                }
            }
            return rootElements;
        }

        public async Task<ValidationDto<string>> SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                return new ValidationDto<string>(true, "Success", "ChangesSaved", null);
            }
            catch (Exception ex)
            {
                return new ValidationDto<string>(false, "Error", "SaveFailed", ex.Message);
            }
        }

        public async Task<ListDto<MenueElement>> GetAllMenueElement(int pageSize, int pageNumber)
        {
            var query = _context.MenueElements;

            var count = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new ListDto<MenueElement>(items, count, pageSize, pageNumber);
        }
    }
}