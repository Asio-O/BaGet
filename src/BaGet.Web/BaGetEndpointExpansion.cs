using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BaGet.Web
{
    public static class BaGetEndpointExpansion
    {
        public static WebApplication MapBaGetEndpoint(this WebApplication app)
        {
            app.MapRazorPages();

            MapServiceIndexRoutes(app);
            MapPackagePublishRoutes(app);
            MapSymbolRoutes(app);
            MapSearchRoutes(app);
            MapPackageMetadataRoutes(app);
            MapPackageContentRoutes(app);

            return app;
        }

        public static void MapServiceIndexRoutes(WebApplication app)
        {
            app.MapControllerRoute(
                name: Routes.IndexRouteName,
                pattern: "v3/index.json",
                defaults: new { controller = "ServiceIndex", action = "Get" });
        }

        public static void MapPackagePublishRoutes(WebApplication app)
        {
            app.MapControllerRoute(
                name: Routes.UploadPackageRouteName,
                pattern: "api/v2/package",
                defaults: new { controller = "PackagePublish", action = "Upload" },
                constraints: new { httpMethod = new HttpMethodRouteConstraint("PUT") });

            app.MapControllerRoute(
                name: Routes.DeleteRouteName,
                pattern: "api/v2/package/{id}/{version}",
                defaults: new { controller = "PackagePublish", action = "Delete" },
                constraints: new { httpMethod = new HttpMethodRouteConstraint("DELETE") });

            app.MapControllerRoute(
                name: Routes.RelistRouteName,
                pattern: "api/v2/package/{id}/{version}",
                defaults: new { controller = "PackagePublish", action = "Relist" },
                constraints: new { httpMethod = new HttpMethodRouteConstraint("POST") });
        }

        public static void MapSymbolRoutes(WebApplication app)
        {
            app.MapControllerRoute(
                name: Routes.UploadSymbolRouteName,
                pattern: "api/v2/symbol",
                defaults: new { controller = "Symbol", action = "Upload" },
                constraints: new { httpMethod = new HttpMethodRouteConstraint("PUT") });

            app.MapControllerRoute(
                name: Routes.SymbolDownloadRouteName,
                pattern: "api/download/symbols/{file}/{key}/{file2}",
                defaults: new { controller = "Symbol", action = "Get" });

            app.MapControllerRoute(
                name: Routes.PrefixedSymbolDownloadRouteName,
                pattern: "api/download/symbols/{prefix}/{file}/{key}/{file2}",
                defaults: new { controller = "Symbol", action = "Get" });
        }

        public static void MapSearchRoutes(WebApplication app)
        {
            app.MapControllerRoute(
                name: Routes.SearchRouteName,
                pattern: "v3/search",
                defaults: new { controller = "Search", action = "Search" });

            app.MapControllerRoute(
                name: Routes.AutocompleteRouteName,
                pattern: "v3/autocomplete",
                defaults: new { controller = "Search", action = "Autocomplete" });

            // This is an unofficial API to find packages that depend on a given package.
            app.MapControllerRoute(
                name: Routes.DependentsRouteName,
                pattern: "v3/dependents",
                defaults: new { controller = "Search", action = "Dependents" });
        }

        public static void MapPackageMetadataRoutes(WebApplication app)
        {
            app.MapControllerRoute(
               name: Routes.RegistrationIndexRouteName,
               pattern: "v3/registration/{id}/index.json",
               defaults: new { controller = "PackageMetadata", action = "RegistrationIndex" });

            app.MapControllerRoute(
                name: Routes.RegistrationLeafRouteName,
                pattern: "v3/registration/{id}/{version}.json",
                defaults: new { controller = "PackageMetadata", action = "RegistrationLeaf" });
        }

        public static void MapPackageContentRoutes(WebApplication app)
        {
            app.MapControllerRoute(
                name: Routes.PackageVersionsRouteName,
                pattern: "v3/package/{id}/index.json",
                defaults: new { controller = "PackageContent", action = "GetPackageVersions" });

            app.MapControllerRoute(
                name: Routes.PackageDownloadRouteName,
                pattern: "v3/package/{id}/{version}/{idVersion}.nupkg",
                defaults: new { controller = "PackageContent", action = "DownloadPackage" });

            app.MapControllerRoute(
                name: Routes.PackageDownloadManifestRouteName,
                pattern: "v3/package/{id}/{version}/{id2}.nuspec",
                defaults: new { controller = "PackageContent", action = "DownloadNuspec" });

            app.MapControllerRoute(
                name: Routes.PackageDownloadReadmeRouteName,
                pattern: "v3/package/{id}/{version}/readme",
                defaults: new { controller = "PackageContent", action = "DownloadReadme" });

            app.MapControllerRoute(
                name: Routes.PackageDownloadIconRouteName,
                pattern: "v3/package/{id}/{version}/icon",
                defaults: new { controller = "PackageContent", action = "DownloadIcon" });
        }
    }
}
