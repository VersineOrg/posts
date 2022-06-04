using System.Net;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace posts;

class HttpServer
{
    private static HttpListener? listener;

    private static async Task HandleIncomingConnections(EasyMango.EasyMango userDatabase, EasyMango.EasyMango postDatabase)
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
                string media;
                List<BsonValue> circles = new List<BsonValue>();
                uint date = (uint) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                
                
                try
                {
                    token = ((string) body.token).Trim();
                    message = ((string) body.message).Trim();
                    foreach (var circle in body.circles)
                    {
                        circles.Add(circle);
                    }
                }
                catch
                {
                    token = "";
                    message = "";
                    
                }
                try
                {
                    media = ((string) body.media).Trim();
                }
                catch
                {
                    media = "";
                }
                
                
                if (!(String.IsNullOrEmpty(token) || String.IsNullOrEmpty(message))) 
                {
                    string id = WebToken.GetIdFromToken(token);
                    if (!id.Equals(""))
                    {
                        if (userDatabase.GetSingleDatabaseEntry("_id", new ObjectId(id), out BsonDocument user))
                        {
                            /*if (CDN.Add(media, out pathtomedia) )
                            {
                                Post post = new Post(user.GetElement("_id").Value.AsObjectId,message,pathtomedia);
                                postDatabase.AddSingleDatabaseEntry(post.ToBson());
                                Response.Success(resp, "post created successfully", null);
                            }
                            else
                            {
                                Response.Fail(resp,"An error occured with the CDN");
                            }*/
                            Post post = new Post(user.GetElement("_id").Value.AsObjectId,message,"NO CDN FOR NOW BITCH",circles);
                            postDatabase.AddSingleDatabaseEntry(post.ToBson());
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
            else if (req.HttpMethod == "POST" && req.Url?.AbsolutePath == "/rmPost")
            {
                string postid; //id of the post to rm
                string token; //token of the user asking to rm 
                
                StreamReader reader = new StreamReader(req.InputStream);
                string bodyString = await reader.ReadToEndAsync();
                dynamic body = JsonConvert.DeserializeObject(bodyString)!;
                
                try
                {
                    token = ((string) body.token).Trim();
                    postid = ((string) body.id).Trim();
                }
                catch
                {
                    token = "";
                    postid = "";
                }

                if (!(String.IsNullOrEmpty(token) || String.IsNullOrEmpty(postid)))
                {
                    string id = WebToken.GetIdFromToken(token);
                    if (!id.Equals(""))
                    {
                        if (postDatabase.GetSingleDatabaseEntry("_id", postid, out BsonDocument post))
                        {
                            if (post.GetElement("userId").Value.AsObjectId.ToString() == id)
                            {
                                postDatabase.RemoveSingleDatabaseEntry("_id", postid);
                                //DELETE FROM CDN
                                Response.Success(resp, "post deleted successfully", null);
                            }
                            else
                            {
                                Response.Fail(resp,"You do not have the ownership on this post");
                            }
                        }
                        else
                        {
                            Response.Fail(resp,"post doesn't exist");
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
            
            else if (req.HttpMethod == "POST" && req.Url?.AbsolutePath == "/editPost")
            {
                string postid; //id of the post to edit
                string token; //token of the user asking to rm 
                List<BsonValue> newcircles; // new list of circles of a post 
                
                StreamReader reader = new StreamReader(req.InputStream);
                string bodyString = await reader.ReadToEndAsync();
                dynamic body = JsonConvert.DeserializeObject(bodyString)!;
                
                try
                {
                    token = ((string) body.token).Trim();
                    postid = ((string) body.id).Trim();
                    newcircles = ((List<BsonValue>)body.newcircles);
                }
                catch
                {
                    token = "";
                    postid = "";
                    newcircles = null;
                }

                if (!(String.IsNullOrEmpty(token) || String.IsNullOrEmpty(postid) || newcircles == null))
                {
                    string id = WebToken.GetIdFromToken(token);
                    if (!id.Equals(""))
                    {
                        if (postDatabase.GetSingleDatabaseEntry("_id", postid, out BsonDocument post))
                        {
                            if (post.GetElement("userId").Value.AsObjectId.ToString() == id)
                            {
                                Post newpost = new Post(post);
                                newpost.Circles = newcircles;
                                BsonDocument newbson = newpost.ToBson();
                                postDatabase.ReplaceSingleDatabaseEntry("_id", postid, newbson);
                                postDatabase.ReplaceSingleDatabaseEntry("circles", newcircles, post);
                                Response.Success(resp, "post deleted successfully", null);
                            }
                            else
                            {
                                Response.Fail(resp,"You do not have the ownership on this post");
                            }
                        }
                        else
                        {
                            Response.Fail(resp,"post doesn't exist");
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
