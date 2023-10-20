using Microsoft.AspNetCore.Mvc;
using WebApiDemo.Filter;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//È«¾Ö¹ýÂËÆ÷
builder.Services.Configure<MvcOptions>(option => {
    //option.Filters.Add(new ActionFilter());
    option.Filters.Add<ExceptionFilter>();

});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
