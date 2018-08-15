using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using FacebookAPI.Entity;
namespace FacebookAPI
{
    public class MD5
    {
        public static string Create(string s)
        {
            using (var provider = System.Security.Cryptography.MD5.Create())
            {
                StringBuilder builder = new StringBuilder();

                foreach (byte b in provider.ComputeHash(Encoding.UTF8.GetBytes(s)))
                    builder.Append(b.ToString("x2").ToLower());

                return builder.ToString();
            }
        }
    }
    
    public class Request
    {
        public static string Send(string url, bool isRedirect=false, string postData = "")
        {
            string html = "";
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                if (postData == "")
                    request.Method = "GET";
                else
                    request.Method = "POST";
                if (isRedirect == false)
                    request.AllowAutoRedirect = false;
                request.Headers.Add("Upgrade-Insecure-Requests: 1");
                request.Accept = "*/*";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) coc_coc_browser/66.4.104 Chrome/60.4.3112.104 Safari/537.36";
                request.Headers.Add("Accept-Language: vi-VN,vi;q=0.8,fr-FR;q=0.6,fr;q=0.4,en-US;q=0.2,en;q=0.2");
                request.Headers.Add("X-Requested-With:XMLHttpRequest");
                Stream dataStream;
                if (postData != "")
                {
                    request.ContentType = "application/x-www-form-urlencoded";
                    byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                    request.ContentLength = byteArray.Length;
                    dataStream = request.GetRequestStream();
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Flush();
                    dataStream.Close();
                }
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                html = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                response.Close();
                return html;
            }
            catch (Exception e) { throw e; }
            
        }
    }
    public class Facebook
    {
        string user, pass;
        static string token, uid;
        static bool status;
        Dictionary<string, string> postData = new Dictionary<string, string>();
        /// <summary>
        /// Khởi tạo lớp Facebook API
        /// </summary>
        /// <param name="user">Tên đăng nhập</param>
        /// <param name="pass">Mật khẩu</param>
        public Facebook(string user, string pass)
        {
            this.user = user;
            this.pass = pass;
            postData.Add("api_key", "882a8490361da98702bf97a021ddc14d");
            postData.Add("email", user);
            postData.Add("format", "JSON");
            postData.Add("locale", "vi_vn");
            postData.Add("method", "auth.login");
            postData.Add("password", pass);
            postData.Add("return_ssl_resources", "0");
            postData.Add("v", "1.0");
        }
        string SigCreate()
        {
            string stringSig = "";
            foreach(var item in postData)
            {
                stringSig += item.Key + "=" + item.Value;
            }
            stringSig += "62f8ce9f74b12f84c123cc23437a4a32";
            Console.WriteLine(stringSig);
            return MD5.Create(stringSig);
        }
        /// <summary>
        /// Lấy token từ facebook
        /// </summary>
        /// <returns>Trả về ahihi</returns>
        public bool GetToken() 
        {
            try
            {
                string postString = "api_key=882a8490361da98702bf97a021ddc14d&email={0}&password={1}&format=JSON&locale=vi_vn&method=auth.login&return_ssl_resources=0&v=1.0&sig={2}";
                postString = String.Format(postString, user, pass, SigCreate());
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.facebook.com/restserver.php");
                request.Method = "POST";
                request.AllowAutoRedirect = false;
                request.Headers.Add("Upgrade-Insecure-Requests: 1");
                request.Accept = "*/*";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) coc_coc_browser/66.4.104 Chrome/60.4.3112.104 Safari/537.36";
                request.Headers.Add("Accept-Language: vi-VN,vi;q=0.8,fr-FR;q=0.6,fr;q=0.4,en-US;q=0.2,en;q=0.2");
                request.Headers.Add("X-Requested-With:XMLHttpRequest");
                Stream dataStream;
                if (postString != "")
                {
                    request.ContentType = "application/x-www-form-urlencoded";
                    byte[] byteArray = Encoding.UTF8.GetBytes(postString);
                    request.ContentLength = byteArray.Length;
                    dataStream = request.GetRequestStream();
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Flush();
                    dataStream.Close();
                }
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string tokenData = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                response.Close();
                Match match = Regex.Match(tokenData, "uid\":(\\d+).+access_token\":\"(.+?)\"");
                if (match.Success)
                {
                    token = match.Groups[2].Value;
                    uid = match.Groups[1].Value;
                    status = true;
                    return true;
                }
                status = false;
                return false;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        /// <summary>
        /// Trạng thái lấy token
        /// </summary>
        public static bool Status { get { return status; } }
        /// <summary>
        /// Trả về token đã được lấy
        /// </summary>
        public static string Token { get { return token; } }
        /// <summary>
        /// Trả về uid của tài khoản facebook
        /// </summary>
        public static string Uid { get { return uid; } }
        public List<Post> PostList {
            get
            {
                List<Post> lstPost = new List<Post>();
                string data = Request.Send("https://graph.facebook.com/v3.0/me/posts?fields=caption%2Cid%2Ccreated_time%2Cdescription%2Cmessage%2Cobject_id&access_token="+Facebook.Token);
                JObject jobject = JObject.Parse(data);
                JToken jdata = jobject["data"];
                for (int i=0; i< jdata.Count(); i++)
                {
                    Console.WriteLine(jdata[i]["id"]);
                    lstPost.Add(new Post
                    {
                        Id = (string)jdata[i]["id"],
                        Caption=(string)jdata[i]["caption"],
                        CreateTime= (DateTime)jdata[i]["created_time"],
                        Description =(string)(string)jdata[i]["description"],
                        Message= (string)(string)jdata[i]["message"],
                        ObjectId= (string)(string)jdata[i]["object_id"],
                    });
                }
                return lstPost;
            }
        }
        string UpLoadPhoto(string fileName)
        {
            Dictionary<string, object> d = new Dictionary<string, object>();
            Upload.FileParameter f = new Upload.FileParameter(File.ReadAllBytes(fileName), fileName, "multipart/form-data");
            d.Add(fileName, f);
            string ua = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
            StreamReader rd = new StreamReader(Upload.MultipartFormDataPost("https://graph.facebook.com/v3.0/me/photos?published=false&access_token="+Token, ua, d).GetResponseStream());
            JToken jtoken = JToken.Parse(rd.ReadToEnd());
            if (String.IsNullOrEmpty((string)jtoken["id"]))
            {
                throw new Exception("Lỗi trong lúc post ảnh!");
            }
            else
                return (string)jtoken["id"];
        }
        string UpLoadPhoto(string fileName, string id)
        {
            Dictionary<string, object> d = new Dictionary<string, object>();
            Upload.FileParameter f = new Upload.FileParameter(File.ReadAllBytes(fileName), fileName, "multipart/form-data");
            d.Add(fileName, f);
            string ua = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
            StreamReader rd = new StreamReader(Upload.MultipartFormDataPost("https://graph.facebook.com/v3.0/"+id+"/photos?published=false&access_token=" + Token, ua, d).GetResponseStream());
            JToken jtoken = JToken.Parse(rd.ReadToEnd());
            if (String.IsNullOrEmpty((string)jtoken["id"]))
            {
                throw new Exception("Lỗi trong lúc post ảnh!");
            }
            else
                return (string)jtoken["id"];
        }
        /// <summary>
        /// Đăng nhiều ảnh lên tường
        /// </summary>
        /// <param name="message"></param>
        /// <param name="fileNames"></param>
        /// <returns></returns>
        public string UpLoadPhoto(string message, string[] fileNames)
        {
            try
            {
                List<Media> lstMedia = new List<Media>();
                foreach(string item in fileNames)
                {
                    Media media = new Media
                    {
                        MediaFbID = UpLoadPhoto(item)
                    };
                    lstMedia.Add(media);
                }
                string postString = "attached_media=" + JsonConvert.SerializeObject(lstMedia);
                string data=Request.Send("https://graph.facebook.com/v3.0/me/feed?message=" + message+"&access_token=" + Token, false, postString);
                JToken jtoken = JToken.Parse(data);
                return (string)jtoken["id"];
            }
            catch(Exception e)
            {
                throw e;
            }

        }
        /// <summary>
        /// Đăng ảnh lên nhiều nhóm với id truyền vào
        /// </summary>
        /// <param name="message"></param>
        /// <param name="fileNames"></param>
        /// <param name="idArray"></param>
        /// <returns></returns>
        public void UpLoadPhoto(string message, string[] fileNames, string[] idArray)
        {
            try
            {
                foreach(string item in idArray)
                {
                    List<Media> lstMedia = new List<Media>();
                    foreach (string it in fileNames)
                    {
                        Media media = new Media
                        {
                            MediaFbID = UpLoadPhoto(it, item)
                        };
                        lstMedia.Add(media);
                    }
                    string postString = "attached_media=" + JsonConvert.SerializeObject(lstMedia);
                    List<string> lstId = new List<string>();
                    string data = Request.Send("https://graph.facebook.com/v3.0/"+item+"/feed?message=" + message + "&access_token=" + Token, false, postString);
                    JToken jtoken = JToken.Parse(data);
                    lstId.Add((string)jtoken["id"]);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        /// <summary>
        /// Loại cảm xúc
        /// </summary>
        public enum ReationsType
        {
            NONE,
            LIKE,
            LOVE,
            WOW,
            HAHA,
            SAD,
            ANGRY,
            THANKFUL,
            PRIDE
        }
        /// <summary>
        /// Gửi cảm xúc về một bài viết với id và loại cảm xúc truyền vào
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool Reaction(string id, ReationsType type)
        {
            string postString = "type="+type;
            string data = Request.Send("https://graph.facebook.com/v3.0/100002744658811_1349388685162607/reactions?access_token="+Token, false, postString);
            JToken jtoken = JToken.Parse(data);
            return (bool)jtoken["success"];
        }
        public List<Friend> GetFriend
        {
            get
            {
                try
                {
                    List<Friend> lstFriend = new List<Friend>();
                    string url = "https://graph.facebook.com/fql?q=SELECT+uid,+name,+friend_count,+subscriber_count+FROM+user+WHERE+uid+IN+(SELECT+uid2+FROM+friend+WHERE+uid1+=+me())++ORDER+BY+name+LIMIT+5000&access_token=" + Token;
                    string data = Request.Send(url);
                    JObject jobject = JObject.Parse(data);
                    JToken jtoken = jobject["data"];
                    foreach(var item in jtoken)
                    {
                        lstFriend.Add(new Friend
                        {
                            Uid = (string)item["uid"],
                            Name = Regex.Unescape((string)item["name"]),
                            Follow = (String.IsNullOrEmpty((string)item["subscriber_count"]))?0:(long)item["subscriber_count"],
                            FriendCount = (String.IsNullOrEmpty((string)item["friend_count"])) ? 0 : (int)item["subscriber_count"]
                        });
                    }
                    return lstFriend;
                }
                catch
                {
                   throw new Exception("Lỗi không thể lấy được danh sách bạn bè!");
                }
            }
        }
        public List<Groups> GetGroup
        {
            get
            {
                List<Groups> lstGroup = new List<Groups>();
                try
                {
                    string data = Request.Send("https://graph.facebook.com/v3.0/me/groups?fields=cover,name,id,purpose&limit=10000&access_token=" + Token);
                    JObject jobject = JObject.Parse(data);
                    JToken jtoken = jobject["data"];

                    foreach (JToken item in jtoken)
                    {
                        JToken cover = item["cover"];
                        lstGroup.Add(
                            new Groups
                            {
                                Id = (string)item["id"],
                                Name = (string)item["name"],
                                Purpose = (string)item["purpose"],
                                Cover = (cover == null ? null : (string)cover["source"])
                            }
                        );
                    }
                    return lstGroup;
                }catch
                {
                    throw new Exception("Lỗi không thể lấy được danh sách nhóm!");
                }
            }
        }
        public List<Comment> GetComment(string postId)
        {
            try
            {
                List<Comment> lstComment = new List<Comment>();
                string data = Request.Send("https://graph.facebook.com/v3.0/" + postId + "/comments?limit=10000&access_token=" + Token);
                JObject jobject = JObject.Parse(data);
                JToken jdata = jobject["data"];
                foreach(JToken item in jdata)
                {
                    lstComment.Add(new Comment {
                        Id=(string)item["id"],
                        Message=(string)item["message"],
                        CreateTime=(DateTime)item["created_time"],
                        Uid=(string)item["from"]["id"],
                        Name=(string)item["from"]["name"]
                    });
                }
                return lstComment;
            }
            catch
            {
                throw new Exception("Không thể lấy danh sách bình luận!");
            }
        }
        public bool SendComment(string id,string message)
        {
            try
            {
                string url = "https://graph.facebook.com/v3.0/" + id+"/comments?message="+message+"&access_token="+Token+ "&method=POST";
                if (Request.Send(url).IndexOf("id") != -1)
                    return true;
                else
                    return false;
            }catch(Exception e)
            {
                throw e;
            }
        }
        public bool SendComment(string id, string message, string stickerId)
        {
            try
            {
                string url = "https://graph.facebook.com/v3.0/" + id + "/comments?message=" + message + "&attachment_id="+stickerId+"&access_token=" + Token+ "&method=POST";
                string data = Request.Send(url);
                if (data.IndexOf("id") != -1)
                    return true;
                else
                    return false;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public bool LikeComment(string id)
        {
            try
            {
                string data = Request.Send("https://graph.facebook.com/v3.0/"+id+"/likes?access_token="+Token+ "&method=POST");
                if (data.IndexOf("true") != -1)
                    return true;
                else
                    return false;
            }
            catch
            {
                throw new Exception("Có lỗi! Không thể thích bình luận này!");
            }
        }
        public bool RemoveLikeComment(string id)
        {
            try
            {
                string data = Request.Send("https://graph.facebook.com/v3.0/" + id + "/likes?access_token=" + Token + "&method=DELETE");
                if (data.IndexOf("true") != -1)
                    return true;
                else
                    return false;
            }
            catch
            {
                throw new Exception("Có lỗi! Không thể thích bình luận này!");
            }
        }
    }
}
