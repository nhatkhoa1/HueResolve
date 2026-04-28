using HueResolve.Models.DTOs;

namespace HueResolve.Business.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
}