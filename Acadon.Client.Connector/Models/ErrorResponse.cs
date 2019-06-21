using Newtonsoft.Json;

namespace Acadon.Client.Connector.Models
{
    public class ErrorResponse
    {
        public ErrorResponse()
        {

        }

        public ErrorResponse(string message)
        {
            Message = message;
        }

        public string ResponseType { get; set; } = "Error";
        public string Message { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
