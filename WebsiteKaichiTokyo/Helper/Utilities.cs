using System.Text;
using System.Text.RegularExpressions;

namespace WebsiteKaichiTokyo.Helper
{
    public static class Utilities
    {
        public static int PAGE_SIZE = 20;
        public static int NEWS_PAGE_SIZE = 6;
        public static void CreateIfMissing(string path)
        {
            bool folderExists = Directory.Exists(path);
            if(!folderExists)
            {
                Directory.CreateDirectory(path);
            }
        }
        public static string ToTitleCase(string str)
        {
            string result = str;
            if (!string.IsNullOrEmpty(result))
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
        public static string GetRandomKey(int length = 5)
        {
            string pattern = @"0123456789zxcvbnmasdfghjklqwertyuiop[]{}:~!@#$%^&*()+";
            Random rd = new Random();
            StringBuilder sb = new StringBuilder();
            for (int i=0; i<length; i++)
            {
                sb.Append(pattern[rd.Next(0,pattern.Length)]);
            }
            return sb.ToString();
        }
        public static string SEOUrl(string url)
        {
            url = url.ToLower();
            url = Regex.Replace(url, @"[áàạảãâấầậẩẫăằắẵặẳ]", "a");
            url = Regex.Replace(url, @"[éèéẻẹẽêếềểệễ]", "e");
            url = Regex.Replace(url, @"[óòọõỏồốộỗổôơớờởỡở]", "o");
            url = Regex.Replace(url, @"[íìịĩỉ]", "i");
            url = Regex.Replace(url, @"[ýỳỹỵỷ]", "y");
            url = Regex.Replace(url, @"[úùụũủưứừựữử]", "u");
            url = Regex.Replace(url, @"[đ]", "d");
            url = Regex.Replace(url.Trim(), @"[^0-9a-z\s-]", "").Trim();
            url = Regex.Replace(url.Trim(), @"\s+", "-");
            url = Regex.Replace(url, @"\s", "-");
            while (true)
            {
                if (url.IndexOf("--") != -1)
                {
                    url = url.Replace("--", "-");
                }
                else
                {
                    break;
                }
            }
            return url;
        }
        public static async Task<string>UpLoadFile(IFormFile file,string sDirectory, string newname)
        {
            try
            {
                if(newname == null)
                {
                    newname = file.FileName;
                }
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", sDirectory);
                CreateIfMissing(path);
                string pathFile = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", sDirectory, newname);
                var supportedTypes = new[] { "jpg", "jpeg", "png", "gif" };
                var fileExt =Path.GetExtension(file.FileName).Substring(1);
                if (!supportedTypes.Contains(fileExt.ToLower())) //Khac cac file dinh nghia
                {
                    return null;
                }
                else
                {
                    using (var stream = new FileStream(pathFile,FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    return newname;
                }
            }
            catch
            {
                return null;
            }
        }
        public static bool IsValidEmail(string email)
        {
            if(email.Trim().EndsWith(".")) 
            {
                return false;
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch 
            {
                return false;
            }
        }
    }
}
