using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace posts;

public class Post
{
    public BsonObjectId UserId { get; set; }
    public String Message { get; set; }
    public String PathToMedia { get; set; }

    public String Username { get; set; }
    
    public String Useravatar { get; set; }
    public List<BsonValue> Circles { get; set; }
    public List<BsonValue> Upvoter { get; set; }

    public List<BsonValue> Downvoter { get; set; }
    
    public uint Date { get; set; }

    public Post(BsonObjectId id, string message, string pathtomedia ,List<BsonValue> circles,string username,string useravatar)
    {
        UserId = id;
        Message = message;
        PathToMedia = pathtomedia;
        Circles = circles;
        Username = username;
        Useravatar = useravatar;
        Date = (uint) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        Upvoter = new List<BsonValue>();
        Downvoter = new List<BsonValue>();
    }

    public Post(BsonDocument document)
    {
        UserId = document.GetElement("userId").Value.AsObjectId;
        Message = document.GetElement("message").Value.AsString;
        PathToMedia = document.GetElement("pathtomedia").Value.AsString;
        Date = (uint) document.GetElement("date").Value.AsInt64;
        Username = document.GetElement("username").Value.AsString;
        Useravatar = document.GetElement("useravatar").Value.AsString;
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
                new("date",Date),
                new("username",Username),
                new("useravatar",Useravatar),
                new("circles",circles),
                new("upvoter", upvoter),
                new("downvoter", downvoter)
            });
        return result;
        }
    } 