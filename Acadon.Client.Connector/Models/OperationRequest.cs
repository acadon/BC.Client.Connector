using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Acadon.Client.Connector.Models
{
    public class OperationRequest
    {
        public string OperationName { get; set; }
        public string CompanyName { get; set; }
        public string SerialNo { get; set; }
        public string ApiVersion { get; set; }
        public string Script { get; set; }
        public Parameter[] Parameters { get; set; }
        public FileParameter[] Files { get; set; }

        public static OperationRequest ParseFromString(string value)
        {
            return JsonConvert.DeserializeObject<OperationRequest>(value);
        }

        public string GetCheckSum()
        {
            var md5Hash = MD5.Create();
            var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(SerialNo + CompanyName + Script));
            var stringBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
                stringBuilder.Append(data[i].ToString("x2"));

            return stringBuilder.ToString();
        }

        public void SaveAllFiles(string path)
        {
            if (Files == null || Files.Length == 0)
                return;

            foreach (var item in Files)
            {
                var filePath = Path.Combine(path, item.Name);
                File.WriteAllBytes(filePath, Convert.FromBase64String(item.Value));
            }
        }
    }

    public class Parameter
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }

    public class FileParameter
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
