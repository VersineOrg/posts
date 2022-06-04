using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace posts;

public class Post
{
    public BsonObjectId UserId { get; set; }
    public String Message { get; set; }
    public String PathToMedia { get; set; }

    public List<BsonValue> Circles { get; set; }
    public List<BsonValue> Upvoter { get; set; }

    public List<BsonValue> Downvoter { get; set; }



    public Post(BsonObjectId id, string message, string pathtomedia ,List<BsonValue> circles)
    {
        UserId = id;
        Message = message;
        PathToMedia = pathtomedia;
        Circles = circles;
        Upvoter = new List<BsonValue>();
        Downvoter = new List<BsonValue>();
    }

    public Post(BsonDocument document)
    {
        UserId = document.GetElement("userId").Value.AsObjectId;
        Message = document.GetElement("message").Value.AsString;
        PathToMedia = document.GetElement("pathtomedia").Value.AsString;
        Circles = document.GetElement("circles").Value.AsBsonArray.ToList();
        Upvoter = document.GetElement("upvoter").Value.AsBsonArray.ToList();
        Downvoter = document.GetElement("downvoter").Value.AsBsonArray.ToList();
    }

    public BsonDocument ToBson()
    {
        BsonArray circles = new BsonArray();
        BsonArray upvoter = new BsonArray();
        BsonArray downvoter = new BsonArray();
        foreach (var id in Circles)
        {
            circles.Add(id);
        }
        foreach (var id in Upvoter)
        {
            upvoter.Add(id);
        }

        foreach (var id in Downvoter)
        {
            downvoter.Add(id);
        }
        BsonDocument result = new BsonDocument(
            (IEnumerable<BsonElement>)
            new BsonElement[]
            {
                new("userId", UserId),
                new("message", Message),
                new("pathtomedia", PathToMedia),
                new("circles",circles),
                new("upvoter", upvoter),
                new("downvoter", downvoter)
            });
        return result;
        }
    } 