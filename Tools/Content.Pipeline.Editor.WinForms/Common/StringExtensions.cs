 using System;
 using System.Security;

namespace Content.Pipeline.Editor
{
    public static class StringExtensions
    {
        public static string Unescape(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var result = Uri.UnescapeDataString(text);

            // JCF: XmlReader already does this.
            /*
            result = result.Replace("&apos;", "'");
            result = result.Replace("&quot;", "\"");
            result = result.Replace("&gt;", ">");
            result = result.Replace("&amp;", "&");
            */             
            
            return result;
        }
    }
}