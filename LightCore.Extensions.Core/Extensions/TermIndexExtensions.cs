using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NetCore.Extensions.Core.Extensions
{
    public static class TermIndexExtensions
    {
        private static readonly Regex latinRegex = new Regex("^[a-z]+$", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        /// <summary>
        /// Break words into terms to index purpose
        /// </summary>
        /// <returns></returns>
        public static ICollection<string> GetTerms(this string name)
        {
            var words = name.Normalize().Split(new[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var collection = new List<string>();
            foreach (var w in words)
            {
                foreach (var term in WorkToTerm(w))
                {
                    collection.Add(term);
                }
            }
            return collection.AsReadOnly();
        }

        private static IEnumerable<string> WorkToTerm(string w)
        {
            if (!IsLatinWord(w))
            {
                foreach (var c in w)
                {
                    yield return c.ToString();
                }
            }
            else
            {
                yield return Termalize(w);
            }
        }

        private static string Termalize(string w)
        {
            return w;
        }

        /// <summary>
        /// </summary>
        /// <param name="normalizedWord">normalized word (contains a-Z and numbers only)</param>
        /// <returns></returns>
        private static bool IsLatinWord(this string normalizedWord)
        {
            return latinRegex.IsMatch(normalizedWord);
        }

        private static string Normalize(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            text = text.ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var chars = text.Where(c =>
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(c);
                return
                    category == UnicodeCategory.LowercaseLetter ||
                    category == UnicodeCategory.OtherLetter ||
                    category == UnicodeCategory.SpaceSeparator;
            }).ToArray();

            return new string(chars).Normalize(NormalizationForm.FormC);
        }
    }
}
