using Arclight.Api.Filters;
using Arclight.Application.DTOs;
using Arclight.Application.Interfaces;

namespace Arclight.Api.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/categories");

        group.MapGet("/", GetAllCategories);
        group.MapPost("/", CreateCategory)
            .RequireAuthorization("RequireAdmin")
            .AddEndpointFilter<ValidationFilter<CreateCategoryRequest>>();

        group.MapDelete("/{id:guid}", DeleteCategory)
            .RequireAuthorization("RequireAdmin");

        group.MapPut("/{id:guid}", UpdateCategory)
            .RequireAuthorization("RequireAdmin")
            .AddEndpointFilter<ValidationFilter<UpdateCategoryRequest>>();
    }

    static async Task<IResult> GetAllCategories(ICategoryService service)
    {
        return Results.Ok(await service.GetAllCategoriesAsync());
    }

    static async Task<IResult> CreateCategory(CreateCategoryRequest request, ICategoryService service)
    {
        var id = await service.CreateCategoryAsync(request);
        return Results.Created($"/categories/{id}", id);
    }

    static async Task<IResult> DeleteCategory(Guid id, ICategoryService service)
    {
        try
        {
            var success = await service.DeleteCategoryAsync(id);
            return success ? Results.NoContent() : Results.NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Results.Conflict(new { error = ex.Message });
        }
    }

    static async Task<IResult> UpdateCategory(Guid id, UpdateCategoryRequest request, ICategoryService service)
    {
        var success = await service.UpdateCategoryAsync(id, request);
        return success ? Results.NoContent() : Results.NotFound();
    }
}