namespace FrameWork.ExeptionHandler.ExeptionModel
{
    public class CustomException : Exception
    {
        public int StatusId { get; set; }
        public CustomException(string Message) : base(Message)
        { }
        public CustomException(string Message, int StatusId) : base(Message)
        {
            this.StatusId = StatusId;
        }
        public CustomException() : base("خطایی در سامانه رخ داده است")
        {
            this.StatusId = 503;
        }
        public CustomException(Exception ex) : base("خطایی در سامانه رخ داده است")
        {
            Console.WriteLine(ex);
            this.StatusId = 503;
        }
    }
}
