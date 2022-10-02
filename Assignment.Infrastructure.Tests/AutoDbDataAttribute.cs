using AutoFixture;
using AutoFixture.Xunit2;

namespace Assignment.Infrastructure.Tests;

public class AutoDbDataAttribute : AutoDataAttribute
{
    public AutoDbDataAttribute() : base(CreateFixture)
    {
    }

    private static IFixture CreateFixture()
    {
        var fixture = new Fixture();
        fixture.Customizations.Add(new IdPropertyOmitter());

        return fixture;
    }
}
