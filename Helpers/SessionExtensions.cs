using Newtonsoft.Json;

namespace ShopWebMVC.Helpers
{
    public static class SessionExtensions
    {
        // حفظ الكائن كـ JSON في الـ Session
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        // استرجاع الكائن من الـ Session
        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonConvert.DeserializeObject<T>(value);
        }
    }
}
