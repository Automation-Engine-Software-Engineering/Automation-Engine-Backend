using System.Text.RegularExpressions;
using DataLayer.DbContext;
using HtmlAgilityPack;

namespace Services
{
    public interface IHtmlService
    {
        Task<List<string>> FindeHtmlTag(string htmBody, string htmlTag, List<string> attributes);
        Task<string> InsertTag(string parentTag, List<string> childTags);
        Task<string> getTagAttributesValue(string TagBody, string attributeName);
        Task<List<string>> getAttributeConditionValues(string input);
    }

    public class HtmlService : IHtmlService
    {
        private readonly DbContext _context;
        public HtmlService(DbContext context)
        {
            _context = context;
        }

        public async Task<List<string>> FindeHtmlTag(string htmBody, string htmlTag, List<string> attributes)
        {
            var result = new List<string>();

            if (string.IsNullOrWhiteSpace(htmBody) || string.IsNullOrWhiteSpace(htmlTag) || attributes == null || attributes.Count == 0)
            {
                return result;
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmBody);

            var nodes = htmlDoc.DocumentNode.Descendants(htmlTag)
                .Where(node => attributes.All(attr => node.Attributes[attr] != null));

            foreach (var node in nodes)
            {
                result.Add(node.OuterHtml);
            }

            return result;
        }

        public async Task<List<string>> getAttributeConditionValues(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return new List<string>();
            }

            string pattern = "\\{\\{(.*?)\\}\\}";
            var matches = Regex.Matches(input, pattern);
            var results = new List<string>();

            foreach (Match match in matches)
            {
                results.Add(match.Groups[1].Value);
            }

            return results;
        }

        public async Task<string> getTagAttributesValue(string TagBody, string attributeName)
        {
            if (string.IsNullOrWhiteSpace(TagBody) || string.IsNullOrWhiteSpace(attributeName))
            {
                return null;
            }

            string pattern = $"{attributeName}=['\\\"](.*?)['\\\"]";
            var match = Regex.Match(TagBody, pattern);

            return match.Success ? match.Groups[1].Value : null;
        }

        public async Task<string> InsertTag(string parentTag, List<string> childTags)
        {
            if (string.IsNullOrWhiteSpace(parentTag) || childTags == null || childTags.Count == 0)
            {
                return parentTag;
            }

            int closingTagIndex = parentTag.IndexOf("</");
            if (closingTagIndex == -1)
            {
                throw new ArgumentException("Invalid primary tag format.");
            }

            string openingPart = parentTag.Substring(0, closingTagIndex);
            string closingPart = parentTag.Substring(closingTagIndex);

            string embeddedContent = string.Join("\n", childTags);

            return openingPart + "\n" + embeddedContent + "\n" + closingPart;
        }
    }
}
