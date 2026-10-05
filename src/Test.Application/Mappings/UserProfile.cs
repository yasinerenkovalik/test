using Test.Application.Features.Users.DTOs;

namespace Test.Application.Mappings;

public sealed class UserProfile : Profile
{
    public UserProfile() => CreateMap<User, UserDto>();
}
