using System.IO.Abstractions;
using System.Reflection;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;

namespace Gitup
{
    [VersionOptionFromMember(MemberName = nameof(GetVersion))]
    [HelpOption]
    class Program
    {
        
        [Option(Description = "The path to scan from.", ShortName = "p", LongName = "path", ValueName = "PATH")]
        public string Path { get; set; }

        private readonly GitupService _service;
        
        public static int Main(string[] args)
        {
            var services = new ServiceCollection()
                .AddSingleton<IConsole, PhysicalConsole>()
                .AddSingleton<IFileSystem, FileSystem>()
                .AddTransient<GitupService>()
                .AddSingleton<IReporter>(provider => new ConsoleReporter(provider.GetService<IConsole>()))
                .BuildServiceProvider();

            var app = new CommandLineApplication<Program>()
            {
                ThrowOnUnexpectedArgument = false
            };

            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(services);

            return app.Execute(args);

        }

        public Program(GitupService service)
        {
            
            _service = service;
        }

        private static string GetVersion() => typeof(Program)
            .Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            .InformationalVersion;
        

        public async Task<int> OnExecute(CommandLineApplication app)
        {
            await _service.Execute(Path);
            return 0;
        }
        
    }
}