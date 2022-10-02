using System.Reflection;
using AutoFixture.Kernel;

namespace Assignment.Infrastructure.Tests;

public class IdPropertyOmitter : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        return request switch
        {
            // Don't populate id properties, as the database will generate the ids, or they might be foreign keys.
            PropertyInfo propertyInfo when propertyInfo.Name.EndsWith("Id") => new OmitSpecimen(),
            // Same with parameters in constructors, but supply a default value, otherwise it will omit the constructor.
            ParameterInfo parameterInfo when parameterInfo.Name?.EndsWith("Id") ?? false => Activator.CreateInstance(
                parameterInfo.ParameterType)!,
            _ => new NoSpecimen()
        };
    }
}
