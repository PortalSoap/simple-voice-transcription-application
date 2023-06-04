using System;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace voice_recognition_application
{
    internal class Program
    {
        static string API_Key = "0xxx0x0x0xxx0xx00x00x0xx000xxxx0";

        static async Task Main(string[] args)
        {
            Console.WriteLine("Por favor, selecione 1. para enviar o arquivo.");
            Console.WriteLine("Por favor, selecione 2. para verificar se o processo terminou.");

            if(Console.ReadLine() == "1")
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("https://api.assemblyai.com/v2/");
                client.DefaultRequestHeaders.Add("authorization", API_Key);

                string jsonResult = SendFile(client, @"C:\Users\lucas\Documents\AudioSample\audio-sample.m4a").Result;
                Console.WriteLine(jsonResult);

                client.Dispose();

                client = new HttpClient();
                client.BaseAddress = new Uri("https://api.assemblyai.com/v2/");
                client.DefaultRequestHeaders.Add("authorization", API_Key);

                var json = new { audio_url = JsonConvert.DeserializeObject<UploadItem>(jsonResult).upload_url, language_code = "pt"};

                StringContent payload = new StringContent(JsonConvert.SerializeObject(json), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("https://api.assemblyai.com/v2/transcript", payload);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseJson);
            }
            else
            {
                Console.WriteLine("Por favor, insira o ID");
                string ticketID = Console.ReadLine();

                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("Authorization", API_Key);
                    httpClient.DefaultRequestHeaders.Add("Accepts", "application/json");
                    
                    HttpResponseMessage response = await httpClient.GetAsync("https://api.assemblyai.com/v2/transcript/" + ticketID);
                    response.EnsureSuccessStatusCode();

                    var responseJson = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseJson);
                }
            }
        }

        private static async Task<string> SendFile(HttpClient client, string filePath)
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "upload");
                request.Headers.Add("Transfer-Encoding", "chunked");

                var fileReader = File.OpenRead(filePath);
                var streamContent = new StreamContent(fileReader);

                request.Content = streamContent;
                HttpResponseMessage response = await client.SendAsync(request);

                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                throw;
            }
        }
    }
}