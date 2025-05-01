using FrameWork.Model.DTO;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace FrameWork.ExeptionHandler.ExeptionModel
{
    public class CustomException : Exception
    {
        public CustomException(string messageParentKey = "", string messageKey = "", object? logData = null)
        {
            LogData = logData;
            MessageParentKey = messageParentKey;
            MessageKey = messageKey;
        }
        public object? LogData { get; set; }
        public string MessageParentKey { get; set; }
        public string MessageKey { get; set; }
        public string GetMessage()
        {
            return GetMessageJson().Message ?? "خطایی در عملیات رخ داده است (درصورت اطمینان از صحت داده های خود و تکرار مجدد با پشتیبانی تماس حاصل نمایید)";
        }
        public int GetStatusCode()
        {
            return GetMessageJson().StatusCode ?? 503;
        }
        private (int? StatusCode, string? Message) GetMessageJson()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Message.json");
            if (File.Exists(filePath))
            {
                var jsonData = System.IO.File.ReadAllText(filePath);

                var jsonObject = JObject.Parse(jsonData);
                if (jsonObject?["fa"]?[MessageParentKey]?.Children<JProperty>() != null)
                    foreach (var errorType in jsonObject["fa"][MessageParentKey]?.Children<JProperty>())
                    {
                        var statusCode = errorType.Name; // خواندن statusCode مانند Entity، Property، و غیره

                        if (errorType.Value[this.MessageKey] != null)
                        {
                            return (int.Parse(statusCode), (string)errorType.Value[this.MessageKey]);
                        }
                    }
            }
            return (503, "");
        }
    }
    public class CustomException<T> : CustomException
    {
        public CustomException(ValidationDto<T> data, int statusId) : base(data.GetMessage(statusId))
        {
            base.LogData = data.Value;
        }
    }
}
