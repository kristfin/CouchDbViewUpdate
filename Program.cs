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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace CouchDbViewUpdate
{   
    public class Program
    {
        private const string Version = "0.9";
        private const string Copyright = "Copyright (C) 2018 Kristján Þór Finnsson <kristfin@gmail.com>";
        private static Logger _logger;
        private static Options _options;
        public static void Main(string[] args)
        {
            _logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            _logger.Information("CouchDbViewUpdate " + Version);
            _logger.Information(Copyright);

            try
            {
                _options = new Options(config);
            }
            catch (Exception ex)
            {
                _logger.Error("Bad config " + ex.Message);
                _logger.Information(Options.Help());
                return;
            }

            _logger.Information("Using configuration\n" + _options);

            var allDbs = _options.Databases.Count == 0;

            try
            {
                if (_options.Daemon)
                {
                    while (true)
                    {
                        _logger.Information("Running in daemon mode.  Ctrl-C to stop");
                        Doit(allDbs);
                        _logger.Information("Will sleep for " + _options.DaemonDelaySec + " seconds");
                        Task.Delay(_options.DaemonDelaySec * 1000).Wait();
                    }
                }
                else
                {
                    Doit(allDbs);
                }
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "We have a problem");
            }
        }

        private static void Doit(bool allDbs)
        {
            if (allDbs)
            {
                _options.Databases = GetDatabases();
            }
            UpdateViews();            
        }      

        private static List<ViewInfo> GetViews(string database)
        {
            var viewList = new List<ViewInfo>();
            using (var wc = new WebClient())
            {
                wc.UseDefaultCredentials = true;
                wc.Credentials = new NetworkCredential(_options.User, _options.Password);
                _logger.Information("Getting views from " + database);
                try
                {
                    var url = _options.Server + "/" + database + "/_design/views";
                    _logger.Information("Opening " + url);
                    var jsonString = wc.DownloadString(url);
                    var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
                    var views = ((JObject)data["views"]).ToObject<Dictionary<string, object>>();
                    foreach (var view in views)
                    {
                        var name = view.Key;
                        var viewData = ((JObject)view.Value).ToObject<Dictionary<string, object>>();
                        var reduce = viewData.ContainsKey("reduce");
                        viewList.Add(new ViewInfo() { Name = name, Reduce = reduce });
                    }
                }
                catch(WebException wex)
                {
                    if (wex.Response is HttpWebResponse webResponse && webResponse.StatusCode == HttpStatusCode.NotFound)
                    {
                        // no views found
                        return null;
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Getting views");
                    return null;
                }
                return viewList;
            }
        }

        private static List<string> GetDatabases()
        {
            var dbList = new List<string>();
            using (var wc = new WebClient())
            {
                var url = _options.Server + "/_all_dbs";
                wc.UseDefaultCredentials = true;
                wc.Credentials = new NetworkCredential(_options.User, _options.Password);
                _logger.Information("Getting databases from " + _options.Server);
                try
                {                    
                    var jsonString = wc.DownloadString(url);
                    var dbs = JsonConvert.DeserializeObject<List<string>>(jsonString);
                    foreach (var x in dbs)
                    {
                        string db = (string)x;
                        if (!db.StartsWith("_"))
                        {
                            dbList.Add(db);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Getting databases from " + url);
                    throw;
                }
                return dbList;
            }
        }

        private static void UpdateViews()
        {
            _logger.Information("Will update views in databases: " + string.Join(", ", _options.Databases));
            foreach (var database in _options.Databases)
            {
                var views = GetViews(database);                
                if (views == null || views.Count == 0)
                {
                    _logger.Warning("No views in " + database + ", skipping");
                    continue;
                }           
                foreach (var viewInfo in views)
                {
                    UpdateView(database, viewInfo);
                }
            }
        }

        private static void UpdateView(string database, ViewInfo viewInfo)
        {
            using (var wc = new WebClient())
            {
                wc.UseDefaultCredentials = true;
                wc.Credentials = new NetworkCredential(_options.User, _options.Password);
                var viewUrl = _options.Server + "/" + database + "/_design/views/_view/" + viewInfo.Name + "?limit=1";
                if (viewInfo.Reduce)
                {
                    viewUrl += "&reduce=True&group=True";
                }
                _logger.Information("updating view " + database + "." + viewInfo.Name);
                try
                {                    
                    wc.DownloadString(new Uri(viewUrl));
                }
                catch(WebException wex)
                {
                    _logger.Information(wex.Message);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Updating view " + viewInfo.Name);
                }
            }
        }
    }
}

