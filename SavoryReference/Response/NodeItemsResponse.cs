using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SavoryReference.Response
{
    public class NodeItemsResponse
    {
        [JsonProperty("items")]
        public List<Item> ItemList { get; set; }
    }
}