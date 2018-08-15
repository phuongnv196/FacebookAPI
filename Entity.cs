using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace FacebookAPI.Entity
{
    public class Post
    {
        public string Id { set; get; }
        public string ObjectId { set; get; }
        public string Caption { set; get; }
        public DateTime CreateTime { set; get; }
        public string Description { set; get; }
        public string Message { set; get; }
    }
    public class Media
    {
        [JsonProperty(PropertyName = "media_fbid")]
        public string MediaFbID { get; set; }
    }
    public class AttachedMedia
    {
        [JsonProperty(PropertyName = "attached_media")]
        public Media[] Attached { set; get; }
    }
    public class Friend
    {
        public string Uid { get; set; }
        public string Name { get; set; }
        public int FriendCount { get; set; }
        public long Follow { get; set; }
    }
    public class Groups
    {
        public string Id { get; set; }
        public string Cover { get; set; }
        public string Name { get; set; }
        public string Purpose { get; set; }
    }
    public class Comment
    {
        public string Id { set; get; }
        public string Message { set; get; }
        public DateTime CreateTime { set; get; }
        public string Uid { set; get; }
        public string Name { set; get; }
    }
}
