using System;
using System.Threading.Tasks;
using BaGet;
using BaGet.Core;
using BaGet.Web;
using Humanizer.Configuration;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

var root = Environment.GetEnvironmentVariable("BAGET_CONFIG_ROOT");

if (!string.IsNullOrEmpty(root))
{
    builder.Configuration.SetBasePath(root);
}

// TODO: Ideally we'd use:
//
//       services.ConfigureOptions<ConfigureBaGetOptions>();
//
//       However, "ConfigureOptions" doesn't register validations as expected.
//       We'll instead register all these configurations manually.
// See: https://github.com/dotnet/runtime/issues/38491
builder.Services.AddTransient<IConfigureOptions<CorsOptions>, ConfigureBaGetOptions>();
builder.Services.AddTransient<IConfigureOptions<FormOptions>, ConfigureBaGetOptions>();
builder.Services.AddTransient<IConfigureOptions<ForwardedHeadersOptions>, ConfigureBaGetOptions>();
builder.Services.AddTransient<IConfigureOptions<IISServerOptions>, ConfigureBaGetOptions>();
builder.Services.AddTransient<IValidateOptions<BaGetOptions>, ConfigureBaGetOptions>();
builder.Services.AddBaGetOptions<IISServerOptions>(nameof(IISServerOptions));
builder.Services.AddBaGetWebApplication(option =>
{
    // Add database providers.
    //option.AddAzureTableDatabase();
    option.AddMySqlDatabase();
    option.AddPostgreSqlDatabase();
    option.AddSqliteDatabase();
    option.AddSqlServerDatabase();

    // Add storage providers.
    option.AddFileStorage();
    option.AddAliyunOssStorage();
    option.AddAwsS3Storage();
    //option.AddAzureBlobStorage();
    option.AddGoogleCloudStorage();

    // Add search providers.
    //option.AddAzureSearch();
});

// You can swap between implementations of subsystems like storage and search using BaGet's configuration.
// Each subsystem's implementation has a provider that reads the configuration to determine if it should be
// activated. BaGet will run through all its providers until it finds one that is active.
builder.Services.AddScoped(DependencyInjectionExtensions.GetServiceFromProviders<IContext>);
builder.Services.AddTransient(DependencyInjectionExtensions.GetServiceFromProviders<IStorageService>);
builder.Services.AddTransient(DependencyInjectionExtensions.GetServiceFromProviders<IPackageDatabase>);
builder.Services.AddTransient(DependencyInjectionExtensions.GetServiceFromProviders<ISearchService>);
builder.Services.AddTransient(DependencyInjectionExtensions.GetServiceFromProviders<ISearchIndexer>);

builder.Services.AddSingleton<IConfigureOptions<MvcRazorRuntimeCompilationOptions>, ConfigureRazorRuntimeCompilation>();

builder.Services.AddCors();


var app = builder.Build();

if (!app.ValidateStartupOptions())
{
    return;
}

var options = app.Configuration.Get<BaGetOptions>();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseStatusCodePages();
}

app.UseForwardedHeaders();
app.UsePathBase(options.PathBase);

app.UseStaticFiles();
app.UseRouting();

app.UseCors(ConfigureBaGetOptions.CorsPolicy);
app.UseOperationCancelledMiddleware();

app.MapBaGetEndpoint();


//var host = CreateHostBuilder(args).Build();
//if (!host.ValidateStartupOptions())
//{
//    return;
//}

var cmdApp = new CommandLineApplication
{
    Name = "baget",
    Description = "A light-weight NuGet service",
};

cmdApp.HelpOption(inherited: true);

cmdApp.Command("import", import =>
{
    import.Command("downloads", downloads =>
    {
        downloads.OnExecuteAsync(async cancellationToken =>
        {
            using (var scope = app.Services.CreateScope())
            {
                var importer = scope.ServiceProvider.GetRequiredService<DownloadsImporter>();

                await importer.ImportAsync(cancellationToken);
            }
        });
    });
});

cmdApp.Option("--urls", "The URLs that BaGet should bind to.", CommandOptionType.SingleValue);

cmdApp.OnExecuteAsync(async cancellationToken =>
{
    await app.RunMigrationsAsync(cancellationToken);
    await app.RunAsync(cancellationToken);
});

await cmdApp.ExecuteAsync(args);
//public static IHostBuilder CreateHostBuilder(string[] args)
//{
//    return Host
//        .CreateDefaultBuilder(args)
//        .ConfigureAppConfiguration((ctx, config) =>
//        {
//            var root = Environment.GetEnvironmentVariable("BAGET_CONFIG_ROOT");

//            if (!string.IsNullOrEmpty(root))
//            {
//                config.SetBasePath(root);
//            }
//        })
//        .ConfigureWebHostDefaults(web =>
//        {
//            web.ConfigureKestrel(options =>
//            {
//                // Remove the upload limit from Kestrel. If needed, an upload limit can
//                // be enforced by a reverse proxy server, like IIS.
//                options.Limits.MaxRequestBodySize = null;
//            });

//            web.UseStartup<Startup>();
//        });
//}
