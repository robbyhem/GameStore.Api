using GameStore.Api.Data;
using GameStore.Api.Dtos;
using GameStore.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace GameStore.Api.Endpoints
{
    public static class GamesEndpoints
    {
        const string GetGameEndpointName = "GetGame";

        //private static readonly List<GameSummaryDto> games = [
        //    new(1, "Street Fighter II", "Fighting", 19.99M, new DateOnly(1992, 7, 15)),
        //    new(2, "Final Fantasy VII Rebirth", "RPG", 69.99M, new DateOnly(2024, 2, 29)),
        //    new(3, "Astro Bot", "Platformer", 59.99M, new DateOnly(2024, 9, 6))
        //];

        public static void MapGamesEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/games");

            // GET /games
            group.MapGet("/", async (ApplicationDbContext context) => /*games*/ 
                await context.Games.Include(game => game.Genre)
                                    .Select(game => new GameSummaryDto(
                                        game.Id,
                                        game.Name,
                                        game.Genre!.Name,
                                        game.Price,
                                        game.ReleaseDate
                                    )).AsNoTracking().ToListAsync()
            );

            // GET /games/1
            group.MapGet("/{id}", async (int id, ApplicationDbContext context) =>
            {
                //var game = games.Find(game => game.Id == id);


                var game = await context.Games.FindAsync(id);

                return game is null ? Results.NotFound() : Results.Ok(
                    new GameDetailsDto(
                        game.Id,
                        game.Name,
                        game.GenreId,
                        game.Price,
                        game.ReleaseDate
                    )
                );
            }).WithName("GetGame");

            // POST /games
            group.MapPost("/", async (CreateGameDto newGame, ApplicationDbContext context) =>
            {
                //GameDto game = new(
                //    games.Count + 1,
                //    newGame.Name,
                //    newGame.Genre,
                //    newGame.Price,
                //    newGame.ReleaseDate
                //);

                //games.Add(game);

                //return Results.CreatedAtRoute("GetGame", new { id = game.Id }, game);


                Game game = new()
                {
                    Name = newGame.Name,
                    GenreId = newGame.GenreId,
                    Price = newGame.Price,
                    ReleaseDate = newGame.ReleaseDate
                };

                context.Games.Add(game); // This line doesn't send the game into the db, it adds EF Core to keep track of the changes (the addition)
                await context.SaveChangesAsync();

                GameDetailsDto gameDto = new(
                    game.Id,
                    game.Name,
                    game.GenreId,
                    game.Price,
                    game.ReleaseDate
                );

                return Results.CreatedAtRoute("GetGame", new { id = gameDto.Id }, gameDto);
            });

            // PUT /games/1
            group.MapPut("/{id}", async (int id, UpdateGameDto updatedGame, ApplicationDbContext context) =>
            {
                //var index = games.FindIndex(game => game.Id == id);

                //if (index == -1)
                //{
                //    return Results.NotFound();
                //}

                //games[index] = new GameSummaryDto(
                //    id,
                //    updatedGame.Name,
                //    updatedGame.Genre,
                //    updatedGame.Price,
                //    updatedGame.ReleaseDate
                //);

                //return Results.NoContent();


                var existingGame = await context.Games.FindAsync( id);

                if (existingGame is null)
                {
                    return Results.NotFound();
                }

                existingGame.Name = updatedGame.Name;
                existingGame.GenreId = updatedGame.GenreId;
                existingGame.Price = updatedGame.Price;
                existingGame.ReleaseDate = updatedGame.ReleaseDate;

                await context.SaveChangesAsync();

                return Results.NoContent();
            });

            // DELETE /games/1
            group.MapDelete("/{id}", async (int id, ApplicationDbContext context) =>
            {
                //games.RemoveAll(game => game.Id == id);

                //return Results.NoContent();


                await context.Games.Where(game => game.Id == id).ExecuteDeleteAsync();

                return Results.NoContent();
            });
        }
    }
}
