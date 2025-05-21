using System.Text.RegularExpressions;
using DataLayer.DbContext;
using HtmlAgilityPack;

namespace Services
{
    public interface IHtmlService
    {
        List<string> FindHtmlTag(string htmBody, string htmlTag, List<string> attributes);
        List<string> FindSingleHtmlTag(string htmBody, string htmlTag, List<string> attributes);
        List<string> FindSingleHtmlTag(string htmlBody);
        string InsertTag(string parentTag, List<string> childTags);
        string InsertTagWithRemoveAllChild(string tag, string parentTag, string childTags);
        string? GetTagAttributesValue(string TagBody, string attributeName);
        List<string> GetAttributeConditionValues(string input);
        string ExtractContentAfterTableCell(string htmlBody);
        List<string> ExtractWorkflowUserValues(string html);
    }

    public class HtmlService : IHtmlService
    {
        private readonly Context _context;
        public HtmlService(Context context)
        {
            _context = context;
        }
        public List<string> ExtractWorkflowUserValues(string html)
        {
            List<string> values = new List<string>();
            string pattern = @"data-workflow-user=""([^""]*)""";
            MatchCollection matches = Regex.Matches(html, pattern);
            foreach (Match match in matches)
            {
                values.Add(match.Groups[1].Value);
            }
            return values;
        }
        public List<string> FindHtmlTag(string htmlBody, string htmlTag, List<string> attributes)
        {
            var result = new List<string>();

            if (string.IsNullOrWhiteSpace(htmlBody) || string.IsNullOrWhiteSpace(htmlTag) || attributes == null || attributes.Count == 0)
            {
                return result;
            }

            string pattern = $@"<{htmlTag}\b[^>]*\b(?:{string.Join("|", attributes.ConvertAll(a => $@"\b{Regex.Escape(a)}=""[^""]*"""))})[^>]*>(.*?)</{htmlTag}>";
            var matches = Regex.Matches(htmlBody, pattern, RegexOptions.Singleline);

            foreach (Match match in matches)
            {
                result.Add(match.Value);
            }

            return result;
        }
        public List<string> FindSingleHtmlTag(string htmlBody, string htmlTag, List<string> attributes)
        {
            var result = new List<string>();

            if (string.IsNullOrWhiteSpace(htmlBody) || string.IsNullOrWhiteSpace(htmlTag) || attributes == null || attributes.Count == 0)
            {
                return result;
            }

            string pattern = $@"<{htmlTag}\b[^>]*\b(?:{string.Join("|", attributes.ConvertAll(a => $@"\b{Regex.Escape(a)}=""[^""]*"""))})[^>]*>(.*?)";
            var matches = Regex.Matches(htmlBody, pattern, RegexOptions.Singleline);

            foreach (Match match in matches)
            {
                result.Add(match.Value);
            }

            return result;
        }

        public List<string> FindSingleHtmlTag(string htmlBody)
        {
            var result = new List<string>();

            if (string.IsNullOrWhiteSpace(htmlBody))
            {
                return result;
            }

            string pattern = @"<input\b[^>]*\bdata-current-date=""true""[^>]*>";
            var matches = Regex.Matches(htmlBody, pattern, RegexOptions.Singleline);

            foreach (Match match in matches)
            {
                result.Add(match.Value);
            }

            return result;
        }

        public string ExtractContentAfterTableCell(string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(htmlBody))
            {
                return string.Empty;
            }

            string pattern = @"<td>\n\s*پیش\s*نمایش\s*جدول\s*\n\s*</td>(.*?)</tbody>";
            var match = Regex.Match(htmlBody, pattern, RegexOptions.Singleline);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return string.Empty;
        }

        public List<string> GetAttributeConditionValues(string input)
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

        public string? GetTagAttributesValue(string TagBody, string attributeName)
        {
            if (string.IsNullOrWhiteSpace(TagBody) || string.IsNullOrWhiteSpace(attributeName))
            {
                return null;
            }

            string pattern = $"{attributeName}=\"(.*?)\"";
            var match = Regex.Match(TagBody, pattern);

            return match.Success ? match.Groups[1].Value : null;
        }

        public string InsertTag(string parentTag, List<string> childTags)
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

            closingPart = RemoveSpecificTag(closingPart);

            string embeddedContent = string.Join("\n", childTags);

            return openingPart + "\n" + embeddedContent + "\n" + closingPart;
        }

        public string InsertTagWithRemoveAllChild(string tag, string parentTag, string childTags)
        {
            if (string.IsNullOrWhiteSpace(parentTag) || childTags == null || childTags == null)
            {
                return parentTag;
            }

            int closingTagIndex = parentTag.IndexOf($"</{tag}");
            if (closingTagIndex == -1)
            {
                throw new ArgumentException("Invalid primary tag format.");
            }

            string openingPart = parentTag.Substring(0, closingTagIndex);
            string closingPart = parentTag.Substring(closingTagIndex);

            // حذف تمامی تگ‌های داخل تگ پدر
            int openingTagEndIndex = openingPart.IndexOf('>') + 1;
            openingPart = openingPart.Substring(0, openingTagEndIndex);

            return openingPart + "\n" + childTags + "\n" + closingPart;
        }

        public string RemoveSpecificTag(string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(htmlBody))
            {
                return string.Empty;
            }

            // بررسی وجود عبارت data-listener-added
            if (htmlBody.Contains("data-listener-added"))
            {
                // الگوی حذف آخرین تگ <td>
                string pattern = @"(<td\b[^>]*>.*?</td>)(?!.*<td\b[^>]*>.*?</td>)";
                htmlBody = Regex.Replace(htmlBody, pattern, string.Empty, RegexOptions.Singleline);
            }

            return htmlBody;
        }
    }
}
