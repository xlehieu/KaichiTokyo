using Newtonsoft.Json;

namespace WebsiteKaichiTokyo.Extension
{
    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session,string key, T Value)
        {
            session.SetString(key, JsonConvert.SerializeObject(Value));
        }
        public static T Get<T>(this ISession session,string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }
}
