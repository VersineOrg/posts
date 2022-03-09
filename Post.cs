using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace posts;

public class Post
{
    public String UserId { get; set; }
    public String Message { get; set; }

    
    public Post(string id, string password)
    {
        this.UserId = id;
        this.Message = password;
    }

    public Post(BsonDocument document)
    {
        this.UserId = document.GetElement("userId").Value.AsString;
        this.Message = document.GetElement("message").Value.AsString;;
    }

    public BsonDocument ToBson()
    {
        BsonDocument result = new BsonDocument(
            new BsonElement("userId", UserId),
            new BsonElement("message", Message));
        return result;
    }
}    