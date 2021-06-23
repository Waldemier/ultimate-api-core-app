using System.Collections.Generic;
using System.Dynamic;

namespace Entities.LinkModels
{
    public class LinkResponse
    {
        public bool HasLinks { get; set; } // by default is false
        public List<ExpandoObject> ShapedEntities { get; set; }
        public LinkCollectionWrapper<ExpandoObject> LinkedEntities { get; set; }

        public LinkResponse()
        {
            LinkedEntities = new LinkCollectionWrapper<ExpandoObject>();
            ShapedEntities = new List<ExpandoObject>();
        }
    }
}