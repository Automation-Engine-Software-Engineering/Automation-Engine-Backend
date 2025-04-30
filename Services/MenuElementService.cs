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
    public interface IMenuElementService
    {
        Task InsertMenuElement(MenuElement MenuElement);
        Task UpdateMenuElement(MenuElement MenuElement);
        Task DeleteMenuElement(int Id);
        Task<List<MenuElementDTO>> GetMenuElementByRoleId(int roleId);
        Task<MenuElement> GetMenuElementById(int id);
        Task<ListDto<MenuElement>> GetAllMenuElement(int pageSize, int pageNumber);
        Task<ValidationDto<string>> SaveChangesAsync();
    }

    public class MenuElementService : IMenuElementService
    {
        private readonly DataLayer.DbContext.Context _context;

        public MenuElementService(DataLayer.DbContext.Context context)
        {
            _context = context;
        }
        public async Task InsertMenuElement(MenuElement MenuElement)
        {
            await _context.MenuElements.AddAsync(MenuElement);
        }
        public async Task<MenuElement> GetMenuElementById(int id)
        {
            var roleUser = await _context.MenuElements.FirstOrDefaultAsync(x => x.Id == id);

            return roleUser;
        }
        public async Task UpdateMenuElement(MenuElement MenuElement)
        {
            var existingRoleUser = await _context.MenuElements.FirstOrDefaultAsync(x => x.Id == MenuElement.Id);

            existingRoleUser.Name = MenuElement.Name;
            existingRoleUser.MenuType = MenuElement.MenuType;
            existingRoleUser.RoleId = MenuElement.RoleId;
            existingRoleUser.ParentMenuElemntId = MenuElement.ParentMenuElemntId;
            existingRoleUser.WorkflowId = MenuElement.WorkflowId;
            _context.MenuElements.Update(existingRoleUser);
        }

        public async Task DeleteMenuElement(int Id)
        {
            var roleUser = await _context.MenuElements.FirstOrDefaultAsync(x => x.Id == Id);
            _context.MenuElements.Remove(roleUser);
        }

        public async Task<List<MenuElementDTO>> GetMenuElementByRoleId(int roleId)
        {
            var query = _context.MenuElements.Include(x => x.workflow).Where(x => x.RoleId == roleId);
            var items = await query.ToListAsync();

            var MenuElement = BuildTree(items);
            return MenuElement;
        }

        public List<MenuElementDTO> BuildTree(List<MenuElement> elements)
        {
            var elementMap = elements.ToDictionary(e => e.Id, e => new MenuElementDTO
            {
                Name = e.Name,
                MenuType = e.MenuType,
                workflow = e.workflow,
                childs = new List<MenuElementDTO>()
            });

            var rootElements = new List<MenuElementDTO>();

            foreach (var element in elements)
            {
                var dto = elementMap[element.Id];
                if (element.ParentMenuElemntId.HasValue && elementMap.ContainsKey(element.ParentMenuElemntId.Value))
                {
                    var parentDto = elementMap[element.ParentMenuElemntId.Value];
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

        public async Task<ListDto<MenuElement>> GetAllMenuElement(int pageSize, int pageNumber)
        {
            var query = _context.MenuElements;

            var count = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new ListDto<MenuElement>(items, count, pageSize, pageNumber);
        }
    }
}