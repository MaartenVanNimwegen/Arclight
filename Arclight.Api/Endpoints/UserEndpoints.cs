using Arclight.Application.DTOs;
using Arclight.Application.Services;
using Arclight.Domain.Enums;

namespace Arclight.Api.Endpoints
{
    public static class UserEndpoints
    {
        public static void MapUserEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/user");

            group.MapPost("/register", CreateUser);
            group.MapGet("/{id:guid}", GetUser);
            group.MapPost("/login", Login);
        }

        static async Task<IResult> CreateUser(RegisterRequest request, UserRole role, IUserService service)
        {
            var id = await service.CreateUserAsync(request.email, request.firstName, request.lastName, request.password, role);
            return Results.Created($"/users/{id}", id);
        }

        static async Task<IResult> GetUser(Guid id, IUserService service)
        {
            var user = await service.GetUserAsync(id);
            return user is not null ? Results.Ok(user) : Results.NotFound();
        }

        static async Task<IResult> Login(LoginRequest request, IUserService service)
        {
            var token = await service.LoginAsync(request);

            if (token is null)
            {
                return Results.Unauthorized();
            }
            return Results.Ok(new { Token = token });
        }
    }
}
