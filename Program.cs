using System.Net;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace posts;

class HttpServer
{
        
    public static HttpListener? Listener;

    public static async Task HandleIncomingConnections(EasyMango.EasyMango database, EasyMango.EasyMango userDatabase)
    {
        while (true)
        {
            HttpListenerContext ctx = await Listener?.GetContextAsync()!;

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
                            Post post = new Post(user.GetElement("_id").Value.AsObjectId.ToString(),
                                message);
                            database.AddSingleDatabaseEntry(post.ToBson());
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
        string databaseNAme = config.GetValue<String>("databaseName");
        string collectionName = config.GetValue<String>("collectionName");
        
        string databaseNAme1 = config.GetValue<String>("databaseName1");
        string collectionName1 = config.GetValue<String>("collectionName1");
        
        // Create a new EasyMango database
        EasyMango.EasyMango database = new EasyMango.EasyMango(connectionString,databaseNAme,collectionName);
        EasyMango.EasyMango userdb = new EasyMango.EasyMango(connectionString,databaseNAme1,collectionName1);
            
        // Create a Http server and start listening for incoming connections
        string url = "http://*:" + config.GetValue<String>("Port") + "/";
        Listener = new HttpListener();
        Listener.Prefixes.Add(url);
        Listener.Start();
        Console.WriteLine("Listening for connections on {0}", url);

        // Handle requests
        Task listenTask = HandleIncomingConnections(database, userdb);
        listenTask.GetAwaiter().GetResult();
        
        // Close the listener
        Listener.Close();
    }
}
