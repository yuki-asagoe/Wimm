using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wimm.Common
{
    [AttributeUsage(AttributeTargets.Assembly,AllowMultiple =true)]
    public class BuiltInResourceAttribute:Attribute
    {
        public string ResourcePath { get; }
        public ResourceType Resource { get; }

        public BuiltInResourceAttribute(ResourceType type,string path)
        {
            Resource = type;
            ResourcePath = path;
        }
    }

    public enum ResourceType
    {
        Description,Icon, Reference, ScriptOnControl,ScriptControlMap,ScriptInitialize
    }
}
