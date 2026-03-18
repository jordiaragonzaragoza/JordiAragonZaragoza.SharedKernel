namespace JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Contracts.HttpClientHelpers
{
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;

    public static class StringContentHelpers
    {
        public static StringContent FromModelAsJson(object model)
        {
            return new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
        }
    }
}