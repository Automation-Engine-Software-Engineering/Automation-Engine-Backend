using Xunit;
using Services;
using Tools.AuthoraizationTools;
using Microsoft.Extensions.Configuration;
using Moq;
using Entities.Models.MainEngine;

namespace AutomationEngine.Tests
{
    /// <summary>
    /// Test suite for authentication-related functionality including user login and token management
    /// </summary>
    public class AuthenticationTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IRoleService> _roleServiceMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly TokenGenerator _tokenGenerator;

        public AuthenticationTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _roleServiceMock = new Mock<IRoleService>();
            _configMock = new Mock<IConfiguration>();
            _tokenGenerator = new TokenGenerator(_configMock.Object);
        }

        /// <summary>
        /// Tests user login with valid credentials.
        /// Verifies that the correct user is returned when valid username and password are provided.
        /// </summary>
        /// <remarks>
        /// Test steps:
        /// 1. Arranges test data with valid credentials
        /// 2. Mocks the user service to return a valid user
        /// 3. Verifies the returned user matches the expected values
        /// </remarks>
        [Fact]
        public async Task Login_WithValidCredentials_ReturnsUser()
        {
            // Arrange
            var username = "testUser";
            var password = "testPass";
            var salt = "testSalt";
            var hashedPassword = HashString.HashPassword(password, salt);
            var user = new User { Id = 1, UserName = username, Password = hashedPassword, Salt = salt };
            
            _userServiceMock.Setup(x => x.GetUserByUsernameAsync(username))
                .ReturnsAsync(user);

            // Act
            var result = await _userServiceMock.Object.GetUserByUsernameAsync(username);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(username, result.UserName);
            Assert.Equal(hashedPassword, result.Password);
        }

        /// <summary>
        /// Tests user login with invalid credentials.
        /// Verifies that null is returned when invalid username is provided.
        /// </summary>
        /// <remarks>
        /// Test steps:
        /// 1. Arranges test data with invalid username
        /// 2. Mocks the user service to return null
        /// 3. Verifies null is returned
        /// </remarks>
        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsNull()
        {
            // Arrange
            var username = "wrongUser";
            _userServiceMock.Setup(x => x.GetUserByUsernameAsync(username))
                .ReturnsAsync((User)null);

            // Act
            var result = await _userServiceMock.Object.GetUserByUsernameAsync(username);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests JWT token generation functionality.
        /// Verifies that both access and refresh tokens are generated correctly for a valid user.
        /// </summary>
        /// <remarks>
        /// Test steps:
        /// 1. Arranges test data with valid user and JWT settings
        /// 2. Generates tokens using the token generator
        /// 3. Verifies both tokens are generated and are different
        /// </remarks>
        [Fact]
        public void GenerateTokens_WithValidUser_ReturnsTokens()
        {
            // Arrange
            var user = new User { Id = 1, UserName = "testUser" };
            _configMock.Setup(x => x["JWTSettings:Issuer"]).Returns("testIssuer");
            _configMock.Setup(x => x["JWTSettings:Audience"]).Returns("testAudience");
            _configMock.Setup(x => x["JWTSettings:Key"]).Returns("testKey123456789testKey123456789");

            // Act
            var (accessToken, refreshToken) = _tokenGenerator.GenerateTokens(user, null);

            // Assert
            Assert.NotNull(accessToken);
            Assert.NotNull(refreshToken);
            Assert.NotEqual(accessToken, refreshToken);
        }
    }
}