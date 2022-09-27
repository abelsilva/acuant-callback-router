var builder = WebApplication.CreateBuilder(args);
AcuantCallbackRouter.AppSetup.SetupConfiguration(builder.Environment, builder.Configuration);
AcuantCallbackRouter.AppSetup.SetupLogging(builder.Configuration, builder.Logging);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();