using FrameWork.Model.DTO;

namespace FrameWork.ExeptionHandler.ExeptionModel
{
    public class CustomException : Exception
    {
        public int StatusId { get; set; }
        public CustomException(string Message) : base(Message) { }
    }
    public class CustomException<T> : CustomException
    {
        public CustomException(ValidationDto<T> data, int statusId) : base(data.GetMessage(statusId))
        {
            base.StatusId = statusId;
        }

    }
}
