using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Persistence.ValueConverters;

internal sealed class AbstractAddressToStringConverter()
    : ValueConverter<AbstractAddress, string>(
        to => to.AddressToString(),
        from => AbstractAddressExt.AddressFromString(from));