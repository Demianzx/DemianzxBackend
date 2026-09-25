using DemianzxBackend.Application.Common.Models;
using DemianzxBackend.Application.Games.Commands.CreateGame;
using DemianzxBackend.Application.Games.Commands.DeleteGame;
using DemianzxBackend.Application.Games.Commands.UpdateGame;
using DemianzxBackend.Application.Games.Queries.GetGames;
using Microsoft.AspNetCore.Http.HttpResults;

namespace DemianzxBackend.Web.Endpoints;

public class Games : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        // Public endpoints (no authorization required)
        app.MapGroup(this)
            .MapGet(GetGames);

        // Protected endpoints (require authorization)
        app.MapGroup(this)
            .RequireAuthorization()
            .MapPost(CreateGame)
            .MapPut(UpdateGame, "{id}")
            .MapDelete(DeleteGame, "{id}");
    }

    public async Task<Ok<PaginatedList<GameSimplifiedDto>>> GetGames(ISender sender, [AsParameters] GetGamesQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    public async Task<Created<int>> CreateGame(ISender sender, CreateGameCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/Games/{id}", id);
    }

    public async Task<Results<NoContent, BadRequest, NotFound>> UpdateGame(ISender sender, int id, UpdateGameCommand command)
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

    public async Task<Results<NoContent, NotFound>> DeleteGame(ISender sender, int id)
    {
        try
        {
            await sender.Send(new DeleteGameCommand(id));
        }
        catch (NotFoundException)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.NoContent();
    }
}