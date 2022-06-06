using System.Net;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace posts;

class HttpServer
{
    private static HttpListener? listener;

    private static async Task HandleIncomingConnections(EasyMango.EasyMango userDatabase, EasyMango.EasyMango postDatabase,WebToken.WebToken jwt, string CDNurl)
    {
        
         async Task<bool> DeleteFile(string filename, string CDNurl)
        {
            HttpClient client = new HttpClient();
        
            Dictionary<string, string> body = new Dictionary<string, string>
            {
                {"id",filename}
            };
        
        
            string requestBody = JsonConvert.SerializeObject(body);
            var httpContent = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var result = await client.PostAsync(CDNurl + "/deleteFile", httpContent);
            string bodystr = await result.Content.ReadAsStringAsync();
            dynamic json = JsonConvert.DeserializeObject(bodystr)!;
            return ((string) json.status == "success");
        }
         
         async Task<Tuple<bool,string>> AddFile(string media, string CDNurl)
         {
             HttpClient client = new HttpClient();
        
             Dictionary<string, string> body = new Dictionary<string, string>
             {
                 {"data",media}
             };
        
        
             string requestBody = JsonConvert.SerializeObject(body);
             var httpContent = new StringContent(requestBody, Encoding.UTF8, "application/json");
             var result = await client.PostAsync(CDNurl + "/addFile", httpContent);
             string bodystr = await result.Content.ReadAsStringAsync();
             dynamic json = JsonConvert.DeserializeObject(bodystr)!;
             return new Tuple<bool, string>((string)json.status == "success",(string)json.data);
         }
        
         async Task<Tuple<string,string>> GetFile(string pathtomedia,string CDNurl)
         {
             HttpClient client = new HttpClient();
        
             Dictionary<string, string> body = new Dictionary<string, string>
             {
                 {"id",pathtomedia}
             };
        
        
             string requestBody = JsonConvert.SerializeObject(body);
             var httpContent = new StringContent(requestBody, Encoding.UTF8, "application/json");
             var result = await client.PostAsync(CDNurl + "/getFile", httpContent);
             string bodystr = await result.Content.ReadAsStringAsync();
             dynamic json = JsonConvert.DeserializeObject(bodystr)!;
             return new Tuple<string, string>((string)json.data,(string)json.message);
         }
         
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
                }
                catch
                {
                    token = "";
                    message = "";
                }
                try
                {
                    foreach (var circle in body.circles)
                    {
                        circles.Add(new BsonObjectId(circle));
                    }
                    media = ((string) body.media).Trim();
                }
                catch
                {
                    media = "";
                }
                
                
                if (!(String.IsNullOrEmpty(token) || String.IsNullOrEmpty(message))) 
                {
                    string id = jwt.GetIdFromToken(token);
                    if (!id.Equals(""))
                    {
                        if (userDatabase.GetSingleDatabaseEntry("_id", new BsonObjectId(new ObjectId(id)), out BsonDocument user))
                        {
                            Tuple<bool,string> CDNresponse = AddFile(media, CDNurl).Result;
                            if (CDNresponse.Item1)
                            {
                                Post post = new Post(user.GetElement("_id").Value.AsObjectId,message,CDNresponse.Item2,circles);
                                postDatabase.AddSingleDatabaseEntry(post.ToBson());
                                if (postDatabase.GetSingleDatabaseEntry("pathtomedia",CDNresponse.Item2,out BsonDocument postindb))
                                {
                                    Response.Success(resp, "post created successfully", postindb.GetElement("_id").Value.AsObjectId.ToString());
                                }
                                else
                                {
                                    Response.Fail(resp,"An error occured with the Database");
                                }
                            }
                            else
                            {
                                Response.Fail(resp,"An error occured with the CDN");
                            }
                            //Post post = new Post(user.GetElement("_id").Value.AsObjectId,message,"NO CDN FOR NOW BITCH",circles);
                            //postDatabase.AddSingleDatabaseEntry(post.ToBson());
                            //Response.Success(resp, "post created successfully", null);
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
                string pathtomedia;
                
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
                    string id = jwt.GetIdFromToken(token);
                    if (!id.Equals(""))
                    {
                        if (postDatabase.GetSingleDatabaseEntry("_id", new BsonObjectId(new ObjectId(postid)), out BsonDocument post))
                        {
                            if (post.GetElement("userId").Value.AsObjectId.ToString() == id)
                            {
                                pathtomedia = post.GetElement("pathtomedia").Value.AsString;
                                //DELETE FROM CDN
                                if (DeleteFile(pathtomedia, CDNurl).Result && postDatabase.RemoveSingleDatabaseEntry("_id", new BsonObjectId(new ObjectId(postid))))
                                {
                                    Response.Success(resp, "post deleted successfully", null);
                                }
                                else
                                {
                                    Response.Fail(resp,"Couldn't delete post");
                                }
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
                    newcircles = new List<BsonValue>();
                    foreach (var circle in body.circles)
                    {
                        newcircles.Add(circle.ToString());
                    }
                }
                catch
                {
                    token = "";
                    postid = "";
                    newcircles = null;
                }

                if (!(String.IsNullOrEmpty(token) || String.IsNullOrEmpty(postid) || newcircles == null))
                {
                    string id = jwt.GetIdFromToken(token);
                    if (!id.Equals(""))
                    {
                        if (postDatabase.GetSingleDatabaseEntry("_id", new BsonObjectId(new ObjectId(postid)), out BsonDocument post))
                        {
                            if (post.GetElement("userId").Value.AsObjectId.ToString() == id)
                            {
                                Post newpost = new Post(post);
                                newpost.Circles = newcircles;
                                BsonDocument newbson = newpost.ToBson();
                                if (postDatabase.ReplaceSingleDatabaseEntry("_id", new BsonObjectId(new ObjectId(postid)), newbson))
                                {
                                    Response.Success(resp, "post edited successfully", null);
                                }
                                else
                                {
                                    Response.Fail(resp,"An error occured with the database");
                                }
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
            else if (req.HttpMethod == "POST" && req.Url?.AbsolutePath == "/vote")
            {
                string token;
                string postid;
                string direction;
                
                StreamReader reader = new StreamReader(req.InputStream);
                string bodyString = await reader.ReadToEndAsync();
                dynamic body = JsonConvert.DeserializeObject(bodyString)!;

                try
                {
                    token = ((string) body.token).Trim();
                    postid = ((string) body.id).Trim();
                    direction = ((string) body.direction).Trim();
                }
                catch
                {
                    token = "";
                    postid = "";
                    direction = "";
                }
                if (!(String.IsNullOrEmpty(token) || String.IsNullOrEmpty(postid) ))
                {
                    string id = jwt.GetIdFromToken(token);
                    if (!id.Equals(""))
                    {
                        if (postDatabase.GetSingleDatabaseEntry("_id", new BsonObjectId(new ObjectId(postid)), out BsonDocument post))
                        {
                            
                            if (direction == "up")
                            {
                                Post newpost = new Post(post);
                                if (newpost.Upvoter.Contains(new BsonObjectId(new ObjectId(postid))))
                                {
                                    newpost.Upvoter.Remove(new BsonObjectId(new ObjectId(postid)));
                                }
                                else
                                {
                                    newpost.Upvoter.Add(new BsonObjectId(new ObjectId(postid)));
                                }

                                BsonDocument newbson = newpost.ToBson();
                                if (postDatabase.ReplaceSingleDatabaseEntry("_id", new BsonObjectId(new ObjectId(postid)), newbson))
                                {
                                    Response.Success(resp, "post upvoted successfully", null);
                                }
                                else
                                {
                                    Response.Fail(resp,"An error occured with the database");
                                }
                            }
                            else if (direction == "down")
                            {
                                Post newpost = new Post(post);
                                if (newpost.Downvoter.Contains(new BsonObjectId(new ObjectId(postid))))
                                {
                                    newpost.Downvoter.Remove(new BsonObjectId(new ObjectId(postid)));
                                }
                                else
                                {
                                    newpost.Downvoter.Add(new BsonObjectId(new ObjectId(postid)));
                                }

                                BsonDocument newbson = newpost.ToBson();
                                if (postDatabase.ReplaceSingleDatabaseEntry("_id", new BsonObjectId(new ObjectId(postid)), newbson))
                                {
                                    Response.Success(resp, "post downvoted successfully", null);
                                }
                                else
                                {
                                    Response.Fail(resp,"An error occured with the database");
                                }
                            }
                            else
                            {
                                Response.Fail(resp, "invalid direction");
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
            else if (req.HttpMethod == "POST" && req.Url?.AbsolutePath == "/getPost")
            {
                string token;
                string postid;

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
                    string id = jwt.GetIdFromToken(token);
                    if (!id.Equals(""))
                    {
                        // TODO /!\ have to check if ID is allowed to get post => is in one of the cirlces of the post
                        if (postDatabase.GetSingleDatabaseEntry("_id", new BsonObjectId(new ObjectId(postid)), out BsonDocument post))
                        {
                            Post postO = new Post(post);
                            string pathtomedia = postO.PathToMedia;
                            string media = GetFile(pathtomedia, CDNurl).Result.Item1;
                            Dictionary<string, string> respbody = new Dictionary<string, string>
                            {
                                {"postid",postid},
                                {"userid",postO.UserId.Value.ToString()},
                                {"message",postO.Message},
                                {"date",postO.Date.ToString()},
                                {"media",media},
                                {"Cirlces",postO.Circles.ToString()},
                                {"upvoter",postO.Upvoter.ToString()},
                                {"downvoter",postO.Downvoter.ToString()}
                            };
        
        
                            string requestBody = JsonConvert.SerializeObject(respbody);
                            Response.Success(resp,"success",requestBody);
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
        string CDNurl = config.GetValue<string>("CDNurl");
        string secretKey = config.GetValue<string>("secretKey");
        uint expireDelay = config.GetValue<uint>("expireDelay");
        WebToken.WebToken jwt = new WebToken.WebToken(secretKey, expireDelay);
        // Handle requests
        Task listenTask = HandleIncomingConnections(userDatabase, postDatabase, jwt ,CDNurl);
        listenTask.GetAwaiter().GetResult();
        
        // Close the listener
        listener.Close();
    }
}
