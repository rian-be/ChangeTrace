namespace ChangeTrace.CredentialTrace.Interfaces;

internal interface IValidatableAuthProvider : IAuthProvider
{
    Task<bool> ValidateTokenAsync(string token, CancellationToken ct = default);
}