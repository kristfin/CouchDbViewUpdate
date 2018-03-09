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

namespace CouchDbViewUpdate
{
    /// <summary>
    /// Simple class to store information about a single view
    /// </summary>
    public class ViewInfo
    {
        /// <summary>
        /// The name of the view
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Does it use reduce?
        /// </summary>
        public bool Reduce { get; set; }
    }
}
