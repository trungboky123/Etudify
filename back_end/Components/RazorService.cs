using System.Reflection;
using RazorLight;

namespace back_end.Components;

public class RazorService
{
    private readonly RazorLightEngine _engine;

    public RazorService()
    {
        var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates");

        _engine = new RazorLightEngineBuilder()
            .UseFileSystemProject(templatePath)
            .UseMemoryCachingProvider()
            .SetOperatingAssembly(Assembly.GetExecutingAssembly())
            .Build();
    }

    public async Task<string> RenderAsync<T>(string templatePath, T model)
    {
        return await _engine.CompileRenderAsync(templatePath, model);
    }
}