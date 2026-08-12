using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
namespace DotNetProject.WindowForm.Services

{
    internal class HttpClientServices
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HttpClientServices(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<T> ExectueAsync<T>(string url, object obj, EnumHttpMethod method)
        {
            HttpResponseMessage? responseMessage = null;
            HttpContent? content = null;
            if (obj != null)
            {
                string jsonString = JsonConvert.SerializeObject(obj);
                content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            }
            var client = _httpClientFactory.CreateClient();
            switch (method)
            {
                case EnumHttpMethod.None:
                    break;
                case EnumHttpMethod.Get:
                    responseMessage = await client.GetAsync(url);
                    break;
                case EnumHttpMethod.Post:
                    responseMessage = await client.PostAsync(url, content);
                    break;
                case EnumHttpMethod.Put:
                    responseMessage = await client.PutAsync(url, content);
                    break;
                case EnumHttpMethod.Patch:
                    responseMessage = await client.PatchAsync(url, content);
                    break;
                case EnumHttpMethod.Delete:
                    responseMessage = await client.DeleteAsync(url);
                    break;
                default:
                    throw new Exception("Invalid Http Method");
            }
            if (responseMessage.IsSuccessStatusCode)
            {
                var jsonStr = await responseMessage.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(jsonStr);
            }
            throw new Exception("Error occured while executin the Http request.");
        }
    }
    public enum EnumHttpMethod
    {
        None,
        Get,
        Post,
        Put,
        Patch,
        Delete
    }
}
