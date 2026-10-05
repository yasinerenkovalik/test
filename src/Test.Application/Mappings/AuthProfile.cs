using Test.Application.Features.Auth.DTOs;

namespace Test.Application.Mappings;

public sealed class AuthProfile : Profile
{
    public AuthProfile() => CreateMap<User, AuthUserDto>();
}
