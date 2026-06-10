using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.Repositories;
using StackExchange.Redis;

namespace WebIde.Web.Services;

// IXmlRepository backed by Redis — used instead of PersistKeysToStackExchangeRedis
// so that IConnectionMultiplexer can be resolved lazily from DI (enabling test factories
// to swap in a mock before the first connection attempt).
internal sealed class RedisDataProtectionRepository(IConnectionMultiplexer redis) : IXmlRepository
{
    private const string Key = "DataProtection-Keys";

    public IReadOnlyCollection<XElement> GetAllElements()
    {
        var db = redis.GetDatabase();
        return db.ListRange(Key)
                 .Where(v => !v.IsNull)
                 .Select(v => XElement.Parse((string)v!))
                 .ToList()
                 .AsReadOnly();
    }

    public void StoreElement(XElement element, string friendlyName)
    {
        var db = redis.GetDatabase();
        db.ListRightPush(Key, element.ToString(SaveOptions.DisableFormatting));
    }
}
