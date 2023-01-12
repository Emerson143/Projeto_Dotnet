using System.Collections.Immutable;
using System.Reflection.Emit;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSqlServer<ApplicationDbContext>(builder.Configuration["Database:SqlServe"]);

var app = builder.Build();
var configuration = app.Configuration;
ProductRepository.Init(configuration);

app.MapPost("/products", (ProductRequest productRequest, ApplicationDbContext context ) => {

 var category = context.Categories.Where(c => c.Id == productRequest.CategoryId).First();
 var product = new Product {
       Code = productRequest.Code,
       Name = productRequest.Name,
       Description = productRequest.Description,
       Category = category
      };
 context.Products.Add(product);    

return Results.Created("/products" +product.Id, product.Id);

});

//api.app.com/user/{code}
app.MapGet("/products/{code}", ([FromRoute] string code ) =>{
       var product = ProductRepository.GetBy(code);
       if(product != null)
              return Results.Ok(product);

       return Results.NotFound();
});


app.MapPut("/products", (Product product) => {
       var productSaved = ProductRepository.GetBy(product.Code);
       productSaved.Name = product.Name;
       return Results.Ok();
});

app.MapDelete("/products/{code}", ([FromRoute] string code ) => {
       var productSaved = ProductRepository.GetBy(code);
       ProductRepository.Remove(productSaved);
       return Results.Ok();
});


app.MapGet("/configuration/database", (IConfiguration configuration) => {

       return Results.Ok($"{configuration["database:connection"]}/{configuration["database:port"]}");

});

app.Run();
