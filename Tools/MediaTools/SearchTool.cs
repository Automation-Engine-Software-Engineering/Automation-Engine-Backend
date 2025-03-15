using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.MediaTools
{
    public class SearchTool
    {
        public static IEnumerable<string> FindFirstFile(string searchPattern,string rootDirectory, SearchOption searchOption = SearchOption.AllDirectories)
        {
            IEnumerable<string> result = new List<string>();
            try
            {
                if (!Directory.Exists(rootDirectory))
                    return result;

                result = Directory.EnumerateFiles(rootDirectory, searchPattern, searchOption);

                return result;
            }
            catch (Exception)
            {
                return result;
            }

        }
    }
}
