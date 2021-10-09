using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieDatabaseCLI
{
    [Verb("orphan-files", HelpText = "Find files without an entry at db.")]
    public class OrphanFilesOptions
    {
        [Option('p', "path", Required = true, HelpText = "Path where to search for files.")]
        public string Path { get; set; }

        [Option('f', "filter", Default = "*", HelpText = "Filter for files")]
        public string Filter { get; set; }

        [Option('o', "output", Required = true, HelpText = "Filepath and name for the output file.")]
        public string Output { get; set; }
    }

    [Verb("check-files-exists", HelpText = "Ensure that all files set at db exists at file storage.")]
    public class CheckFilesExistsOptions
    {
        [Option('m', "missing-output", Required = true, HelpText = "Filepath and name for the output file for missing files.")]
        public string MissingFilesOutput { get; set; }

        [Option('e', "existing-output", Required = true, HelpText = "Filepath and name for the output file for existing files.")]
        public string ExistingFilesOutput { get; set; }

        [Option('c', "clear", HelpText = "Clear db filename field for entries where file does not exists. Be carefull with this. Do a run without this option before to check the results.")]
        public bool Clear { get; set; }
    }

    [Verb("find-matches", HelpText = "Try to find matches for db entries without a filename set.")]
    public class FindMatchesOptions
    {

        [Option('p', "path", Required = true, HelpText = "Path where to search for files.")]
        public string Path { get; set; }

        [Option('f', "filter", Default = "*", HelpText = "Filter for files")]
        public string Filter { get; set; }

        [Option('o', "output", Required = true, HelpText = "Filepath and name for the output file.")]
        public string Output { get; set; }
        
        [Option('u', "update", HelpText = "Update db filename field for entries where a unique file match has been found.")]
        public bool Update { get; set; }

    }
}


