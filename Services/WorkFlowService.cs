using DataLayer.Context;
using DataLayer.Models.FormBuilder;
using DataLayer.Models.TableBuilder;
using DataLayer.Models.WorkFlow;
using FrameWork.ExeptionHandler.ExeptionModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;
using ViewModels.ViewModels.Unknown;

namespace Services
{
    public interface IWorkFlowService
    {
        Task InsertWorFlow(WorkFlow workFlow);
        Task UpdateWorFlow(WorkFlow workFlow);
        Task DeleteWorFlow(int id);
        Task<WorkFlow> GetWorFlowById(int id);
        Task<List<WorkFlow>> GetAllWorFlows();
        Task<UnknownDto> GetWorFlowValueById(int id , int userId);
        Task<UnknownDto> GetNextWorFlowValueById(int id, int userId);
        Task<UnknownDto> GetLastWorFlowValueById(int id, int userId);
        Task SaveChangesAsync();
    }
    public class WorkFlowService : IWorkFlowService
    {
        private readonly Context _context;
        public WorkFlowService(Context context)
        {
            _context = context;
        }
        public async Task DeleteWorFlow(int id)
        {
            if (id == null) throw new CustomExeption("گردشکار معتبر نمی باشد");

            var result = _context.WorkFlow.FirstOrDefault(x => x.Id == id)
                  ?? throw new CustomExeption("گردشکار یافت نشد.");

            _context.Remove(result);
        }

        public async Task<List<WorkFlow>> GetAllWorFlows()
        {
            var result = _context.WorkFlow.Include(x => x.Nodes).Include(x => x.Edges).ToList();
            return result;
        }

        public async Task<UnknownDto> GetLastWorFlowValueById(int idWorkflowUser, int userId)
        {
            if (idWorkflowUser == null) throw new CustomExeption("گردشکار معتبر نمی باشد");

            var userWorkFlow = _context.WorkFlow_User.FirstOrDefault(x => x.WorkFlowId == idWorkflowUser && x.UserId == userId)
               ?? throw new CustomExeption("گردشکار یافت نشد.");

            var workflow = _context.WorkFlow.Include(x => x.Nodes).Include(x => x.Edges)
                .FirstOrDefault(x => x.Id == userWorkFlow.WorkFlowId)
               ?? throw new CustomExeption("گردشکار با خطا مواجه شد.");

            var result = workflow.Nodes.FirstOrDefault(x => x.Id == userWorkFlow.WorkFlowState)
                ?? throw new CustomExeption("گردشکار با خطا مواجه شد.");

            var lastEdge = workflow.Edges.FirstOrDefault(x => x.Target == result.Id)
                ?? throw new CustomExeption("گردشکار با خطا مواجه شد.");

            var LastNode = workflow.Nodes.FirstOrDefault(x => x.Id == lastEdge.Source)
                ?? throw new CustomExeption("گردشکار با خطا مواجه شد.");

            userWorkFlow.WorkFlowState = LastNode.Id;
            _context.WorkFlow_User.Update(userWorkFlow);
            _context.SaveChanges();

            return new UnknownDto()
            {
                Type = LastNode.Type,
                DataId = LastNode.Type == UnknownType.form ? LastNode.formId :
                result.Type == UnknownType.table ?  result.entityId: 0
            };
        }
        public async Task<UnknownDto> GetWorFlowValueById(int idWorkflowUser , int userId)
        {
            if (idWorkflowUser == null) throw new CustomExeption("گردشکار معتبر نمی باشد");

            var userWorkFlow = _context.WorkFlow_User.FirstOrDefault(x => x.WorkFlowId == idWorkflowUser && x.UserId == userId)
               ?? throw new CustomExeption("گردشکار یافت نشد.");

            var workflow = _context.WorkFlow.Include(x => x.Nodes).Include(x => x.Edges)
                .FirstOrDefault(x => x.Id == userWorkFlow.WorkFlowId)
               ?? throw new CustomExeption("گردشکار یافت نشد.");

            var result = workflow.Nodes.FirstOrDefault(x => x.Id == userWorkFlow.WorkFlowState)
                ?? throw new CustomExeption("گردشکار یافت نشد.");

            return new UnknownDto()
            {
                Type = result.Type,
                DataId = result.Type == UnknownType.form ?  result.formId:
                result.Type == UnknownType.table ? result.entityId : 0
            };
        }

        public async Task<UnknownDto> GetNextWorFlowValueById(int idWorkflowUser, int userId)
        {
            if (idWorkflowUser == null) throw new CustomExeption("گردشکار معتبر نمی باشد");

            var userWorkFlow = _context.WorkFlow_User.FirstOrDefault(x => x.WorkFlowId == idWorkflowUser && x.UserId == userId)
               ?? throw new CustomExeption("گردشکار یافت نشد.");

            var workflow = _context.WorkFlow.Include(x => x.Nodes).Include(x => x.Edges)
                .FirstOrDefault(x => x.Id == userWorkFlow.WorkFlowId)
               ?? throw new CustomExeption("گردشکار با خطا مواجه شد.");

            var result = workflow.Nodes.FirstOrDefault(x => x.Id == userWorkFlow.WorkFlowState)
                ?? throw new CustomExeption("گردشکار با خطا مواجه شد.");

            var NextEdge = workflow.Edges.FirstOrDefault(x => x.Target == result.Id)
                ?? throw new CustomExeption("گردشکار با خطا مواجه شد.");

            var NextNode = workflow.Nodes.FirstOrDefault(x => x.Id == NextEdge.Target)
                ?? throw new CustomExeption("گردشکار با خطا مواجه شد.");

            userWorkFlow.WorkFlowState = NextNode.Id;
            _context.WorkFlow_User.Update(userWorkFlow);
            _context.SaveChanges();

            return new UnknownDto()
            {
                Type = NextNode.Type,
                DataId = NextNode.Type == UnknownType.form ?  NextNode.formId:
                NextNode.Type == UnknownType.table ? NextNode.entityId : 0
            };
        }

        public async Task<WorkFlow> GetWorFlowById(int id)
        {
            if (id == null) throw new CustomExeption("گردشکار معتبر نمی باشد");
            var result = _context.WorkFlow.Include(x => x.Nodes).Include(x => x.Edges).FirstOrDefault(x => x.Id == id)
               ?? throw new CustomExeption("گردشکار یافت نشد.");

            return result;
        }

        public async Task InsertWorFlow(WorkFlow workFlow)
        {
            await WorkFlowValidation(workFlow);


            await _context.WorkFlow.AddAsync(workFlow);
        }

        public async Task UpdateWorFlow(WorkFlow workFlow)
        {
            await WorkFlowValidation(workFlow);
            if (workFlow.Id == null) throw new CustomExeption("گردشکار معتبر نمی باشد");

            var result = _context.WorkFlow.FirstOrDefault(x => x.Id == workFlow.Id)
               ?? throw new CustomExeption("گردشکار یافت نشد.");

            var feachModel = new WorkFlow()
            {
                Nodes = workFlow.Nodes,
                Edges = workFlow.Edges,
                Id = result.Id
            };
            _context.Update(feachModel);
        }

        public async Task<string> WorkFlowValidation(WorkFlow workFlow)
        {
            if (workFlow == null) throw new CustomExeption("اطلاعات گردشکار معتبر نمی باشد");
            if (workFlow.Nodes == null) throw new CustomExeption("اطلاعات گردشکار معتبر نمی باشد");
            if (workFlow.Edges == null) throw new CustomExeption("اطلاعات گردشکار معتبر نمی باشد");
            if (workFlow.Nodes.Any(x => !x.Name.IsValidateString() || x.Type == null || x.Icon == null || x.X == null || x.Y == null)) throw new CustomExeption("اطلاعات گردشکار معتبر نمی باشد");
            if (workFlow.Edges.Any(x => x.Source == null || x.Target == null || x.SourceHandle == null || x.TargetHandle == null)) throw new CustomExeption("اطلاعات گردشکار معتبر نمی باشد");

            var isRotate = false;
            workFlow.Edges.ForEach(x =>
            {
                if (workFlow.Edges.Where(x => x.Source == x.Source).ToList().Count > 1)
                {
                    isRotate = true;
                }
                if (workFlow.Edges.Where(x => x.Target == x.Target).ToList().Count > 1)
                {
                    isRotate = true;
                }
            });

            if (isRotate) throw new CustomExeption("تمامی مراحل تنها باید دارای یک خروجی و یک ورودی باشند");
            return "";
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new CustomExeption();
            }
        }
    }
}
