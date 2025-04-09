using DemianzxBackend.Application.Categories.Commands.CreateCategory;
using DemianzxBackend.Application.Categories.Commands.DeleteCategory;
using DemianzxBackend.Application.Categories.Commands.UpdateCategory;
using DemianzxBackend.Application.Categories.Queries;
using DemianzxBackend.Application.Categories.Queries.GetCategories;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DemianzxBackend.Web.Endpoints;

public class Categories : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetCategories)
            .MapPost(CreateCategory)
            .MapPut(UpdateCategory, "{id}")
            .MapDelete(DeleteCategory, "{id}");
    }

    public async Task<Ok<List<CategoryDto>>> GetCategories(ISender sender)
    {
        var result = await sender.Send(new GetCategoriesQuery());
        return TypedResults.Ok(result);
    }

    public async Task<Created<int>> CreateCategory(ISender sender, CreateCategoryCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/Categories/{id}", id);
    }

    public async Task<Results<NoContent, BadRequest, NotFound>> UpdateCategory(ISender sender, int id, UpdateCategoryCommand command)
    {
        if (id != command.Id)
            return TypedResults.BadRequest();

        try
        {
            await sender.Send(command);
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.NoContent();
    }

    public async Task<Results<NoContent, NotFound>> DeleteCategory(ISender sender, int id)
    {
        try
        {
            await sender.Send(new DeleteCategoryCommand(id));
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.NoContent();
    }
}
