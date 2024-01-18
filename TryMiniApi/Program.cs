using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TryMiniApi;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// הוספת שירותי DbContext
builder.Services.AddDbContext<MyDbContext>( 
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors(builder=>{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
});

// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }

app.MapGet("/", () => {
    
    return "Hello!";});

app.MapGet("/item", async ([FromServices] MyDbContext dbContext) =>
{
    // שליפת כל המשימות 
    
    var item = await dbContext.Items.ToListAsync();
    return Results.Ok(item);
});

app.MapPost("/item", async ([FromServices] MyDbContext dbContext, [FromBody] Item item) =>
{
    // הוספת משימה חדשה
    
    await dbContext.Items.AddAsync(item);
    await dbContext.SaveChangesAsync();
    return Results.Created($"/tasks/{item.Id}", item);
});

app.MapPut("/item/{itemId}", async ([FromServices] MyDbContext dbContext,[FromRoute] int itemId,  [FromBody] Item updatedItem) =>
{
    // עדכון משימה
    var existingItem = await dbContext.Items.FindAsync(itemId);
    if (existingItem == null)
    {
        return Results.NotFound($"Task with ID {itemId} not found");
    }
    existingItem.IsComplete = updatedItem.IsComplete;
    await dbContext.SaveChangesAsync();
    return Results.Ok(existingItem);
});

app.MapDelete("/item/{itemId}", async ([FromServices] MyDbContext dbContext, int itemId) =>
{
    // מחיקת משימה
    var itemToDelete = await dbContext.Items.FindAsync(itemId);
    if (itemToDelete == null)
    {
        return Results.NotFound($"Task with ID {itemId} not found");
    }
    dbContext.Items.Remove(itemToDelete);
    await dbContext.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();