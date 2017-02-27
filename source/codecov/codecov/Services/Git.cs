﻿using System.Text.RegularExpressions;
using codecov.Program;
using codecov.Services.Utils;

namespace codecov.Services
{
    internal class Git : Service
    {
        public Git()
        {
            QueryParameters["branch"] = Branch;
            QueryParameters["commit"] = Commit;
            QueryParameters["slug"] = Slug;
        }

        public override bool Detect
        {
            get
            {
                var isGit = !string.IsNullOrWhiteSpace(RunGit("--version"));
                if (!isGit)
                {
                    return false;
                }

                Log.Information("No CI provider detected, using Git.");
                return true;
            }
        }

        private static string Branch
        {
            get
            {
                var branch = RunGit(@"rev-parse --abbrev-ref HEAD");
                if (branch != null && branch.Equals("HEAD"))
                {
                    return string.Empty;
                }

                return !string.IsNullOrWhiteSpace(branch) ? branch : string.Empty;
            }
        }

        private static string Commit
        {
            get
            {
                var commit = RunGit(@"rev-parse HEAD");
                return !string.IsNullOrWhiteSpace(commit) ? commit : string.Empty;
            }
        }

        private static string Slug
        {
            get
            {
                var remote = RunGit("config --get remote.origin.url");
                var regex = new Regex(@"\b/.*/.*$");
                var match = regex.Match(remote);
                if (match.Success)
                {
                    var m = match.Value;
                    m = m.TrimStart('/');
                    m = m.TrimEnd('t');
                    m = m.TrimEnd('i');
                    m = m.TrimEnd('g');
                    m = m.TrimEnd('.');
                    return System.Net.WebUtility.UrlEncode(m);
                }

                return string.Empty;
            }
        }

        private static string RunGit(string commandArguments)
        {
            return Cmd.Run("git", commandArguments);
        }
    }
}