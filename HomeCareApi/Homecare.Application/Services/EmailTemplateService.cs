namespace Homecare.Application.Services;

public class EmailTemplateService
{
    public string GetTemplate(string templateName, Dictionary<string, string> values)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Templates", templateName);
        var html = File.ReadAllText(path);

        foreach (var item in values)
            html = html.Replace($"{{{{{item.Key}}}}}", item.Value);

        return html;
    }
}