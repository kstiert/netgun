using MongoDB.Bson;
using MongoDB.Bson.IO;

namespace Netgun.Model
{
    public class Document
    {
        public static Document FromBsonDocument(BsonDocument bson)
        {
            var doc = new Document();
            var jsonSettings = new JsonWriterSettings()
            {
                Indent = true,
                NewLineChars = "\n",
                OutputMode = JsonOutputMode.Shell
            };

            doc.Raw = bson.ToJson();
            doc.Json = bson.ToJson(jsonSettings);
            return doc;
        }


        public string Raw { get; set; }

        public string Json { get; set; }
    }
}