using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;

namespace Entities.Models
{
    public class ShapedEntityWrapper
    {
        public ShapedEntityWrapper()
        {
            EntityProperties = new ExpandoObject();
        }
        
        public Guid Id { get; set; }
        public ExpandoObject EntityProperties { get; set; }
    }
}