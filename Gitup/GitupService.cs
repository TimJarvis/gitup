using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using LibGit2Sharp;
using McMaster.Extensions.CommandLineUtils;
using RunProcessAsTask;

namespace Gitup
{    
    public class GitupService
    {
        private readonly IFileSystem _fileSystem;
        private readonly IConsole _console;

        public GitupService(IFileSystem fileSystem, IConsole console)
        {
            _fileSystem = fileSystem;
            _console = console;
        }

        public async Task Execute(string path)
        {
            var rootPath = string.IsNullOrEmpty(path) ? _fileSystem.Directory.GetCurrentDirectory() : path;
            foreach (var p in _fileSystem.Directory.GetDirectories(rootPath, ".git", SearchOption.AllDirectories)
                .OrderBy(p => p))
            {
                var repository = new Repository(p);
                var dir = $"{repository.Info.WorkingDirectory.TrimEnd(new[] {'\\', '/'})}";
                _console.WriteLine(dir);
                _fileSystem.Directory.SetCurrentDirectory(dir);

                var fetchResults = await ProcessEx.RunAsync("git", "fetch --prune");
                LogResults(fetchResults);
                var mergeResults = await ProcessEx.RunAsync("git", "merge --ff-only @{u}");
                LogResults(mergeResults);
            }
            _fileSystem.Directory.SetCurrentDirectory(rootPath);
        }

        private void LogResults(ProcessResults results)
        {
            if (results.ExitCode == 0)
            {
                foreach (var line in results.StandardOutput)
                {
                    _console.WriteLine(line);
                }
            }
            else
            {
                foreach (var line in results.StandardError)
                {
                    _console.WriteLine(line);
                }
            }
                
        }
        
    }
}