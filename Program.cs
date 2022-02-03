using System.Collections;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace door
{
    
    // Default Schema for a Http Response
    public class Response
    {
        public String success { get; set; }
        public String message { get; set; }
    }
    
    
    class HttpServer
    {
        
        public static HttpListener? Listener;

        public static async Task HandleIncomingConnections(IConfigurationRoot config)
        { 
            while (true)
            {
                // Will wait here until we hear from a connection
                HttpListenerContext ctx = await Listener?.GetContextAsync()!;

                // Peel out the requests and response objects
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                // Print out some info about the request
                Console.WriteLine(req.HttpMethod);
                Console.WriteLine(req.Url?.ToString());
                Console.WriteLine(req.UserHostName);
                Console.WriteLine(req.UserAgent);
                bool condition = false;
                if (condition)
                {
                    
                }
                else
                {
                    Response response = new Response
                    {
                        success = "false",
                        message = "404"
                    };
                    
                    string jsonString = JsonConvert.SerializeObject(response);
                    byte[] data = Encoding.UTF8.GetBytes(jsonString);
                    
                    resp.ContentType = "application/json";
                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentLength64 = data.LongLength;
                    
                    // Write out to the response stream (asynchronously), then close it
                    await resp.OutputStream.WriteAsync(data, 0, data.Length);
                    resp.Close();    
                }
            }
        }


        public static void Main(string[] args)
        {
            // Build the configuration for the env variables
            IConfigurationRoot config =
                new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", true)
                    .AddEnvironmentVariables()
                    .Build();
            
            // Create a Http server and start listening for incoming connections
            string url = "http://*:" + config.GetValue<String>("Port") + "/";
            Listener = new HttpListener();
            Listener.Prefixes.Add(url);
            Listener.Start();
            Console.WriteLine("Listening for connections on {0}", url);

            // Handle requests
            Task listenTask = HandleIncomingConnections(config);
            listenTask.GetAwaiter().GetResult();

            // Close the listener
            Listener.Close();
        }
    }
}
