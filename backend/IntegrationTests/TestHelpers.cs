using Domain.Users;
using Infrastructure.DataContext;
using Microsoft.AspNetCore.Identity;
using Moq;

public static class TestHelpers
{
    public static UserManager<User> MockUserManager(DomainDataContext context)
    {
        var store = new Mock<IUserStore<User>>();
        var userManager = new UserManager<User>(
            store.Object, null, null, null, null, null, null, null, null);

        return userManager;
    }

    public static RoleManager<IdentityRole> MockRoleManager()
    {
        var store = new Mock<IRoleStore<IdentityRole>>();
        return new RoleManager<IdentityRole>(
            store.Object, null, null, null, null);
    }
}
