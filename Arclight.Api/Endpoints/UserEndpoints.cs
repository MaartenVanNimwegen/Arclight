using Arclight.Api.Filters;
using Arclight.Application.DTOs;
using Arclight.Application.Interfaces;
using Arclight.Domain.Entities;
using Arclight.Domain.Enums;

namespace Arclight.Api.Endpoints
{
    public static class UserEndpoints
    {
        private static readonly string ValidRoles = string.Join(", ", Enum.GetNames<UserRole>());

        public static IEndpointConventionBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/user");

            group.MapPost("/register", CreateUser)
                .AddEndpointFilter<ValidationFilter<RegisterRequest>>();

            group.MapGet("/{id:guid}", GetUser);

            group.MapPost("/login", Login)
                .AddEndpointFilter<ValidationFilter<LoginRequest>>();

            group.MapGet("/", GetAllUsers)
                .RequireAuthorization("RequireAdmin");

            group.MapPut("/{id:guid}/{role}", UpdateUser)
                .RequireAuthorization("RequireAdmin");

            group.MapDelete("/{id:guid}", DeleteUser)
                .RequireAuthorization("RequireAdmin");

            return group;
        }

        static async Task<IResult> CreateUser(RegisterRequest request, IUserService service)
        {
            try
            {
                Guid id = await service.CreateUserAsync(request.Email, request.FirstName, request.LastName, request.Password, UserRole.User);
                return Results.Created($"/user/{id}", id);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        }

        static async Task<IResult> GetUser(Guid id, IUserService service)
        {
            // This endpoint returns the user with the given Id, or a NotFound.
            User? user = await service.GetUserAsync(id);
            return user is not null ? Results.Ok(user) : Results.NotFound();
        }

        static async Task<IResult> Login(LoginRequest request, IUserService service)
        {
            // LoginAsync checks the credentials and returns a JWTToken if correct.
            string? token = await service.LoginAsync(request);

            // If token is null, the login was unsuccesfull
            if (token is null)
            {
                return Results.Unauthorized();
            }

            // Else the user is logged in and the token is send to the user
            return Results.Ok(new { Token = token });
        }

        static async Task<IResult> GetAllUsers(IUserService service)
        {
            IEnumerable<UserResponse> users = await service.GetAllUsersAsync();
            return Results.Ok(users);
        }

        static async Task<IResult> UpdateUser(Guid id, string role, IUserService service)
        {
            if (!Enum.TryParse<UserRole>(role, true, out UserRole parsedRole) || !Enum.IsDefined(parsedRole))
            {
                return Results.BadRequest(new { error = $"Invalid role. Valid roles are: {ValidRoles}." });
            }

            try
            {
                await service.UpdateUserRoleAsync(id, parsedRole);
                return Results.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound(new { error = "User not found." });
            }
        }

        static async Task<IResult> DeleteUser(Guid id, IUserService service)
        {
            try
            {
                await service.DeleteUserAsync(id);

                return Results.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound(new { error = "User not found." });
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        }
    }
}
