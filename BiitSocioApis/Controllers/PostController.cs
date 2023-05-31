using BiitSocioApis.classes;
using BiitSocioApis.HelpingClasses;
using BiitSocioApis.Models;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using static System.Collections.Specialized.BitVector32;
using HttpGetAttribute = System.Web.Http.HttpGetAttribute;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;

namespace BiitSocioApis.Controllers
{
    public class PostController : ApiController
    {
       
        
        [System.Web.Http.HttpPost]
        public HttpResponseMessage addPost()
        {
            BIITSOCIOEntities db = new BIITSOCIOEntities();
            try
            {
                Post post = new Post();
                HttpRequest request = HttpContext.Current.Request;
                post.postedBy = request["postedBy"];
                post.postFor = request["postFor"];
                post.description = request["description"];
                post.dateTime = request["dateTime"];
                post.type = request["type"];
                post.user = request["user"];
                post.fromWall = request["fromWall"];
                if (request["type"] == "image" || request["type"] == "video")
                {
                    HttpPostedFile imagefile = request.Files["image"];
                    post.text= saveImage(imagefile, post.postedBy, "postImages");
                }
                // if (Request.conte["type"]=="image")
                db.Posts.Add(post);
                db.SaveChanges();
                notification n = new notification();
                n.post_id = db.Posts.OrderByDescending(s => s.id).Select(s => s.id).FirstOrDefault();
                n.status = 0;
                n.NotificationTo = post.postFor;
                n.fromWall = int.Parse(post.fromWall);
                n.dateTime = DateTime.Now.ToShortDateString();
                n.type = "newPost";
                n.notificationFrom = post.postedBy.Trim();
                db.notifications.Add(n);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK,"Post Added Successfully!");
            }
            catch (Exception ex)
            {
                return getExceptionMessage();
            }

        }
        [HttpGet]
        public HttpResponseMessage getPostsForHistory(string userCnic,int lastSavedPostId)
        {
            try
            {
                BIITSOCIOEntities db = new BIITSOCIOEntities();
                List<Post> posts = new List<Post>();
                var user = db.Users.Where(s => s.CNIC == userCnic).FirstOrDefault();
                if (user.userType == "1")
                {
                    user.section = db.students.Where(s => s.cnic == userCnic).Select(s => s.section).FirstOrDefault();

                }
                posts= db.Posts.Where(p => p.id>lastSavedPostId&&p.postFor.ToLower().Contains(user.CNIC.ToLower())).Distinct().ToList();
                if (user.userType == "1")
                {
                    posts.AddRange(db.Posts.Where(p => p.id > lastSavedPostId&& p.postFor.ToLower().Contains(user.section.ToLower())).Distinct().ToList());
                }
                posts = posts.Distinct().ToList();
                var posts1 = getDetail(posts, userCnic);
                return Request.CreateResponse(HttpStatusCode.OK,posts1);

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, "something gone wrong!");
            }
        }
       

        [HttpGet]
        public HttpResponseMessage getAllTimeTable() {

            try
            {
                BIITSOCIOEntities db = new BIITSOCIOEntities();
                var res = new
                {
                    
                    slot = db.Timetables.ToList().Select(s => s.slot).Distinct().ToList(),
                    monday = db.Timetables.Select(s => new { data = s.monday, slot = s.slot }).ToList(),
                    tuesday = db.Timetables.Select(s => new { data = s.tuesday, slot = s.slot }).ToList(),
                    wednesday = db.Timetables.Select(s => new { data = s.wednesday, slot = s.slot }).ToList(),
                    thursday = db.Timetables.Select(s => new { data = s.thursday, slot = s.slot }).ToList(),
                    friday = db.Timetables.Select(s => new { data = s.friday, slot = s.slot }).ToList(),
                    venue = getVenue(),
                    teacher = db.Users.Where(s=>s.userType=="2").Select(s => s.CNIC).Distinct().ToList()

                };
                return Request.CreateResponse(HttpStatusCode.OK,res);
              
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Try again");
            }
        }
        [HttpGet]
        public HttpResponseMessage getTimeTable(string section,string userType) {
            try
            {
                BIITSOCIOEntities db = new BIITSOCIOEntities();
                /* var res = db.Timetables.Where(s => s.section.ToLower().Contains(section.ToLower())).GroupBy(s => s.slot).ToList();
                 var sortedObjects = res.OrderBy(obj => {
                     var startTime = DateTime.ParseExact(obj.Key.Split('-')[0].Trim(), "h:mm", null);
                     return startTime.TimeOfDay;
                 }).ToList();*/

                    if (userType == "1")
                    {
                    var res = new
                    {
                        slot = db.Timetables.ToList().Select(s => s.slot).Distinct().ToList(),
                        monday = db.Timetables.Where(s => s.section.ToLower().Contains(section.ToLower())).Select(s => new { data = s.monday, slot = s.slot }).ToList(),
                        tuesday = db.Timetables.Where(s => s.section.ToLower().Contains(section.ToLower())).Select(s => new { data = s.tuesday, slot = s.slot }).ToList(),
                        wednesday = db.Timetables.Where(s => s.section.ToLower().Contains(section.ToLower())).Select(s => new { data = s.wednesday, slot = s.slot }).ToList(),
                        thursday = db.Timetables.Where(s => s.section.ToLower().Contains(section.ToLower())).Select(s => new { data = s.thursday, slot = s.slot }).ToList(),
                        friday = db.Timetables.Where(s => s.section.ToLower().Contains(section.ToLower())).Select(s => new { data = s.friday, slot = s.slot }).ToList(),
                        venue =new List<string>(),
                        teacher = new List<string>(),


                    };
                       return Request.CreateResponse(HttpStatusCode.OK, res);
                    }
                    else {
                        string day = "Monday";
                        var res = new {

                            slot = db.Timetables.ToList().Select(s => s.slot).Distinct().ToList(),
                          monday =db.Timetables.AsEnumerable().Where(s=> checkTeahcer(s.monday.ToLower(), section.ToLower())).Select(s=>new {data= s.monday,slot=s.slot }).ToList(),
                            tuesday = db.Timetables.AsEnumerable().Where(s => checkTeahcer(s.tuesday.ToLower(),section.ToLower())).Select(s => new { data = s.tuesday,slot = s.slot }).ToList(),
                            wednesday = db.Timetables.AsEnumerable().Where(s => checkTeahcer(s.wednesday.ToLower(), section.ToLower())).Select(s => new { data = s.wednesday, slot = s.slot }).ToList(),
                            thursday = db.Timetables.AsEnumerable().Where(s => checkTeahcer(s.thursday.ToLower(), section.ToLower())).Select(s => new { data = s.thursday, slot = s.slot }).ToList(),
                            friday = db.Timetables.AsEnumerable().Where(s => checkTeahcer(s.friday.ToLower(), section.ToLower())).Select(s => new { data = s.friday, slot = s.slot }).ToList(),
                            venue = new List<string>(),
                            teacher = new List<string>(),
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, res);

                    }

                
               
            }
            catch (Exception ex)
            {

                return getExceptionMessage();
            }
        }
        private List<string> getVenue() {
            List<string> data = new List<string>();
            try
            {
                BIITSOCIOEntities db = new BIITSOCIOEntities();
                data = db.Timetables.AsEnumerable().Select(s => getLt(s.friday)).Distinct().ToList();

                data.AddRange ( db.Timetables.AsEnumerable().Select(s => getLt(s.tuesday)).Distinct().ToList());

                data.AddRange(db.Timetables.AsEnumerable().Select(s => getLt(s.wednesday)).Distinct().ToList());

                data.AddRange(db.Timetables.AsEnumerable().Select(s => getLt(s.thursday)).Distinct().ToList());

                data.AddRange(db.Timetables.AsEnumerable().Select(s => getLt(s.monday)).Distinct().ToList());
            }
            catch (Exception ex)
            {

            }
            return data.Distinct().ToList();
        }
        private string getLt(string venue) {
            try
            {
                if (venue != "" && venue != null)
                {
                    var v = venue.Split('_');
                    venue = v[v.Length-1];

                }
            }
            catch (Exception ex)
            {
                return "";
            }
            return venue;
        }
        private bool checkTeahcer(string data,string name) {
            try
            {
                if (data == null || data == "")
                    return false;
                var list = ExtractTeacherName(data);
                List<string> var = list.Split(',').ToList();
                if (var.Any(s => s.Trim() == name.Trim()))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {

       
            }
            return false;
        }
        public static string ExtractTeacherName(string inputString)
        {
            string teacherName = "";
            try
            {
                if (inputString == "" || inputString == null)
                { return ""; }
                teacherName = inputString.Split('(')[1].Trim().Split(')')[0].Trim();



                if (teacherName.Contains("BCS-".ToLower()) || teacherName.Contains("BAI-".ToLower()) || teacherName.Contains("BSSE-".ToLower()))
                {
                    String[] d = inputString.Split('(');
                    teacherName = ExtractTeacherName("(" + d[d.Length - 1]);
                }
                else
                {
                    return teacherName;
                }

            }
            catch (Exception ex) { }
            return teacherName;
        }
        [HttpPost]
        public HttpResponseMessage updateProfile()
        {
            try
            {
                BIITSOCIOEntities db = new BIITSOCIOEntities();

                User user = new User();
                HttpRequest request = HttpContext.Current.Request;
                user.name = request["name"];
                user.email = request["email"];
                user.phone = request["phone"];
                user.password = request["password"];
                 user.CNIC = request["CNIC"];
                HttpPostedFile imagefile = request.Files["image"];
                if(imagefile!=null)
                user.profileImage = saveImage(imagefile,user.CNIC, "Images");

                var u=db.Users.Where(s=>s.CNIC==user.CNIC).FirstOrDefault();
                if (user.profileImage != null)
                {
                    u.profileImage = user.profileImage;
                }
                u.name = user.name;
                u.password = user.password;
                u.phone = user.phone;
                u.email = user.email;
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, imagefile==null?"-1":u.profileImage);


            }
            catch (Exception)
            {

                throw;
            }
        }
        [System.Web.Http.HttpPost]
        public HttpResponseMessage addStory()
        {
            BIITSOCIOEntities db = new BIITSOCIOEntities();
            try
            {
                Story post = new Story();
                HttpRequest request = HttpContext.Current.Request;
                post.storyFor = request["storyFor"];
                post.time = request["time"];
                post.type = request["type"];
                post.text = request["text"];
                //post.color=int.Parse()


                if (request["type"] == "image" || request["type"] == "video")
                {
                    HttpPostedFile imagefile = request.Files["image"];

                    post.url = saveImage(imagefile, post.societyId.ToString(), "Status");
                }
                

                // if (Request.conte["type"]=="image")
                db.Stories.Add(post);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Story Added Successfully!");
            }
            catch (Exception ex)
            {
                return getExceptionMessage();
            }

        }

        public string saveImage(HttpPostedFile imagefile,string id,string file)
        {
            string extension = imagefile.FileName.Split('.')[1];
            DateTime dt = DateTime.Now;
            string filename = id + "_" + dt.Year + dt.Month + dt.Day + dt.Minute + dt.Second + dt.Hour + "." + extension;
            // filename = filename + DateTime.Now.ToShortTimeString()+"."+extension;
            imagefile.SaveAs(HttpContext.Current.Server.
                           MapPath("~/"+file+"/" + filename));
            String name = HttpContext.Current.Server.
                           MapPath("~/" + file + "/" + filename);
            return filename;
        }
        [HttpGet]
        public HttpResponseMessage deletePost(int post_id)
        {
            BIITSOCIOEntities db = new BIITSOCIOEntities();
            try
            {
                Post post = db.Posts.Where(s => s.id == post_id).SingleOrDefault();
                // db.Posts.Remove(post);
                post.status = "deleted";
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Post deleted Successfully!");
            }
            catch (Exception ex)
            {
                return getExceptionMessage();
            }

        }
        [HttpPost]
        public HttpResponseMessage uploadFile()
        { BIITSOCIOEntities db = new BIITSOCIOEntities();
            try
            {

                HttpRequest request = HttpContext.Current.Request;
                HttpPostedFile imagefile = request.Files["file"];
                string toUpload = request["toUpload"];
                string extension = imagefile.FileName.Split('.')[1];
                DateTime dt = DateTime.Now;
                string filename = toUpload + dt.Year + dt.Month + dt.Day + dt.Minute + dt.Second + dt.Hour + "." + extension;
                // filename = filename + DateTime.Now.ToShortTimeString()+"."+extension;

                imagefile.SaveAs(HttpContext.Current.Server.
                               MapPath("~/Files/" + filename));
                List<Timetable> res = ReadExcel.readExcel(HttpContext.Current.Server.
                         MapPath("~/Files/" + filename), toUpload);
                if (toUpload == "timeTable")
                {
                    if (res.Count != 0)
                    {
                        new Thread(() =>
                        {
                            foreach (var item in res)
                            {

                                for (int i = 0; i < 5; i++)
                                {
                                    item.courseName = i == 0 ? item.monday.Split('(')[0].Trim() : i == 1 ?
                                        item.tuesday.Split('(')[0].Trim() : i == 2 ?
                                        item.wednesday.Split('(')[0].Trim() : i == 3 ?
                                        item.thursday.Split('(')[0].Trim() : i == 4 ?
                                        item.friday.Split('(')[0].Trim() : "";
                                    if (i == 0)
                                    {

                                        db.Timetables.Add(item);
                                    }

                                    course course = new course();
                                    if (!db.courses.Any(s => s.course_no == item.courseName.Trim()))
                                    {

                                        course.title = item.courseName.Trim();
                                        course.course_no = item.courseName.Trim();
                                        course.course_desc = item.teacherName.Trim();
                                        db.courses.Add(course);
                                        db.SaveChanges();

                                    }
                                    if (item.courseName != "" && item.courseName != null)
                                    {
                                        Allocation allocation = new Allocation();
                                        allocation.course_no = item.courseName.Trim();

                                        allocation.section = item.section.Trim();
                                        allocation.emp_no = i == 0 ? ReadExcel.ExtractTeacherName(item.monday) : i == 1 ?
                                           ReadExcel.ExtractTeacherName(item.tuesday) : i == 2 ?
                                            ReadExcel.ExtractTeacherName(item.wednesday) : i == 3 ?
                                           ReadExcel.ExtractTeacherName(item.thursday) : i == 4 ?
                                           ReadExcel.ExtractTeacherName(item.friday) : "";
                                        if (!db.Allocations.Any(s => s.course_no == allocation.course_no && allocation.emp_no == s.emp_no && s.section == allocation.section))
                                            db.Allocations.Add(allocation);
                                        Group group = new Group();
                                        group.isOfficial = true;
                                        group.Admin = allocation.emp_no;
                                        group.name = allocation.course_no;
                                        if (!db.Groups.Any(s => s.name == group.name))
                                        {
                                            db.Groups.Add(group);
                                            db.SaveChanges();
                                        }

                                    }
                                }


                            }
                            db.SaveChanges();
                        }).Start();


                        return Request.CreateResponse(HttpStatusCode.OK, toUpload + " Saved Successfully!");
                    }
                    else {
                        return Request.CreateResponse(HttpStatusCode.OK, "TimeTable not uploaded!");

                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, "Uploaded!");

                }
            
               

            }
            catch (Exception ex)
            { 
                return getExceptionMessage();
            }
        }

        [HttpGet]
        public HttpResponseMessage getSocietiesDetail(string cnic)
        {
            BIITSOCIOEntities db = new BIITSOCIOEntities();
            try
            {
                var user = db.Users.Where(s => s.CNIC == cnic).FirstOrDefault();
                List<object> data1 = new List<object>();
                if (user.userType == "3")
                {
                    var societies = db.Societies.ToList();
                    foreach (var so in societies)
                    {
                        data1.Add(new
                        {
                            id = so.id,
                            name = so.name,

                            profileImage = so.profileImage,
                            stories = db.Stories.AsEnumerable().Where(p => p.storyFor.Split(',').Intersect(societies.Select(s => s.id.ToString())).Any()).Distinct().ToList()



                        });
                    }
                  
                }
                else { 
                var joined=db.JoinedSocieties.Where(s=>s.userId==cnic).Distinct().ToList();
                    foreach (var item in joined)
                    {
                        var so = db.Societies.Where(s => s.id == item.societyId).FirstOrDefault();
                        data1.Add(new
                        {
                            id = so.id,
                            name = so.name,

                            profileImage = so.profileImage,
                            stories = db.Stories.AsEnumerable().Where(p => p.storyFor.Split(',').Intersect(joined.Select(s => s.societyId.ToString())).Any()).Distinct().ToList()



                        });
                    }
                }
               // var dt = (from sj in db.JoinedSocieties join so in db.Societies on sj.societyId equals so.id where sj.userId == cnic select new {).ToList();
                var data = new {
                        dt = data1,
                        isjoined = user.userType == "3" ? true:db.JoinedSocieties.Any(s => s.userId == cnic),
                        isMentor = user.userType=="3"?true: db.JoinedSocieties.Any(s => s.userId == cnic && s.isMentor==true)
                    };
                    //var detail = db.Societies.Select(s => new { id = s.id, name = s.name, profileImage = s.profileImage, stories = db.Stories.Where(m => m.societyId == s.id).ToList() }).Where(s => s.stories.Count > 0).ToList();



                    return Request.CreateResponse(HttpStatusCode.OK, data);
                

            }
            catch (Exception ex)
            {
                return getExceptionMessage();
            }
        }
        
        [HttpGet]
        public HttpResponseMessage unPinPosts(string user_id,int post_id)
        {
            BIITSOCIOEntities db = new BIITSOCIOEntities();
            try
            {


                var posts = db.Diaries.Where(s => s.user_id == user_id && s.post_id==post_id).FirstOrDefault();
                db.Diaries.Remove(posts);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Un-Pinned");

            }
            catch (Exception ex)
            {
                return getExceptionMessage();
            }
        }

        [HttpGet]
        public HttpResponseMessage getStories() {
            try
            {
                BIITSOCIOEntities db = new BIITSOCIOEntities();
                var detail = db.Societies.Select(s=>new {id=s.id,name=s.name,profileImage=s.profileImage,stories=db.Stories.Where(m=>m.societyId==s.id ) .ToList()}).Where(s=>s.stories.Count>0).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, detail);

            }
            catch (Exception ex)
            {
                return getExceptionMessage();
            }
        }

        [HttpGet]
        public HttpResponseMessage getForParentPosts(string cnic, int pageNumber, string fromWall) {
            try
            {
                BIITSOCIOEntities db = new BIITSOCIOEntities();
                var std = db.students.Where(s=>s.sonOf==cnic);
                return getExceptionMessage();
            }
            catch (Exception ex)
            {

                return getExceptionMessage();
            }
        }
        [HttpGet]
        public HttpResponseMessage getPosts(string cnic,int pageNumber,string fromWall)
        {
            BIITSOCIOEntities db = new BIITSOCIOEntities();
            try
            {
                List<Post> posts = new List<Post>();
                List<PostHelper> posts1 = new List<PostHelper>();
                var u = db.Users.Where(s => s.CNIC == cnic).Select(s => new {
                    std=db.students.Where(k=>k.cnic==cnic).FirstOrDefault(),
                    us=s,
                    teacher= db.Teachers.Where(k => k.cnic == cnic).FirstOrDefault()
                }).FirstOrDefault();
                int startIndex = (pageNumber - 1) * 10;
                if (fromWall == "6")
                {
                    var societies = u.us.userType=="3"?db.Societies.ToList() :db.Societies.Join(db.JoinedSocieties.Where(s=>s.userId==cnic), so => so.id, js => js.societyId, (so, js) => so).Distinct().ToList();
                    posts = db.Posts.AsEnumerable().Where(p=>p.fromWall=="6"&& p.postFor.Split(',').Intersect(societies.Select(s => s.id.ToString())).Any()).ToList();

                }
                else if (fromWall == u.us.userType)
                {
                    List<String> request = db.FriendRequests.Where(s => s.status.ToLower() == "accepted" && (s.RequestedBy.Trim() == cnic || s.RequestedTo.Trim() == cnic)).Select(s => s.RequestedTo.Trim() == cnic ? s.RequestedBy.Trim() : s.RequestedTo.Trim()).ToList();
                    posts = db.Posts.Where(p => p.status != "deleted" && (p.postedBy == cnic.ToLower()
                    || p.postFor.ToLower().Contains(cnic.ToLower()) || p.postFor.ToLower().Contains("all")) && p.fromWall == fromWall).ToList();
                    posts.AddRange(db.Posts.Where(p=>p.fromWall==fromWall).Join(request, p => p.postedBy, re => re, (p, re) => p).ToList());
                    posts = posts.Distinct().ToList();
                    Console.Write("");
                }
                else if (fromWall == "5")
                {
                    //var u = (from us in db.Users join std in db.students on us.CNIC equals std.cnic where std.cnic == cnic.Replace("'", "") select new { us, std }).FirstOrDefault();
                    List<string> courses = new List<string>();

                    if (u.us.userType == "3")
                    {

                        posts = db.Posts.Where(p => p.status != "deleted" && fromWall == p.fromWall).ToList();
                    }
                    else if (u.us.userType == "2")
                    {
                        courses =
                            db.Allocations.Where(s => s.emp_no == cnic).Select(s => s.course_no).Distinct().ToList();
                        foreach (var item in courses)
                        {
                            if (item != "" && item != null)
                                posts.AddRange(db.Posts.Where(s => s.status != "deleted" && fromWall == s.fromWall && (s.postFor.Contains(item) || s.postFor.ToLower().Contains("all"))).ToList());
                        }
                    }
                    else if (u.us.userType == "1")
                    {
                        //courses= db.Allocations.Where(s => s.section == u.std.section).Select(s => s.course_no).Distinct().ToList() ;

                        // if (item != "" && item != null)
                        posts.AddRange(db.Posts.Where(s => s.status != "deleted" && fromWall == s.fromWall && (s.postedBy==cnic||s.postFor.Contains(u.std.section) || s.postFor.ToLower().Contains("all"))).ToList());

                    }

                    //cnic = cnic.Replace("''");


                }
                else if (fromWall == "2" && u.us.userType == "1")
                { var courses= db.Allocations.Where(s => s.section == u.std.section).Select(s => s.course_no).Distinct().ToList() ;
                    foreach (var item in courses)
                    {
                        if (item != "" && item != null)
                        {
                            posts.AddRange(db.Posts.Where(s => s.status != "deleted" &&s.fromWall=="5" && (s.postFor.Contains(item) || s.postFor.ToLower().Contains("all"))).ToList());

                        }

                    }
                  

                }
                else
                {
                    if (u.std != null)
                        posts = db.Posts.Where(p => p.status != "deleted" && p.fromWall == fromWall && (p.postFor.ToLower().Contains(u.std.aridNo.ToLower()) || p.postFor.ToLower().Contains(u.us.CNIC.ToLower()) ||p.postedBy.Trim()==cnic.Trim()|| p.postFor.ToLower().Contains("all"))).ToList();
                    else
                    {
                        posts = db.Posts.Where(p => p.status != "deleted" && p.fromWall == fromWall && (p.postFor.ToLower().Contains(cnic.ToLower()) || p.postedBy.Trim() == cnic.Trim() || p.postFor.ToLower().Contains("all"))).ToList();

                    }

                }
                    int count = Math.Min(10, posts.Count - startIndex);
                    if (count > 0)
                    {
                        // Return the sublist of posts for the given page
                        posts= posts.GetRange(startIndex, count);
                        for (int i = 0; i < posts.Count; i++)
                        {
                            var us= posts[i].postedBy;
                            var c=JsonConvert.SerializeObject(db.Users.Where(s => s.CNIC == us).FirstOrDefault());
                            if (c != null)
                            {
                            posts[i].user = c;
                            }
                            
                        }
                        posts=posts.OrderByDescending(s=>DateTime.Parse(s.dateTime)).Distinct().ToList();
                        posts1 = getDetail(posts,cnic);

                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "No more posts");

                    }
                    return Request.CreateResponse(HttpStatusCode.OK, posts1);
                

            }
            catch (Exception ex)
            {
                return getExceptionMessage();
            }          
        }
        private List<PostHelper> getDetail(List<Post> posts,string cnic)
        {
            BIITSOCIOEntities db = new BIITSOCIOEntities();
            List<PostHelper> posts1 = new List<PostHelper>();
            try
            {
                var d = (from p in posts
                         join l in db.Reacts.Where(l => l.userid == cnic.Replace("'", ""))
                         on p.id equals l.postId into postLikes
                         from pl in postLikes.DefaultIfEmpty()
                         select new
                         {
                             Post = p,
                             UserLiked = pl != null
                         }).ToList();
                
                foreach (var item in d)
                {


                    string postedBy = item.Post.postedBy;

                    // Query the friendTable to see if there is a record that matches
                    // the postedBy value and the user ID of the current user
                    bool isFriend = db.FriendRequests.Any(f => ((f.RequestedBy == cnic.Replace("'", "") &&
                    f.RequestedTo == postedBy) ||
                    (f.RequestedBy == postedBy &&
                    f.RequestedTo == cnic.Replace("'", ""))) && f.status.ToLower() == "accepted");
                    PostHelper p = new PostHelper();
                    p.isFriend = isFriend;
                    p.isLiked = item.UserLiked;
                    p.isPinned = db.Diaries.Any(s=>s.user_id== cnic.Replace("'", "") && s.post_id==item.Post.id);
                    p.post = item.Post;
                    posts1.Add(p);
                }
            }
            catch (Exception ex) { }
            return posts1;
        }
        [HttpGet]
        public HttpResponseMessage getPinnedPosts(string user_id)
        {
            BIITSOCIOEntities db = new BIITSOCIOEntities();
            try
            {


               List<Post> posts= (from p in db.Posts join d in db.Diaries on p.id equals d.post_id where d.user_id==user_id select p) .ToList();
                List<PostHelper> postHelpers =new List<PostHelper>();
                if (posts.Count != 0)
                {
                    postHelpers = getDetail(posts, user_id);
                }
                return Request.CreateResponse(HttpStatusCode.OK, postHelpers);

            }
            catch (Exception ex)
            {
                return getExceptionMessage();
            }
        }
        [HttpPost]
        public HttpResponseMessage pinPost(Diary diary)
        {
            BIITSOCIOEntities db = new BIITSOCIOEntities();
            try
            {  
                db.Diaries.Add(diary);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Pinned");

            }
            catch (Exception ex)
            {
                return getExceptionMessage();
            }
        }
        public  HttpResponseMessage getExceptionMessage()
        {
            return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something went wrong try Again!");
        }

       
    }
}
