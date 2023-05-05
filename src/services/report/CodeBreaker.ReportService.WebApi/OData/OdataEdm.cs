using CodeBreaker.Data.ReportService.Models;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace CodeBreaker.ReportService.OData;

internal static class ODataEdm
{
    static ODataEdm()
    {
        var builder = new ODataConventionModelBuilder();
        builder.EnableLowerCamelCase();
        builder.EntitySet<Game>("games");
        Model = builder.GetEdmModel();
    }

    public static IEdmModel Model { get; private set; }
}
