namespace FrameWork.ExeptionHandler.ExeptionModel
{
    public class CustomExeption : Exception
    {
        public string Message { get; set; }

        public CustomExeption(string Message)
        {
            this.Message = Message;
        }
        public CustomExeption()
        {
            this.Message = "خطایی در سامانه رخ داده است";
        }
        public CustomExeption(Exception ex)
        {
            Console.WriteLine(ex);
            this.Message = "خطایی در سامانه رخ داده است";
        }
        public string ThrowExeption()
        {
            return this.Message;
        }
    }
}
