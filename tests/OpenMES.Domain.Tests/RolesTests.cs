using OpenMES.Domain;

namespace OpenMES.Domain.Tests;

public class RolesTests
{
    [Fact]
    public void Constants_match_All_collection()
    {
        Assert.Contains(Roles.Operator, Roles.All);
        Assert.Contains(Roles.Technician, Roles.All);
        Assert.Contains(Roles.Quality, Roles.All);
        Assert.Contains(Roles.Supervisor, Roles.All);
        Assert.Contains(Roles.Admin, Roles.All);
        Assert.Equal(5, Roles.All.Count);
    }
}
