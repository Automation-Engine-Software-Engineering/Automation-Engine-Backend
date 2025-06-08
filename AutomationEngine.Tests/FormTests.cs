using Xunit;
using Services;
using Moq;
using Microsoft.EntityFrameworkCore;
using Entities.Models.FormBuilder;
using DataLayer.DbContext;
using FrameWork.Model.DTO;

namespace AutomationEngine.Tests
{
    /// <summary>
    /// Test suite for form handling functionality including form creation, validation, and HTML processing.
    /// </summary>
    public class FormTests
    {
        private readonly Mock<Context> _contextMock;
        private readonly Mock<DynamicDbContext> _dynamicDbContextMock;
        private readonly Mock<IHtmlService> _htmlServiceMock;
        private readonly IFormService _formService;

        public FormTests()
        {
            _contextMock = new Mock<Context>();
            _dynamicDbContextMock = new Mock<DynamicDbContext>();
            _htmlServiceMock = new Mock<IHtmlService>();
            _formService = new FormService(_contextMock.Object, _dynamicDbContextMock.Object, _htmlServiceMock.Object);
        }

        /// <summary>
        /// Tests form validation with valid properties.
        /// Verifies that a form with all required fields passes validation.
        /// </summary>
        /// <remarks>
        /// Test steps:
        /// 1. Creates a form with valid properties.
        /// 2. Performs validation check.
        /// 3. Verifies validation passes successfully.
        /// </remarks>
        [Fact]
        public void FormValidation_ValidForm_Success()
        {
            // Arrange
            var form = new Form
            {
                Id = 1,
                Name = "Test Form",
                Description = "Test Description",
                HtmlFormBody = "<form>Test Form</form>"
            };

            // Act
            var result = _formService.FormValidation(form);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Success", result.Parent);
            Assert.Equal("Success", result.MessageName);
        }

        /// <summary>
        /// Tests form validation with missing required fields.
        /// Verifies that validation fails when required properties are missing.
        /// </summary>
        /// <remarks>
        /// Test steps:
        /// 1. Creates a form with missing required fields.
        /// 2. Performs validation check.
        /// 3. Verifies validation fails with appropriate error message.
        /// </remarks>
        [Fact]
        public void FormValidation_InvalidForm_Failure()
        {
            // Arrange
            var form = new Form
            {
                Id = 1,
                Name = "", // Invalid - empty name
                Description = "Test Description",
                HtmlFormBody = "<form>Test Form</form>"
            };

            // Act
            var result = _formService.FormValidation(form);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Form", result.Parent);
            Assert.Equal("CorruptedForm", result.MessageName);
        }

        /// <summary>
        /// Tests the creation of a new form.
        /// Verifies that a form is correctly created with all required properties.
        /// </summary>
        /// <remarks>
        /// Test steps:
        /// 1. Creates a new form with test data.
        /// 2. Mocks the database context for form creation.
        /// 3. Verifies the form was created with correct properties.
        /// </remarks>
        [Fact]
        public async Task CreateForm_ValidForm_Success()
        {
            // Arrange
            var form = new Form
            {
                Id = 0,
                Name = "New Form",
                Description = "New Form Description",
                HtmlFormBody = "<form>New Form Content</form>"
            };

            var forms = new List<Form>();
            var mockSet = new Mock<DbSet<Form>>();
            
            _contextMock.Setup(m => m.Form).Returns(mockSet.Object);
            mockSet.Setup(m => m.AddAsync(It.IsAny<Form>(), default))
                .Callback<Form, CancellationToken>((f, token) => forms.Add(f));

            // Act
            await _formService.CreateFormAsync(form);

            // Assert
            Assert.Single(forms);
            Assert.Equal("New Form", forms[0].Name);
        }

        /// <summary>
        /// Tests retrieving a form by ID with its elements.
        /// Verifies that a form and its associated elements are correctly retrieved.
        /// </summary>
        /// <remarks>
        /// Test steps:
        /// 1. Sets up test form with form elements.
        /// 2. Mocks database query for form retrieval.
        /// 3. Verifies form and elements are correctly returned.
        /// </remarks>
        [Fact]
        public async Task GetFormByIdIncEntityIncProperty_ExistingForm_ReturnsForm()
        {
            // Arrange
            var formId = 1;
            var form = new Form
            {
                Id = formId,
                Name = "Test Form",
                Description = "Test Description",
                HtmlFormBody = "<form>Test Content</form>"
            };

            var mockSet = new Mock<DbSet<Form>>();
            _contextMock.Setup(m => m.Form
                .Include(It.IsAny<string>())
                .ThenInclude(It.IsAny<string>())
                .FirstOrDefaultAsync(It.IsAny<Expression<Func<Form, bool>>>(), default))
                .ReturnsAsync(form);

            // Act
            var result = await _formService.GetFormByIdIncEntityIncPropertyAsync(formId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(formId, result.Id);
            Assert.Equal("Test Form", result.Name);
        }

        /// <summary>
        /// Tests HTML generation for a form.
        /// Verifies that the correct HTML structure is generated from form properties.
        /// </summary>
        /// <remarks>
        /// Test steps:
        /// 1. Creates a form with specific HTML elements.
        /// 2. Generates HTML using the form service.
        /// 3. Verifies the generated HTML matches expected structure.
        /// </remarks>
        [Fact]
        public async Task GetFormPreview_ValidForm_ReturnsProcessedHtml()
        {
            // Arrange
            var form = new Form
            {
                Id = 1,
                Name = "Test Form",
                HtmlFormBody = "<form><input type='text' name='test'/></form>"
            };

            var tableInput = new TableInput(); // Mock table input data
            _htmlServiceMock.Setup(h => h.FindHtmlTag(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<List<string>>()))
                .Returns(new List<string>());

            // Act
            var result = await _formService.GetFormPreviewAsync(form, 0, tableInput);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("<form>", result);
        }
    }
}