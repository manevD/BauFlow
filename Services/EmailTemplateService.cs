namespace BauFlow.Services
{
    public class EmailTemplateService
    {    private readonly IWebHostEnvironment _env;

        public EmailTemplateService(IWebHostEnvironment env)
        {
            _env = env;
        }
        public string LoadTemplate(string templateName)
        {
            var path = Path.Combine(
                _env.WebRootPath,
                "Templates",
                "Email",
                templateName
            );
            return File.ReadAllText(path);
        }

        public string ReplacePlaceholders(string template, Dictionary<string, string> data)
        {
            foreach (var item in data)
            {
                template = template.Replace($"{{{{{item.Key}}}}}", item.Value);
            }

            return template;
        }
    }
}
