using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Persistence.ValueConverters;

public sealed class WebAddressToStringConverter() : ValueConverter<WebAddress, string>(
    v => v.ToString(),
    v => new WebAddress(v));