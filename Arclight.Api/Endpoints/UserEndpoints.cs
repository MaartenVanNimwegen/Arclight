﻿using Arclight.Api.Filters;
using Arclight.Application.DTOs;
using Arclight.Application.Interfaces;
using Arclight.Domain.Entities;
using Arclight.Domain.Enums;
using Arclight.Domain.Exceptions;

namespace Arclight.Api.Endpoints
{
    public static class UserEndpoints
    {
        public static void MapUserEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/user");

            group.MapPost("/register", CreateUser)
                .AddEndpointFilter<ValidationFilter<RegisterRequest>>();

            group.MapGet("/{id:guid}", GetUser);

            group.MapPost("/login", Login)
                .AddEndpointFilter<ValidationFilter<LoginRequest>>();
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
            User? user = await service.GetUserAsync(id);
            return user is not null ? Results.Ok(user) : throw new NotFoundException("User not found.");
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
