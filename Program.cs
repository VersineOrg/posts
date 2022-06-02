using System.Net;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace posts;

class HttpServer
{
    private static HttpListener? listener;

    private static async Task HandleIncomingConnections(EasyMango.EasyMango database, EasyMango.EasyMango userDatabase)
    {
        while (true)
        {
            HttpListenerContext ctx = await listener?.GetContextAsync()!;

            HttpListenerRequest req = ctx.Request;
            HttpListenerResponse resp = ctx.Response;
                
            Console.WriteLine(req.HttpMethod);
            Console.WriteLine(req.Url?.ToString());
            Console.WriteLine(req.UserHostName);
            Console.WriteLine(req.UserAgent);
            
            if (req.HttpMethod == "POST" && req.Url?.AbsolutePath == "/addPost")
            {
                StreamReader reader = new StreamReader(req.InputStream);
                string bodyString = await reader.ReadToEndAsync();
                dynamic body = JsonConvert.DeserializeObject(bodyString)!;
                
                string token;
                string message;
                
                try
                {
                    token = ((string) body.token).Trim();
                    message = ((string) body.message).Trim();
                }
                catch
                {
                    token = "";
                    message = "";
                }

                if (!(String.IsNullOrEmpty(token) || String.IsNullOrEmpty(message)))
                {
                    string id = WebToken.GetIdFromToken(token);
                    if (!id.Equals(""))
                    {
                        Console.WriteLine(new ObjectId(id));
                        if (userDatabase.GetSingleDatabaseEntry("_id", new ObjectId(id), out BsonDocument user))
                        {
                            Post post = new Post(user.GetElement("_id").Value.AsObjectId,
                                message);
                            database.AddSingleDatabaseEntry(post.ToBson());
                            Response.Success(resp, "post created successfully", null);
                        }
                        else
                        {
                            Response.Fail(resp,"user doesn't exist");
                        }
                    }
                    else
                    {
                        Response.Fail(resp,"invalid token");
                    }
                }
                else
                {
                    Response.Fail(resp,"invalid body");
                }
            }
            else if (req.HttpMethod == "GET" && req.Url?.AbsolutePath == "/health")
            {
                Response.Success(resp,"service up","");
            }
            else
            {
                Response.Fail(resp, "404");
            }
            // close response
            resp.Close();
        }
    }

    public static void Main(string[] args)
    {
        IConfigurationRoot config =
            new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true)
                .AddEnvironmentVariables()
                .Build();
            
        string connectionString = config.GetValue<String>("connectionString");
        string postDatabaseName = config.GetValue<String>("postDatabaseName");
        string postCollectionName = config.GetValue<String>("postCollectionName");
        
        string userDatabaseName = config.GetValue<String>("userDatabaseName");
        string userCollectionName = config.GetValue<String>("userCollectionName");
        
        // Create a new EasyMango database
        EasyMango.EasyMango postDatabase = new EasyMango.EasyMango(connectionString,postDatabaseName,postCollectionName);
        EasyMango.EasyMango userDatabase = new EasyMango.EasyMango(connectionString,userDatabaseName,userCollectionName);
            
        // Create a Http server and start listening for incoming connections
        string url = "http://*:" + config.GetValue<String>("Port") + "/";
        listener = new HttpListener();
        listener.Prefixes.Add(url);
        listener.Start();
        Console.WriteLine("Listening for connections on {0}", url);

        // Handle requests
        Task listenTask = HandleIncomingConnections(postDatabase, userDatabase);
        listenTask.GetAwaiter().GetResult();
        
        // Close the listener
        listener.Close();
    }
}
