using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace posts;

public class Post
{
    public BsonObjectId UserId { get; set; }
    public String Message { get; set; }

    
    public Post(BsonObjectId id, string message)
    {
        UserId = id;
        Message = message;
    }

    public Post(BsonDocument document)
    {
        UserId = document.GetElement("userId").Value.AsObjectId;
        Message = document.GetElement("message").Value.AsString;;
    }

    public BsonDocument ToBson()
    {
        BsonDocument result = new BsonDocument(
            (IEnumerable<BsonElement>)
            new BsonElement[]
            {
                new ("userId", UserId),
                new ("message", Message)
            });
        return result;
    }
}    