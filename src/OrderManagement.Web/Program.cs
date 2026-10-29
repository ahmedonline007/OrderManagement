using OrderManagement.Application;
using OrderManagement.Domain;
using OrderManagement.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddMediatR(configuration =>
{
    configuration.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly);
    configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
});
builder.Services.AddSingleton<OrderFactory>();
builder.Services.AddSingleton<IDiscountStrategy, PercentageDiscountStrategy>();
builder.Services.AddSingleton<IDiscountStrategy, FixedDiscountStrategy>();
builder.Services.AddSingleton<DiscountStrategyResolver>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();

public partial class Program { }
