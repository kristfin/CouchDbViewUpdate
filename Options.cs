// Copyright 2018 Kristján Þór Finnsson <kristfin@gmail.com>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouchDbViewRefresher
{
    /// <summary>
    /// Class defining options for the CouchViewRefresher
    /// </summary>
    public class Options
    {
        /// <summary>
        /// CouchDb server url with port
        /// </summary>
        public string Server { get; }
        /// <summary>
        /// Optional CouchDb user
        /// </summary>
        public string User { get; } = null;
        /// <summary>
        /// Optional CouchDb password
        /// </summary>
        public string Password { get; } = null;
        /// <summary>
        /// Flag specifying if to run in daemon mode
        /// </summary>
        public bool Daemon { get; } = false;
        /// <summary>
        /// Delay between daemon invocations
        /// </summary>
        public int DaemonDelaySec { get; } = 60 * 10;
        /// <summary>
        /// Optional list of databases to update views in
        /// </summary>
        public List<string> Databases { get; set; } = new List<string>();

        /// <summary>
        /// Initializes the Options using Microsoft.Extensions.Configuration.IConfiguration root.
        /// </summary>
        /// <param name="config">The config</param>
        public Options(IConfigurationRoot config)
        {
            User = config["user"];
            Password = config["password"];
            if (User != null && Password == null)
            {
                throw new ArgumentNullException("password");
            }
            if (User == null && Password != null)
            {
                throw new ArgumentNullException("user");
            }
            Server = config["server"];
            if (Server == null)
            {
                throw new ArgumentNullException("server");
            }
            if (!Server.ToLower().StartsWith("http"))
            {
                throw new ArgumentException("server", "must be proper http(s) url");
            }
            if (config["daemon"] != null)
            {
                Daemon = Boolean.Parse(config["daemon"]);
            }
            if (config["daemonDelaySec"] != null)
            {
                DaemonDelaySec = int.Parse(config["daemonDelaySec"]);
            }
            if (DaemonDelaySec < 60 || DaemonDelaySec > 60 * 60)
            {
                throw new ArgumentOutOfRangeException("DaemonDelaySec", "must be >= 60 and < 3600");
            }
            if (config["databases"] != null)
            {
                var dbs = config["databases"];
                dbs = dbs.Replace(" ", "");
                Databases.AddRange(dbs.Split(','));
            }
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("\tserver=" + Server);
            sb.AppendLine("\tuser=" + (string.IsNullOrEmpty(User) ? "null" : User));
            sb.AppendLine("\tpassword=" + (string.IsNullOrEmpty(Password) ? "null" : new string('*', Password.Length)));
            sb.AppendLine("\tdaemon=" + Daemon);
            sb.AppendLine("\tdaemonDelaySec=" + DaemonDelaySec);
            sb.AppendLine("\tdatabases=[" + (Databases.Count > 0 ? string.Join(' ', Databases) : "")+"]");
            return sb.ToString();
        }

        public static string Help()
        {
            var sb = new StringBuilder();
            sb.AppendLine("\nusage: dotnet run server=server [options]");
            sb.AppendLine("where:");
            sb.AppendLine("\tserver=string                couchdb server url with port");
            sb.AppendLine("\tuser=string                  optional, couchdb user");
            sb.AppendLine("\tpassword=string              optional, couchdb password");
            sb.AppendLine("\tdaemon=bool                  optional, run in daemon mode");
            sb.AppendLine("\tdaemonDelaySec=int           optional, delay between runs in daemon mode, default 600");
            sb.AppendLine("\tdatabases=string,string,...  optional, list of databases to update, by default all databases will be updated");
            return sb.ToString();
        }
    }
}
