using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Persistence.ValueConverters;

public sealed class TrackingCodeToStringConverter() : ValueConverter<TrackingCode, string>(
    v => v.ToString(),
    v => new TrackingCode(v));