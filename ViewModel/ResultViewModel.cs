using FrameWork;
using FrameWork.ExeptionHandler.ExeptionModel;
using FrameWork.Model.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools.TextTools;

namespace ViewModels
{
    public class ResultViewModel<T>
	{
        public ResultViewModel(T? Data = default)
        {
            var statusCode = ResponseMessageHandler.GetStatusCode("Success", "Success") ?? 200;
            Message = ResponseMessageHandler.GetMessage("Success", "Success");
            StatusCode = statusCode;
            Status = statusCode.ToString().StartsWith("2") ? true : false;
            this.Data = Data;
        }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public bool Status { get; set; } = true;
        public int StatusCode { get; set; } = 200;
        public int TotalCount { get; set; }
        public int ListSize { get; set; }
        public int ListNumber { get; set; }
    }

}
