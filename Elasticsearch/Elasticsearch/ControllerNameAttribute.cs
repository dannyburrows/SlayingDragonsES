using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Elasticsearch
{
    public class ControllerNameAttribute : Attribute
    {
        public string Name { get; set; }

        public ControllerNameAttribute( string name )
        {
            Name = name;
        }
    }
}