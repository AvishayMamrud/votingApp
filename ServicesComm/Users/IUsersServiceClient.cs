public interface IUsersServiceClient
{
    Task<bool> AuthenticateAsync(string token);
    Task<UserDto> GetUserProfileAsync(Guid userId);
    Task RegisterUserAsync(RegisterUserRequest request);
}
