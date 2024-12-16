using Domain.Aggregates;

namespace Application.Services;

public interface IUserVerificationCodeGenerator
{
    public string GenerateCode(User user, string purpose);

    public bool ValidateCode(User user, string code);
}