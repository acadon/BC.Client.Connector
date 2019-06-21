using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Acadon.Client.Connector.Models
{
    public class OperationResponse
    {
        public string ResponseType { get; set; } = "OK";
        public Variable[] Variables { get; set; }
        public Variable[] Files { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public void AddFile(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            var bytes = File.ReadAllBytes(filePath);
            var content = Convert.ToBase64String(bytes);
            if (Files == null)
                Files = new Variable[] { new Variable { Name = fileName, Value = content } };
            else
            {
                var list = new List<Variable>(Files);
                list.Add(new Variable { Name = fileName, Value = content });
                Files = list.ToArray();
            }
        }

        public void AddVariable(string name, object value)
        {
            var type = GetJsonType(value);
            var variable = new Variable { Name = name, Value = value, Type = type };

            if (Variables == null)
                Variables = new Variable[] { variable };
            else
            {
                var list = new List<Variable>(Variables);
                list.Add(variable);
                Variables = list.ToArray();
            }
        }

        string GetJsonType(object value)
        {
            if (value == null)
                return "null";

            switch (value.GetType().ToString().ToLower())
            {
                case "system.string": return "String";
                case "system.boolean": return "Boolean";
                case "system.int32":
                case "system.int64":
                    return "Integer";
                case "system.decimal":
                case "system.double":
                    return "Number";
                default:
                    return "undefined";
            }
        }
    }

    public class Variable
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public object Value { get; set; }
    }
}
