using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vele.Scrappy.ConsoleApp.Options
{
    public class ConsoleOptions
    {
        [Option('o', "output-folder", Required = true, HelpText = "Set output folder to put the website in.")]
        public string OutputFolder { get; set; }

        [Option('u', "website-url", Required = true, HelpText = "Set URL from which to scrape the website from.")]
        public string WebsiteUrl { get; set; }

        [Option('e', "download-external", Required = false, Default = false, HelpText = "Determine whether to download external files or not.")]
        public bool DownloadExternal { get; set; }

        [Option('p', "parallel-execution", Required = false, Default = 4, HelpText = "Determine the amount of parallel processing tasks.")]
        public int ParallelExecutionCount { get; set; }
    }
}
