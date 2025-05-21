using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models.Enums
{
    public enum WorkflowEnum
    {
        دبیرخانه = 1,
        روندنگار = 2,
        مدیریت = 3 
    }
    public enum UnknownType
    {
        Form = 1, Dynamic = 2
    }
    public enum PropertyType
    {
        INT = 1,
        Float = 2,
        NvarcharShort = 3,
        NvarcharLong = 4,
        BIT = 5,
        Time = 6,
        BinaryLong = 7,
        Color = 8,
        Email = 9,
        Password = 10,
        Select = 11,
        Editor = 12,
        TextArea = 13,
    }
    public enum LogLevel
    {
        Critical,
        Error,
        Warning,
        Information,
        Debug,
        Trace
    }

}
