using Arclight.Application.DTOs;
using Arclight.Application.Interfaces;
using Arclight.Domain.Entities;
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

        static async Task<IResult> CreateUser(RegisterRequest request, IUserService service)
        {
            try
            { 
                Guid id = await service.CreateUserAsync(request.email, request.firstName, request.lastName, request.password, UserRole.User);

                // Happy flow: user is successfully created. This returns a 201 Created response.
                return Results.Created($"/user/{id}", id);
            }
            catch (InvalidOperationException ex)
            {
                // This exception is thrown in UserService when the email is already in use.
                return Results.Conflict(new { error = ex.Message });
            }
            catch (Exception)
            {
                return Results.Problem("There was an internal server error. Try again later.");
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
    }
}
