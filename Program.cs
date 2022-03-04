using System.Net;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json;
using posts;

namespace posts
{

    public class Post
    {
        public ObjectId _id { get; set; }

        public String Author { get; set; }

        public String Content { get; set; }

        public List<String> Attachments { get; set; }

        public List<String> Circles { get; set; }

        public DateTime Date { get; set; }


        [BsonConstructor]
        public Post(string author, string content, List<String> circles, List<String> attachments)
        {
            this.Author = author;
            this.Content = content;
            this.Attachments = attachments;
            this.Circles = circles;
            this.Date = DateTime.Now;
        }
    }

    class HttpServer
    {

        public static HttpListener? Listener;

        public static async Task HandleIncomingConnections(IConfigurationRoot config)
        {

            string connectionString = config.GetValue<String>("MongoDB");
            MongoClientSettings settings = MongoClientSettings.FromConnectionString(connectionString);
            MongoClient client = new MongoClient(settings);
            IMongoDatabase database = client.GetDatabase("UsersDB");
            BsonClassMap.RegisterClassMap<Post>();
            IMongoCollection<Post> collection = database.GetCollection<Post>("users");
            Console.WriteLine("Database connected");



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

                // TODO: Send response
                Response.Fail(resp, "404");
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
