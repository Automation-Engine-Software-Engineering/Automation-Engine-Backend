using Xunit;
using Services;
using Moq;
using Microsoft.EntityFrameworkCore;
using Entities.Models.MainEngine;
using DataLayer.DbContext;
using System.Collections.Generic;
using ViewModels;

namespace AutomationEngine.Tests
{
    /// <summary>
    /// Test suite for menu element management functionality including CRUD operations
    /// </summary>
    public class MenuElementTests
    {
        private readonly Mock<Context> _contextMock;
        private readonly IMenuElementService _menuElementService;

        public MenuElementTests()
        {
            _contextMock = new Mock<Context>();
            _menuElementService = new MenuElementService(_contextMock.Object);
        }

        /// <summary>
        /// Tests the creation of a new menu element.
        /// Verifies that a menu element is correctly added to the database.
        /// </summary>
        /// <remarks>
        /// Test steps:
        /// 1. Creates a new menu element with test data
        /// 2. Mocks the database context for insertion
        /// 3. Verifies the element was added with correct properties
        /// </remarks>
        [Fact]
        public async Task InsertMenuElement_ValidElement_Success()
        {
            // Arrange
            var menuElement = new MenuElement
            {
                Id = 1,
                Name = "Test Menu",
                MenuType = 1,
                RoleId = 1
            };

            var menuElements = new List<MenuElement>();
            var mockSet = new Mock<DbSet<MenuElement>>();
            
            _contextMock.Setup(m => m.MenuElements).Returns(mockSet.Object);
            mockSet.Setup(m => m.AddAsync(It.IsAny<MenuElement>(), default))
                .Callback<MenuElement, CancellationToken>((element, token) => menuElements.Add(element));

            // Act
            await _menuElementService.InsertMenuElement(menuElement);

            // Assert
            Assert.Single(menuElements);
            Assert.Equal("Test Menu", menuElements[0].Name);
        }

        /// <summary>
        /// Tests retrieving menu elements for a specific role.
        /// Verifies that only menu elements associated with the given role are returned.
        /// </summary>
        /// <remarks>
        /// Test steps:
        /// 1. Sets up test data with multiple menu elements
        /// 2. Configures mock database context with query expectations
        /// 3. Verifies filtered results match the role ID
        /// </remarks>
        [Fact]
        public async Task GetMenuElementByRoleId_ValidRole_ReturnsElements()
        {
            // Arrange
            var roleId = 1;
            var menuElements = new List<MenuElement>
            {
                new MenuElement { Id = 1, Name = "Menu 1", RoleId = roleId },
                new MenuElement { Id = 2, Name = "Menu 2", RoleId = roleId },
                new MenuElement { Id = 3, Name = "Menu 3", RoleId = 2 }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<MenuElement>>();
            mockSet.As<IQueryable<MenuElement>>().Setup(m => m.Provider).Returns(menuElements.Provider);
            mockSet.As<IQueryable<MenuElement>>().Setup(m => m.Expression).Returns(menuElements.Expression);
            mockSet.As<IQueryable<MenuElement>>().Setup(m => m.ElementType).Returns(menuElements.ElementType);
            mockSet.As<IQueryable<MenuElement>>().Setup(m => m.GetEnumerator()).Returns(menuElements.GetEnumerator());

            _contextMock.Setup(c => c.MenuElements).Returns(mockSet.Object);

            // Act
            var result = await _menuElementService.GetMenuElementByRoleId(roleId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, item => Assert.Equal(roleId, item.RoleId));
        }

        /// <summary>
        /// Tests updating an existing menu element.
        /// Verifies that menu element properties are correctly updated in the database.
        /// </summary>
        /// <remarks>
        /// Test steps:
        /// 1. Creates test data with original and updated values
        /// 2. Mocks database context for update operation
        /// 3. Verifies the element was updated with new values
        /// </remarks>
        [Fact]
        public async Task UpdateMenuElement_ValidElement_Success()
        {
            // Arrange
            var menuElement = new MenuElement
            {
                Id = 1,
                Name = "Updated Menu",
                MenuType = 1,
                RoleId = 1
            };

            var existingElement = new MenuElement
            {
                Id = 1,
                Name = "Original Menu",
                MenuType = 1,
                RoleId = 1
            };

            var mockSet = new Mock<DbSet<MenuElement>>();
            _contextMock.Setup(m => m.MenuElements.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<MenuElement, bool>>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingElement);

            // Act
            await _menuElementService.UpdateMenuElement(menuElement);

            // Assert
            Assert.Equal("Updated Menu", existingElement.Name);
        }
    }
}