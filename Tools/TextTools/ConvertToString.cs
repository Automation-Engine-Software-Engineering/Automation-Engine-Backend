using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.TextTools
{
    public class ConvertToString
    {
        public static string ConvertObjectToString(object input)
        {
            if (input == null)
            {
                return "null";
            }

            if (input is string str)
            {
                return str;
            }

            if (input is IEnumerable enumerable)
            {
                var result = new StringBuilder();
                result.Append("[");
                foreach (var item in enumerable)
                {
                    result.Append(ConvertObjectToString(item) + ", ");
                }

                if (result.Length > 1)
                    result.Remove(result.Length - 2, 2); // حذف ویرگول اضافی

                result.Append("]");
                return result.ToString();
            }

            if (input.GetType().IsPrimitive || input is decimal)
            {
                return input.ToString();
            }

            try
            {
                return JsonConvert.SerializeObject(input, Formatting.Indented);
            }
            catch
            {
                var result = new StringBuilder();
                var properties = input.GetType().GetProperties();

                result.Append("{ ");
                foreach (var property in properties)
                {
                    var name = property.Name;
                    var value = property.GetValue(input, null);
                    result.Append($"{name}: {ConvertObjectToString(value)}, ");
                }

                if (result.Length > 2)
                    result.Remove(result.Length - 2, 2); 

                result.Append(" }");
                return result.ToString();
            }
        }
    }
}
