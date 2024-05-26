using System.Text.RegularExpressions;

namespace WebsiteKaichiTokyo.Extension
{
    public static class Extension
    {
        public static string ToVnd(this double donGia)
        {
            return donGia.ToString("#,##0") + "đ";
        }
        public static string ToTitleCase(string str)
        {
            string result = str;
            if(!string.IsNullOrEmpty(result))
            {
                var words = result.Split(' ');
                for (int i = 0; i < words.Length; i++)
                {
                    var s = words[i];
                    if (s.Length > 0)
                    {
                        words[i] = s[0].ToString().ToUpper() + s.Substring(1);
                    }
                }
                result = string.Join(" ", words);
            }
            return result;
        } 
        public static string ToUrlFriendly(this string url)
        {
            var result = url.ToLower().Trim();
            result = Regex.Replace(result, "áàạảãâấầậẩẫăằắẵặẳ", "a");
            result = Regex.Replace(result, "éèéẻẹẽêếềểệễ", "e");
            result = Regex.Replace(result, "óòọõỏồốộỗổôơớờởỡở" ,"o");
            result = Regex.Replace(result, "íìịĩỉ", "i");
            result = Regex.Replace(result, "ýỳỹỵỷ", "y");
            result = Regex.Replace(result, "úùụũủưứừựữử", "u");
            result = Regex.Replace(result, "đ", "d");
            result = Regex.Replace(result, "[^a-z0-9-]", "");
            result = Regex.Replace(result, "(-)+", "-");

            return result;
        }
    }
}
