using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml.Linq;

namespace HackathonSabreBot.Tools
{
    public class ParseMan : DynamicObject
    {
        XElement element;

        public ParseMan(string filename)
        {
            element = XElement.Load(filename);
        }

        private ParseMan(XElement el)
        {
            element = el;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (element == null)
            {
                result = null;
                return false;
            }
            
            XElement sub = element.Element(binder.Name);
            
            if (sub == null)
            {
                result = null;
                return false;
            }
            else {
                result = new ParseMan(sub);
                return true;
            }
        }

        public override string ToString()
        {
            if (element != null)
            {
                return element.Value;
            }
            else {
                return string.Empty;
            }
        }

        public string this[string attr]
        {
            get
            {
                if (element == null)
                {
                    return string.Empty;
                }

                return element.Attribute(attr).Value;
            }
        }

        //public static void Parse(dynamic parent, XElement node)
        //{
        //    if (node.HasElements)
        //    {
        //        if (node.Elements(node.Elements().First().Name.LocalName).Count() > 1)
        //        {
        //            //list
        //            var item = new ExpandoObject();
        //            var list = new List<dynamic>();
        //            foreach (var element in node.Elements())
        //            {
        //                Parse(list, element);
        //            }

        //            AddProperty(item, node.Elements().First().Name.LocalName, list);
        //            AddProperty(parent, node.Name.ToString(), item);
        //        }
        //        else
        //        {
        //            var item = new ExpandoObject();

        //            foreach (var attribute in node.Attributes())
        //            {
        //                AddProperty(item, attribute.Name.ToString(), attribute.Value.Trim());
        //            }

        //            //element
        //            foreach (var element in node.Elements())
        //            {
        //                Parse(item, element);
        //            }

        //            AddProperty(parent, node.Name.ToString(), item);
        //        }
        //    }
        //    else
        //    {
        //        AddProperty(parent, node.Name.ToString(), node.Value.Trim());
        //    }
        //}

        //private static void AddProperty(dynamic parent, string name, object value)
        //{
        //    if (parent is List<dynamic>)
        //    {
        //        (parent as List<dynamic>).Add(value);
        //    }
        //    else
        //    {
        //        (parent as IDictionary<String, object>)[name] = value;
        //    }
        //}
    }
}