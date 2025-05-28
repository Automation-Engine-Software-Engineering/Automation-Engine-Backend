using Microsoft.AspNetCore.Http;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools.AuthoraizationTools;
using Tools.LoggingTools;

namespace AutomationEngine.CustomMiddlewares.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IFormService, FormService>();
            services.AddScoped<IEntityService, EntityService>();
            services.AddScoped<IPropertyService, PropertyService>();
            services.AddScoped<IWorkflowService, WorkflowService>();
            services.AddScoped<IWorkflowUserService, WorkflowUserService>();
            services.AddScoped<IWorkflowRoleService, WorkflowRoleService>();
            services.AddScoped<IRoleUserService, RoleUserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IHtmlService, HtmlService>();
            services.AddScoped<IMenuElementService, MenuElementService>();
            services.AddScoped<IEntityRelationService, EntityRelationService>();
            services.AddSingleton<Logging>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddSingleton<TokenGenerator>();
            services.AddSingleton<EncryptionTool>();
        }
    }
}
