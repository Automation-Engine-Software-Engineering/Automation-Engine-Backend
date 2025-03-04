namespace FrameWork.ExeptionHandler.ExeptionModel
{
    public class CostumExeption : Exception
    {
        public string Message { get; set; }

        public CostumExeption(string Message)
        {
            this.Message = Message;
        }
        public CostumExeption()
        {
            this.Message = "خطایی در سامانه رخ داده است";
        }
        public CostumExeption(Exception ex)
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
