//https://gist.github.com/tompazourek/10017430
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
namespace MvcTagBuilder
{
    public enum TagRenderMode
    {
        Normal,
        StartTag,
        EndTag,
        SelfClosing,
    }

    public class TagBuilder
    {
        private string _idAttributeDotReplacement;
        private string _innerHtml;

        public IDictionary<string, string> Attributes { get; private set; }

        public static Dictionary<string, string> AnonymousObjectToHtmlAttributes(object htmlAttributes)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            if (htmlAttributes != null)
            {
                foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(htmlAttributes))
                    dict.Add(propertyDescriptor.Name.Replace('_', '-'), propertyDescriptor.GetValue(htmlAttributes).ToString());
            }
            return dict;
        }

        private const string HtmlHelperIdAttributeDotReplacement = "_";

        public string IdAttributeDotReplacement
        {
            get
            {
                if (string.IsNullOrEmpty(this._idAttributeDotReplacement))
                    this._idAttributeDotReplacement = HtmlHelperIdAttributeDotReplacement;
                return this._idAttributeDotReplacement;
            }
            set
            {
                this._idAttributeDotReplacement = value;
            }
        }

        public string InnerHtml
        {
            get
            {
                return this._innerHtml ?? string.Empty;
            }
            set
            {
                this._innerHtml = value;
            }
        }

        public string TagName { get; private set; }

        public TagBuilder(string tagName)
        {
            if (string.IsNullOrEmpty(tagName))
                throw new ArgumentException("Tag name cannot be null or empty", "tagName");
            this.TagName = tagName;
            this.Attributes = (IDictionary<string, string>)new SortedDictionary<string, string>((IComparer<string>)StringComparer.Ordinal);
        }

        public void AddCssClass(string value)
        {
            string str;
            if (this.Attributes.TryGetValue("class", out str))
                this.Attributes["class"] = value + " " + str;
            else
                this.Attributes["class"] = value;
        }

        public static string CreateSanitizedId(string originalId)
        {
            return TagBuilder.CreateSanitizedId(originalId, HtmlHelperIdAttributeDotReplacement);
        }

        public static string CreateSanitizedId(string originalId, string invalidCharReplacement)
        {
            if (string.IsNullOrEmpty(originalId))
                return (string)null;
            if (invalidCharReplacement == null)
                throw new ArgumentNullException("invalidCharReplacement");
            char c1 = originalId[0];
            if (!TagBuilder.Html401IdUtil.IsLetter(c1))
                return (string)null;
            StringBuilder stringBuilder = new StringBuilder(originalId.Length);
            stringBuilder.Append(c1);
            for (int index = 1; index < originalId.Length; ++index)
            {
                char c2 = originalId[index];
                if (TagBuilder.Html401IdUtil.IsValidIdCharacter(c2))
                    stringBuilder.Append(c2);
                else
                    stringBuilder.Append(invalidCharReplacement);
            }
            return ((object)stringBuilder).ToString();
        }

        public void GenerateId(string name)
        {
            if (this.Attributes.ContainsKey("id"))
                return;
            string sanitizedId = TagBuilder.CreateSanitizedId(name, this.IdAttributeDotReplacement);
            if (string.IsNullOrEmpty(sanitizedId))
                return;
            this.Attributes["id"] = sanitizedId;
        }

        private void AppendAttributes(StringBuilder sb)
        {
            foreach (KeyValuePair<string, string> keyValuePair in (IEnumerable<KeyValuePair<string, string>>)this.Attributes)
            {
                string key = keyValuePair.Key;
                if (!string.Equals(key, "id", StringComparison.Ordinal) || !string.IsNullOrEmpty(keyValuePair.Value))
                {
                    string str = HttpUtilityHtmlAttributeEncode(keyValuePair.Value);
                    sb.Append(' ').Append(key).Append("=\"").Append(str).Append('"');
                }
            }
        }

        internal string HttpUtilityHtmlAttributeEncode(string value)
        {
            if (string.IsNullOrEmpty(value) || HttpEncoderIndexOfHtmlAttributeEncodingChars(value, 0) == -1)
                return value;
            StringWriter stringWriter = new StringWriter((IFormatProvider)CultureInfo.InvariantCulture);
            this.HttpUtilityHtmlAttributeEncode(value, (TextWriter)stringWriter);
            return stringWriter.ToString();
        }

        protected internal virtual void HttpUtilityHtmlAttributeEncode(string value, TextWriter output)
        {
            if (value == null)
                return;
            if (output == null)
                throw new ArgumentNullException("output");
            HttpEncoderHtmlAttributeEncodeInternal(value, output);
        }

        private static void HttpEncoderHtmlAttributeEncodeInternal(string s, TextWriter output)
        {
            int num1 = HttpEncoderIndexOfHtmlAttributeEncodingChars(s, 0);
            if (num1 == -1)
            {
                output.Write(s);
            }
            else
            {
                int num2 = s.Length - num1;
                int chPtr1 = 0;
                {
                    int chPtr2 = chPtr1;
                    while (num1-- > 0)
                        output.Write(s[chPtr2++]);
                    while (num2-- > 0)
                    {
                        char ch = s[chPtr2++];
                        if ((int)ch <= 60)
                        {
                            switch (ch)
                            {
                                case '"':
                                    output.Write("&quot;");
                                    continue;
                                case '&':
                                    output.Write("&amp;");
                                    continue;
                                case '\'':
                                    output.Write("&#39;");
                                    continue;
                                case '<':
                                    output.Write("&lt;");
                                    continue;
                                default:
                                    output.Write(ch);
                                    continue;
                            }
                        }
                        else
                            output.Write(ch);
                    }
                }
            }
        }

        private static int HttpEncoderIndexOfHtmlAttributeEncodingChars(string s, int startPos)
        {
            int cch = s.Length - startPos;
            {
                for (int pch = startPos; cch > 0; pch++, cch--)
                {
                    char ch = s[pch];
                    if (ch <= '<')
                    {
                        switch (ch)
                        {
                            case '<':
                            case '"':
                            case '\'':
                            case '&':
                                return s.Length - cch;
                        }
                    }
                }
            }

            return -1;
        }

        public void MergeAttribute(string key, string value)
        {
            TagBuilder tagBuilder = this;
            bool flag = false;
            string key1 = key;
            string str = value;
            int num = flag ? 1 : 0;
            tagBuilder.MergeAttribute(key1, str, num != 0);
        }

        public void MergeAttribute(string key, string value, bool replaceExisting)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty", "key");
            if (!replaceExisting && this.Attributes.ContainsKey(key))
                return;
            this.Attributes[key] = value;
        }

        public void MergeAttributes(object attributes)
        {
            TagBuilder tagBuilder = this;
            bool flag = false;
            IDictionary<string, string> attributes1 = AnonymousObjectToHtmlAttributes(attributes);
            int num = flag ? 1 : 0;
            tagBuilder.MergeAttributes<string, string>(attributes1, num != 0);
        }

        public void MergeAttributes(object attributes, bool replaceExisting)
        {
            if (attributes == null)
                return;
            foreach (KeyValuePair<string, string> keyValuePair in (IEnumerable<KeyValuePair<string, string>>)attributes)
                this.MergeAttribute(Convert.ToString((object)keyValuePair.Key, (IFormatProvider)CultureInfo.InvariantCulture), Convert.ToString((object)keyValuePair.Value, (IFormatProvider)CultureInfo.InvariantCulture), replaceExisting);
        }

        public void MergeAttributes<TKey, TValue>(IDictionary<TKey, TValue> attributes)
        {
            TagBuilder tagBuilder = this;
            bool flag = false;
            IDictionary<TKey, TValue> attributes1 = attributes;
            int num = flag ? 1 : 0;
            tagBuilder.MergeAttributes<TKey, TValue>(attributes1, num != 0);
        }

        public void MergeAttributes<TKey, TValue>(IDictionary<TKey, TValue> attributes, bool replaceExisting)
        {
            if (attributes == null)
                return;
            foreach (KeyValuePair<TKey, TValue> keyValuePair in (IEnumerable<KeyValuePair<TKey, TValue>>)attributes)
                this.MergeAttribute(Convert.ToString((object)keyValuePair.Key, (IFormatProvider)CultureInfo.InvariantCulture), Convert.ToString((object)keyValuePair.Value, (IFormatProvider)CultureInfo.InvariantCulture), replaceExisting);
        }

        public void SetInnerText(string innerText)
        {
            this.InnerHtml = HttpUtility.HtmlEncode(innerText);
        }

        public override string ToString()
        {
            return this.ToString(TagRenderMode.Normal);
        }

        public string ToString(TagRenderMode renderMode)
        {
            StringBuilder sb = new StringBuilder();
            switch (renderMode)
            {
                case TagRenderMode.StartTag:
                    sb.Append('<').Append(this.TagName);
                    this.AppendAttributes(sb);
                    sb.Append('>');
                    break;

                case TagRenderMode.EndTag:
                    sb.Append("</").Append(this.TagName).Append('>');
                    break;

                case TagRenderMode.SelfClosing:
                    sb.Append('<').Append(this.TagName);
                    this.AppendAttributes(sb);
                    sb.Append(" />");
                    break;

                default:
                    sb.Append('<').Append(this.TagName);
                    this.AppendAttributes(sb);
                    sb.Append('>').Append(this.InnerHtml).Append("</").Append(this.TagName).Append('>');
                    break;
            }
            return ((object)sb).ToString();
        }

        private static class Html401IdUtil
        {
            private static bool IsAllowableSpecialCharacter(char c)
            {
                switch (c)
                {
                    case '-':
                    case ':':
                    case '_':
                        return true;

                    default:
                        return false;
                }
            }

            private static bool IsDigit(char c)
            {
                if (48 <= (int)c)
                    return (int)c <= 57;
                else
                    return false;
            }

            public static bool IsLetter(char c)
            {
                if (65 <= (int)c && (int)c <= 90)
                    return true;
                if (97 <= (int)c)
                    return (int)c <= 122;
                else
                    return false;
            }

            public static bool IsValidIdCharacter(char c)
            {
                if (!TagBuilder.Html401IdUtil.IsLetter(c) && !TagBuilder.Html401IdUtil.IsDigit(c))
                    return TagBuilder.Html401IdUtil.IsAllowableSpecialCharacter(c);
                else
                    return true;
            }
        }
    }
}