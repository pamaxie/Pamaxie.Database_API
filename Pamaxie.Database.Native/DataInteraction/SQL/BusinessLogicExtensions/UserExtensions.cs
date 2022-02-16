using Pamaxie.Data;
using Pamaxie.Database.Native.Sql;

namespace Pamaxie.Database.Native.DataInteraction.BusinessLogicExtensions;

public static class UserExtensions
{
    public static IPamUser ToIPamUser(this User user)
    {
        var pamUser = new PamUser
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.Username,
            LastName = user.LastName,
            FirstName = user.FirstName,
            PasswordHash = user.PasswordHash,
            TwoFactorOptions = new LazyList<(TwoFactorType Type, string Secret)>(){IsLoaded = false},
            KnownIps = new LazyList<string>(){IsLoaded = false},
            Projects = new LazyList<IPamProject>(){IsLoaded = false},
            Flags = user.Flags,
            TTL = user.TTL
        };

        return pamUser;
    }

    /// <summary>
    /// Loads the user from the database
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static User LoadDbUser(this IPamUser user)
    {
        var userObj = PamaxieDatabaseService.UserSingleton.Get(user.Id);

        if (userObj is not User dbUser)
        {
            return null;
        }

        return dbUser;
    }
}