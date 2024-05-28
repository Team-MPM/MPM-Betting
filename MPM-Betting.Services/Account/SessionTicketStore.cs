using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace MPM_Betting.Services.Account;

public class SessionTicketStore(ProtectedBrowserStorage protectedSessionStorage) : ITicketStore
{
    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        var ticketBytes = await protectedSessionStorage.GetAsync<byte[]>(key);
        return ticketBytes.Value?.DeserializeTicket();
    }

    public Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        throw new NotImplementedException();
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        await protectedSessionStorage.SetAsync(key, ticket.Serialize());
    }

    public async Task StoreAsync(string key, AuthenticationTicket ticket)
    {
        await RenewAsync(key, ticket);
    }

    public async Task RemoveAsync(string key)
    {
        await protectedSessionStorage.DeleteAsync(key);
    }
}

public static class AuthenticationTicketExtensions
{
    public static byte[] Serialize(this AuthenticationTicket ticket)
    {
        using var stream = new MemoryStream();
#pragma warning disable SYSLIB0011
        var formatter = new BinaryFormatter();
#pragma warning restore SYSLIB0011
        formatter.Serialize(stream, ticket);
        return stream.ToArray();
    }

    public static AuthenticationTicket DeserializeTicket(this byte[] data)
    {
        using var stream = new MemoryStream(data);
#pragma warning disable SYSLIB0011
        var formatter = new BinaryFormatter();
#pragma warning restore SYSLIB0011
        return (AuthenticationTicket)formatter.Deserialize(stream);
    }
}