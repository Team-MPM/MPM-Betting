using System.Diagnostics.Contracts;
using System.Globalization;
using LanguageExt.Common;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json.Linq;

namespace MPM_Betting.Services.Data;

public static class Utils
{
    public static T TryGetValue<T>(this JToken jToken, string propertyName, T defaultValue = default!)
    {
        try
        {
            var val = jToken[propertyName]!.Value<T>();
            Console.WriteLine("fail");
            if (val is null) throw new Exception();
            Console.WriteLine("success");
            return val;
        }
        catch
        {
            return defaultValue;
        }
    }
    
    public static DateTime? TryGetDateTime(this JToken jToken, string propertyName, string format, DateTime? defaultValue = default!)
    {
        try
        {
            var val = DateTime.ParseExact(jToken[propertyName]!.Value<string>()!, format, CultureInfo.InvariantCulture);
            return val;
        }
        catch
        {
            return defaultValue;
        }
    }
    
    public static MpmResult<T> Try<T>(Func<T> func)
    {
        try
        {
            return func();
        }
        catch (Exception e)
        {
            return e;
        }
    }
}