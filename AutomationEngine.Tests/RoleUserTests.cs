using Xunit;
using Services;
using Moq;
using Microsoft.EntityFrameworkCore;
using Entities.Models.MainEngine;
using DataLayer.DbContext;
using System.Collections.Generic;

namespace AutomationEngine.Tests
{
    /// <summary>
    /// Test suite for role and user management functionality including role assignments and validations
    /// </summary>
    public class RoleUserTests
    {
        private readonly Mock<Context> _contextMock;
        private readonly IRoleService _roleService;
        private readonly IRoleUserService _roleUserService;

        public RoleUserTests()
        {
            _contextMock = new Mock<Context>();
            _roleService = new RoleService(_contextMock.Object);
            _roleUserService = new RoleUserService(_contextMock.Object);
        }

        /// <summary>
        /// Tests the creation of a new role.
        /// Verifies that a role can be created with specified properties.
        /// </summary>
        /// <remarks>
        /// Test steps:
        /// 1. Creates a new role with test data
        /// 2. Mocks the database context for role creation
        /// 3. Verifies the role was created with correct properties
        /// </remarks>
        [Fact]
        public async Task CreateRole_ValidRole_Success()
        {
            // Arrange
            var role = new Role
            {
                Id = 1,
                Name = "Test Role",
                Description = "Test Role Description",
                RoleType = RoleType.Admin
            };

            var roles = new List<Role>();
            var mockSet = new Mock<DbSet<Role>>();
            
            _contextMock.Setup(m => m.Role).Returns(mockSet.Object);
            mockSet.Setup(m => m.AddAsync(It.IsAny<Role>(), default))
                .Callback<Role, CancellationToken>((r, token) => roles.Add(r));

            // Act
            await _roleService.CreateRoleAsync(role);

            // Assert
            Assert.Single(roles);
            Assert.Equal("Test Role", roles[0].Name);
        }

        /// <summary>
        /// Tests assigning a role to a user.
        /// Verifies that a role-user relationship is correctly established.
        /// </summary>
        /// <remarks>
        /// Test steps:
        /// 1. Creates a new role-user association
        /// 2. Mocks the database context for relationship creation
        /// 3. Verifies the association was created with correct IDs
        /// </remarks>
        [Fact]
        public async Task AssignRoleToUser_ValidAssignment_Success()
        {
            // Arrange
            var roleUser = new RoleUser
            {
                Id = 1,
                UserId = 1,
                RoleId = 1
            };

            var roleUsers = new List<RoleUser>();
            var mockSet = new Mock<DbSet<RoleUser>>();
            
            _contextMock.Setup(m => m.RoleUser).Returns(mockSet.Object);
            mockSet.Setup(m => m.AddAsync(It.IsAny<RoleUser>(), default))
                .Callback<RoleUser, CancellationToken>((ru, token) => roleUsers.Add(ru));

            // Act
            await _roleUserService.CreateRoleUserAsync(roleUser);

            // Assert
            Assert.Single(roleUsers);
            Assert.Equal(1, roleUsers[0].UserId);
            Assert.Equal(1, roleUsers[0].RoleId);
        }

        /// <summary>
        /// Tests retrieving all roles assigned to a specific user.
        /// Verifies that all roles associated with a user are correctly returned.
        /// </summary>
        /// <remarks>
        /// Test steps:
        /// 1. Sets up test data with multiple role assignments
        /// 2. Mocks database query for role retrieval
        /// 3. Verifies all assigned roles are returned
        /// </remarks>
        [Fact]
        public async Task GetUserRoles_ExistingUser_ReturnsRoles()
        {
            // Arrange
            var userId = 1;
            var roleUsers = new List<RoleUser>
            {
                new RoleUser { UserId = userId, RoleId = 1, Role = new Role { Name = "Role1" } },
                new RoleUser { UserId = userId, RoleId = 2, Role = new Role { Name = "Role2" } }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<RoleUser>>();
            mockSet.As<IQueryable<RoleUser>>().Setup(m => m.Provider).Returns(roleUsers.Provider);
            mockSet.As<IQueryable<RoleUser>>().Setup(m => m.Expression).Returns(roleUsers.Expression);
            mockSet.As<IQueryable<RoleUser>>().Setup(m => m.ElementType).Returns(roleUsers.ElementType);
            mockSet.As<IQueryable<RoleUser>>().Setup(m => m.GetEnumerator()).Returns(roleUsers.GetEnumerator());

            _contextMock.Setup(c => c.RoleUser).Returns(mockSet.Object);

            // Act
            var result = await _roleUserService.GetRoleUserByUserIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        /// <summary>
        /// Tests role validation functionality.
        /// Verifies that a role with valid properties passes validation.
        /// </summary>
        /// <remarks>
        /// Test steps:
        /// 1. Creates a role with valid properties
        /// 2. Performs validation check
        /// 3. Verifies validation passes successfully
        /// </remarks>
        [Fact]
        public void ValidateRole_ValidRole_ReturnsTrue()
        {
            // Arrange
            var role = new Role
            {
                Id = 1,
                Name = "Valid Role",
                Description = "Valid Description",
                RoleType = RoleType.User
            };

            // Act
            var result = _roleService.ValidateRole(role);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Success", result.MessageName);
        }

        /// <summary>
        /// Tests removing a role assignment from a user.
        /// Verifies that a role-user relationship can be correctly removed.
        /// </summary>
        /// <remarks>
        /// Test steps:
        /// 1. Sets up existing role-user relationship
        /// 2. Executes removal operation
        /// 3. Verifies the relationship was removed
        /// </remarks>
        [Fact]
        public async Task RemoveUserFromRole_ExistingAssignment_Success()
        {
            // Arrange
            var roleUser = new RoleUser
            {
                Id = 1,
                UserId = 1,
                RoleId = 1
            };

            var mockSet = new Mock<DbSet<RoleUser>>();
            _contextMock.Setup(m => m.RoleUser
                .FirstOrDefaultAsync(It.IsAny<Expression<Func<RoleUser, bool>>>(), default))
                .ReturnsAsync(roleUser);

            // Act
            await _roleUserService.DeleteRoleUserAsync(roleUser);

            // Assert
            mockSet.Verify(m => m.Remove(It.IsAny<RoleUser>()), Times.Once());
        }
    }
}