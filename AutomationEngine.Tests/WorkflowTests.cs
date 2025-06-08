using Xunit;
using Services;
using Moq;
using Microsoft.EntityFrameworkCore;
using Entities.Models.WorkFlow;
using DataLayer.DbContext;
using System.Collections.Generic;

namespace AutomationEngine.Tests
{
    /// <summary>
    /// Test suite for workflow management functionality including workflow creation, state transitions, and role associations
    /// </summary>
    public class WorkflowTests
    {
        private readonly Mock<Context> _contextMock;
        private readonly IWorkFlowService _workflowService;

        public WorkflowTests()
        {
            _contextMock = new Mock<Context>();
            _workflowService = new WorkFlowService(_contextMock.Object);
        }

        /// <summary>
        /// Tests creation of a new workflow.
        /// Verifies that a workflow is correctly created with all specified properties.
        /// </summary>
        /// <remarks>
        /// Test steps:
        /// 1. Creates a new workflow with test data
        /// 2. Mocks the database context for workflow creation
        /// 3. Verifies the workflow was created with correct properties
        /// </remarks>
        [Fact]
        public async Task CreateWorkflow_ValidWorkflow_Success()
        {
            // Arrange
            var workflow = new WorkFlow
            {
                Id = 1,
                Name = "Test Workflow",
                Description = "Test Description",
                WorkFlowStatus = WorkFlowStatus.Active
            };

            var workflows = new List<WorkFlow>();
            var mockSet = new Mock<DbSet<WorkFlow>>();
            
            _contextMock.Setup(m => m.WorkFlow).Returns(mockSet.Object);
            mockSet.Setup(m => m.AddAsync(It.IsAny<WorkFlow>(), default))
                .Callback<WorkFlow, CancellationToken>((wf, token) => workflows.Add(wf));

            // Act
            await _workflowService.CreateWorkFlowAsync(workflow);

            // Assert
            Assert.Single(workflows);
            Assert.Equal("Test Workflow", workflows[0].Name);
        }

        /// <summary>
        /// Tests workflow state transition functionality.
        /// Verifies that a workflow's status can be correctly updated from one state to another.
        /// </summary>
        /// <remarks>
        /// Test steps:
        /// 1. Sets up a workflow with initial Draft status
        /// 2. Attempts to transition to Active status
        /// 3. Verifies the status was updated correctly
        /// </remarks>
        [Fact]
        public async Task UpdateWorkflowStatus_ValidTransition_Success()
        {
            // Arrange
            var workflowId = 1;
            var workflow = new WorkFlow
            {
                Id = workflowId,
                Name = "Test Workflow",
                WorkFlowStatus = WorkFlowStatus.Draft
            };

            var mockSet = new Mock<DbSet<WorkFlow>>();
            _contextMock.Setup(m => m.WorkFlow
                .FirstOrDefaultAsync(It.IsAny<Expression<Func<WorkFlow, bool>>>(), default))
                .ReturnsAsync(workflow);

            // Act
            await _workflowService.UpdateWorkFlowStatusAsync(workflowId, WorkFlowStatus.Active);

            // Assert
            Assert.Equal(WorkFlowStatus.Active, workflow.WorkFlowStatus);
        }

        /// <summary>
        /// Tests retrieval of workflow with associated roles.
        /// Verifies that a workflow can be retrieved with its related role assignments.
        /// </summary>
        /// <remarks>
        /// Test steps:
        /// 1. Creates test workflow with associated roles
        /// 2. Mocks database context to return workflow with roles
        /// 3. Verifies workflow and role relationships are correctly retrieved
        /// </remarks>
        [Fact]
        public async Task GetWorkflowByIdWithRoles_ExistingWorkflow_ReturnsWorkflowWithRoles()
        {
            // Arrange
            var workflowId = 1;
            var workflow = new WorkFlow
            {
                Id = workflowId,
                Name = "Test Workflow",
                WorkFlowStatus = WorkFlowStatus.Active,
                WorkFlowRoles = new List<WorkFlowRole>
                {
                    new WorkFlowRole { RoleId = 1 },
                    new WorkFlowRole { RoleId = 2 }
                }
            };

            var mockSet = new Mock<DbSet<WorkFlow>>();
            _contextMock.Setup(m => m.WorkFlow
                .Include(It.IsAny<string>())
                .FirstOrDefaultAsync(It.IsAny<Expression<Func<WorkFlow, bool>>>(), default))
                .ReturnsAsync(workflow);

            // Act
            var result = await _workflowService.GetWorkFlowByIdIncludeRolesAsync(workflowId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(workflowId, result.Id);
            Assert.Equal(2, result.WorkFlowRoles.Count);
        }

        /// <summary>
        /// Tests workflow validation with valid data.
        /// Verifies that a properly configured workflow passes validation checks.
        /// </summary>
        /// <remarks>
        /// Test steps:
        /// 1. Creates a workflow with valid properties
        /// 2. Runs validation on the workflow
        /// 3. Verifies validation passes successfully
        /// </remarks>
        [Fact]
        public void ValidateWorkflow_ValidWorkflow_ReturnsTrue()
        {
            // Arrange
            var workflow = new WorkFlow
            {
                Id = 1,
                Name = "Valid Workflow",
                Description = "Valid Description",
                WorkFlowStatus = WorkFlowStatus.Draft
            };

            // Act
            var result = _workflowService.ValidateWorkFlow(workflow);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Success", result.MessageName);
        }

        /// <summary>
        /// Tests workflow validation with invalid data.
        /// Verifies that validation fails when required properties are missing or invalid.
        /// </summary>
        /// <remarks>
        /// Test steps:
        /// 1. Creates a workflow with invalid properties (empty name)
        /// 2. Runs validation on the workflow
        /// 3. Verifies validation fails with appropriate error message
        /// </remarks>
        [Fact]
        public void ValidateWorkflow_InvalidWorkflow_ReturnsFalse()
        {
            // Arrange
            var workflow = new WorkFlow
            {
                Id = 1,
                Name = "", // Invalid - empty name
                Description = "Valid Description",
                WorkFlowStatus = WorkFlowStatus.Draft
            };

            // Act
            var result = _workflowService.ValidateWorkFlow(workflow);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("WorkFlow", result.Parent);
        }
    }
}