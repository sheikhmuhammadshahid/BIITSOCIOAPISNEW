using BiitSocioApis.classes;
using BiitSocioApis.HelpingClasses;
using BiitSocioApis.Models;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Mvc;
using Twilio.TwiML.Voice;
using HttpGetAttribute = System.Web.Http.HttpGetAttribute;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;

namespace BiitSocioApis.Controllers
{
    public class UserController : ApiController
    {
       
        [HttpPost]
        public HttpResponseMessage LoginUser(User user)
        {
            try
            {
                BIITSOCIOEntities db = new BIITSOCIOEntities();
                User user1;
                student u;
                student student = new student();
                if (user.CNIC.ToLower().Contains("arid"))
                {

                    student = db.students.Where(s => s.aridNo.ToLower().Trim() == user.CNIC.ToLower().Trim()).SingleOrDefault();
                    user1 = db.Users.Where(s => s.CNIC == student.cnic && s.password == user.password).SingleOrDefault();
                    if (user1 != null)
                    {
                        user1.sonOf = student.sonOf;
                        user1.aridNo = student.aridNo;
                        user1.section = student.section;
                    }
                    // if student

                }
                else
                {


                    user1 = db.Users.Where(s => (s.email == user.CNIC || s.CNIC.Trim() == user.CNIC.Trim())).FirstOrDefault();
                    if(user1!=null)
                    if (user1.userType == "4" && user1.password!=user.password)
                    {
                        var vs = db.students.Where(s => s.sonOf == user.CNIC).ToList();
                            if (!vs.Any(s => s.aridNo.ToLower().EndsWith(user.password.ToLower())))
                            {
                                var r = new
                                {
                                    statusCode = 300,
                                    message = "Incorrect Password!"
                                };
                                return Request.CreateResponse(HttpStatusCode.OK, r);
                            }
                            else {
                                user1.password = user.password;
                            }

                    }
                    if (user1 != null)
                    {
                        if (user1.password == user.password)
                        {
                            if (user1.userType == "1")
                            {
                                var v = db.students.Where(s => s.cnic == user1.CNIC).FirstOrDefault();
                                if (v != null)
                                {
                                    user1.aridNo = v.aridNo;
                                    user1.sonOf = v.sonOf;
                                    user1.section = v.section;
                                }
                            }
                            else if (user1.userType == "2")
                            {
                                var v = db.Teachers.Where(s => s.cnic == user1.CNIC).FirstOrDefault();
                                if (v != null)
                                    user1.isTeachingTo = v.isTeachingTo;
                            }
                        }
                    }
                    else
                    {
                        var vs = db.students.Where(s => s.sonOf == user.CNIC).ToList();
                        if (vs.Count != 0)
                        {
                            user1 = new User();
                            user1.userType = "4";
                            user1.CNIC = user.CNIC;
                            user1.password = user.password;
                            user1.name = "Parent";
                            user1.email = "email";
                            user1.phone = "phone";
                            if (!vs.Any(s => s.aridNo.ToLower().EndsWith(user.password.ToLower())))
                            {
                                var r = new
                                {
                                    statusCode = 300,
                                    message = "Incorrect Password!"
                                };
                                return Request.CreateResponse(HttpStatusCode.OK, r);
                            }


                            

                        }
                        else
                        {
                            var r = new
                            {
                                statusCode = 300,
                                message = "User not found!"
                            };
                            return Request.CreateResponse(HttpStatusCode.OK, r);
                        }
                    }
                }
                if (user1 != null)
                {
                    if (user1.userType == "4") {
                        var result = checkParentIfAdded(user1);
                    }
                    int count = db.Posts.Where(s => s.postedBy == user1.CNIC).Count();
                    int countFriends = db.FriendRequests.Where(s => (s.RequestedTo == user1.CNIC || s.RequestedBy == user1.CNIC) && s.status.ToLower() == "accepted").Count();
                    var res = new
                    {
                        user = user1,
                        postsCount = count,
                        countFriends = countFriends,
                        
                        statusCode=200
                    };
                   
                        db.Users.Where(s => s.CNIC == user1.CNIC).FirstOrDefault().lastLogged = DateTime.Now.ToString();
                        db.SaveChanges();
                    
                    
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    user1 = db.Users.Where(s => s.CNIC == user.CNIC).SingleOrDefault();
                    if (user1 != null ||student!=null)
                    {
                        var r = new {
                            statusCode = 300,
                            message = "Incorrect Password!"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK,r);
                    }
                    else
                    {
                        var r = new
                        {
                            statusCode = 300,
                            message = "User not found!"
                        };
                        return Request.CreateResponse(HttpStatusCode.OK,r);

                    }
                }
            }
            catch (Exception ex)
            {
                var r = new
                {
                    statusCode = 300,
                    message = "User not found!"
                };
                return Request.CreateResponse(HttpStatusCode.OK, r);

            }
        }
      private bool  checkParentIfAdded(User user) {
            try
            {
                BIITSOCIOEntities db = new BIITSOCIOEntities();
                if (!db.Users.Any(s => s.CNIC == user.CNIC)) {
                    db.Users.Add(user);
                    db.SaveChanges();
                    return true;
                }

            }
            catch (Exception ex)
            {

                
            }
            return false;
        }
        private bool saveImageInFolder(string f)
        {
            return true;
        }
        [HttpPost]
        public HttpResponseMessage saveTas() {
            try
            {
                BIITSOCIOEntities db = new BIITSOCIOEntities();
                HttpRequest request = HttpContext.Current.Request;
                string students = request["students"];
                string teacher = request["teacher"];
                List<String> list = students.Split(',').ToList();
                foreach (var item in list)
                {
                    if (item != "")
                    {
                        var data=db.Users.Where(s => s.CNIC.Trim() == item.Trim()).FirstOrDefault();
                        if (data != null)
                        {
                            data.TAOf = teacher;
                        }

                    }
                }
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, "saved!");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "try again!");
            }
        }
        public HttpResponseMessage getTeachersAndStudents() {
            try
            {
                BIITSOCIOEntities db = new BIITSOCIOEntities();
                
                var data = new DropDownClass();
                data.users = db.Users.AsEnumerable().Where(s => s.userType == "1").
                    Join(db.students.AsEnumerable().ToList(), us => us.CNIC, std => std.cnic, (us, std) => new User{
                    CNIC=us.CNIC,
                    name=us.name,
                    userType=us.userType    ,
                    profileImage=us.profileImage,
                    TAOf=us.TAOf,
                    CrOf=us.CrOf,
                 aridNo=std.aridNo,
                
                }).ToList();

                data.isString = false;
                data.category = "Students";
                data.data = new List<string>();
                var data1 = new {
                    students =data,
                    teachers=db.Users.Where(s=>s.userType=="2").Select(s=>s.CNIC).Distinct().ToList()
                };
                return Request.CreateResponse(HttpStatusCode.OK, data1);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError,"try again!");
            }
        }
        [HttpGet]
        public HttpResponseMessage getDescipline(string cnic,string fromWall)
        {
            try
            {
                BIITSOCIOEntities db = new BIITSOCIOEntities();
                var user = db.Users.Where(s=>s.CNIC.ToLower()==cnic.ToLower()).FirstOrDefault();
                if (user.userType == "1") {
                   
                  
                    user.section = db.students.Where(s => s.cnic == user.CNIC).Select(s => s.section).FirstOrDefault();
                }
                List<DropDownClass> data1 = new List<DropDownClass>();
                List<string> sec = new List<string>();//db.Timetables.GroupBy(s=>s.section).Select(s=>s.Key).ToList();
                List<string> desc = new List<string>();//sec.Where(s=>s!=""&& s != null).Select(s => new {des =s.Split('-')[0] }).Select(s=>s.des).Distinct().ToList();
                if (fromWall == "6")
                {
                    var dd = new DropDownClass();
                    dd.category = "Societies";
                    if (user.userType != "3")
                    {
                        dd.users = (db.Societies.AsEnumerable().Join(db.JoinedSocieties.AsEnumerable().Where(s => s.userId == cnic).ToList(), so => so.id, js => js.societyId, (so, js) => new User
                        {
                            CNIC = so.id.ToString(),
                            name = so.name,
                            userType = "6",
                            profileImage = so.profileImage,


                        })).ToList();
                    }
                    else {
                        dd.users = (db.Societies.AsEnumerable().Select(so => new User
                        {
                            CNIC = so.id.ToString(),
                            name = so.name,
                            userType = "6",
                            profileImage = so.profileImage,


                        })).ToList();
                    }
                    dd.isString = false;
                    dd.data = new List<string>();
                    data1.Add(dd);
                }
                else if (user.userType != "1" && user.userType != "4")
                {
                    var dd = new DropDownClass();
                    if (fromWall == "5")
                    {

                        dd.category = "Courses";
                        dd.data = user.userType == "3" ? db.Allocations.Select(s => s.course_no.Trim()).Distinct().ToList() : db.Allocations.Where(s => s.emp_no == cnic).Select(s => s.course_no.Trim()).Distinct().ToList();
                        dd.isString = true;
                        dd.users = new List<User>();
                        data1.Add(dd);
                        dd = new DropDownClass();
                        dd.category = "Sections";
                        dd.data = db.Allocations.Where(s => s.emp_no == cnic).Select(s => s.section.Trim()).Distinct().ToList();
                        dd.isString = true;
                        dd.users = new List<User>();
                        data1.Add(dd);
                    }
                    else if (fromWall == "1")
                    {
                        dd = new DropDownClass();
                        dd.category = "Students";
                        dd.users = db.Users.Where(s => s.userType == "1" &&s.CNIC!=cnic).ToList();
                        for (int i = 0; i < dd.users.Count; i++)
                        {
                            string cnc = dd.users[i].CNIC.Trim();
                            dd.users[i].aridNo = db.students.Where(s => s.cnic == cnc).Select(s => s.aridNo).FirstOrDefault();

                        }
                        dd.isString = false;
                        dd.data = new List<string>();
                        data1.Add(dd);
                    }
                    else if(fromWall=="2"){
                      
                        dd = new DropDownClass();
                        dd.category = "Teachers";
                        dd.users = db.Users.Where(s => s.userType == "2" && s.CNIC!=cnic).ToList();
                        dd.isString = false;
                        dd.data = new List<string>();
                        data1.Add(dd);
                        if (user.userType == "2")
                        {
                            List<String> request = db.FriendRequests.Where(s => s.status.ToLower() == "accepted" && (s.RequestedBy.Trim() == cnic || s.RequestedTo.Trim() == cnic)).Select(s => s.RequestedTo.Trim() == cnic ? s.RequestedBy.Trim() : s.RequestedTo.Trim()).ToList();

                            dd = new DropDownClass();
                            dd.category = "Friends";

                            dd.users = db.Users.AsEnumerable().Join(request, us => us.CNIC, r => r, (us, r) => new User
                            {
                                aridNo = db.students.AsEnumerable().Where(s => s.cnic == r).Select(s => s.aridNo).FirstOrDefault(),
                                CNIC = r,
                                email = us.email,
                                name = us.name,
                                profileImage = us.profileImage,
                                phone = us.phone,
                                section = us.section,
                                userType = us.userType,
                                TAOf = us.TAOf,
                            }).Where(s=>s.CNIC!=cnic).ToList();
                            dd.isString = false;
                            dd.data = new List<string>();
                            data1.Add(dd);
                            dd = new DropDownClass();
                            dd.category = "Parents";
                            dd.users = db.students.AsEnumerable().Where(s => s.sonOf != null).Select(m => new User
                            {
                                CNIC = m.sonOf,
                                profileImage = null,
                                name = "Parent",
                                userType = "4",

                            }).Distinct().ToList();
                            dd.isString = false;
                            dd.data = new List<string>();
                            data1.Add(dd);
                           
                        }
                    }
                    else
                    {
                        dd = new DropDownClass();
                        dd.category = "Sections";
                        dd.data = user.userType == "3" ? db.Timetables.GroupBy(s => s.section).Select(s => s.Key).ToList() : db.Allocations.Where(s => s.emp_no == cnic).Select(s => s.section.Trim()).Distinct().ToList();
                        dd.isString = true;
                        dd.users = new List<User>();
                        data1.Add(dd);
                        dd = new DropDownClass();
                        dd.category = "Teachers";
                        dd.users = db.Users.Where(s => s.userType == "2" && s.CNIC != cnic).ToList();
                        dd.isString = false;
                        dd.data = new List<string>();
                        data1.Add(dd);
                        dd = new DropDownClass();
                        dd.category = "Students";
                        dd.users = db.Users.Where(s => s.userType == "1" && s.CNIC != cnic).ToList();
                        for (int i = 0; i < dd.users.Count; i++)
                        {
                            string cnc = dd.users[i].CNIC.Trim();
                            dd.users[i].aridNo = db.students.Where(s => s.cnic == cnc).Select(s => s.aridNo).FirstOrDefault();

                        }
                        dd.isString = false;
                        dd.data = new List<string>();
                        data1.Add(dd);
                        dd = new DropDownClass();
                        dd.category = "Parents";
                        dd.users = db.students.AsEnumerable().Where(s => s.sonOf != null).Select(m => new User
                        {
                            CNIC = m.sonOf,
                            profileImage = null,
                            name = "Parent",
                            userType = "4",

                        }).Distinct().ToList();
                        dd.isString = false;
                        dd.data = new List<string>();
                        data1.Add(dd);
                    }
                    
                }
                else if(user.userType=="1") {
                    if (fromWall == "5")
                    {
                        var dd = new DropDownClass();
                        if ( user.TAOf != null)
                        {
                            dd.category = "Courses";
                            dd.data = db.Allocations.Where(s => s.emp_no == user.TAOf && s.section!=""&&s.section!=null).Select(s => s.section.Trim()).Distinct().ToList();
                            dd.isString = true;
                            dd.users = new List<User>();
                            data1.Add(dd);
                        }
                        dd = new DropDownClass();
                        dd.category = "Section";
                        dd.data = db.students.Where(s => s.cnic == cnic).Select(s => s.section).ToList();//db.Allocations.Where(s => s.section == user.section).Select(s => s.course_no.Trim()).Distinct().ToList();
                        dd.isString = true;
                        dd.users = new List<User>();
                        data1.Add(dd);
                    }
                    else { 
                    List<String> request = db.FriendRequests.Where(s => s.status.ToLower() == "accepted" && (s.RequestedBy.Trim() == cnic || s.RequestedTo.Trim() == cnic)).Select(s => s.RequestedTo.Trim() == cnic ? s.RequestedBy.Trim() : s.RequestedTo.Trim()).ToList();
                   
                    var dd = new DropDownClass();
                    dd.category = "Friends";

                    dd.users = db.Users.AsEnumerable().Join(request, us => us.CNIC, r => r, (us, r) => new User{
                        aridNo = db.students.AsEnumerable().Where(s => s.cnic == r).Select(s => s.aridNo).FirstOrDefault(),
                        CNIC = r,
                        email = us.email,
                        name = us.name,
                        profileImage = us.profileImage,
                        phone = us.phone,
                        section = us.section,
                        userType = us.userType,
                        TAOf = us.TAOf,
                    }).Where(s=>s.CNIC!=cnic).ToList();
                   /* dd.users =  (from us in db.Users join r in request on us.CNIC equals r select new User{ 
                           aridNo=db.students.AsEnumerable().Where(s=>s.cnic==r).Select(s=>s.aridNo).FirstOrDefault(),
                           CNIC=r,
                           email=us.email,
                           name=us.name,
                           profileImage = us.profileImage,
                           phone = us.phone,
                           section = us.section,
                           userType=us.userType,
                           TAOf=us.TAOf,
                           
                    }).AsEnumerable().ToList();*/
                    //db.Allocations.Where(s => s.section == user.section).Select(s => s.course_no.Trim()).Distinct().ToList();
                    dd.isString = false;
                    dd.data = new List<string>();
                    data1.Add(dd);
                    }
                }
               
                return Request.CreateResponse(HttpStatusCode.OK, data1);

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something went wrong try Again!");

            }
        }
        /*[HttpPost]
        public HttpResponseMessage saveUser(clients c) {
            try
            {
                ConnectedClients.addClient(c.user_id,c.socket);
                return Request.CreateResponse(HttpStatusCode.OK,"added");
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something went wrong try Again!");

            }
        }*/
        [HttpGet]
        public HttpResponseMessage getDateSheetsTypes() {

            try
            {
                BIITSOCIOEntities db = new BIITSOCIOEntities();
                List<string> data = db.DateeSheets.Select(s => s.examType).Distinct().ToList();
               return Request.CreateResponse(HttpStatusCode.OK, data);
            }
            catch (Exception ex)
            {
               return Request.CreateResponse(HttpStatusCode.InternalServerError,"Try again!");
            }
        }
        [HttpGet]
        public HttpResponseMessage getDateSheet(string cnic,string examType) {

            try
            {
                
                BIITSOCIOEntities db = new BIITSOCIOEntities();
                var resu = db.DateeSheets.OrderByDescending(s => s.id).FirstOrDefault();
                if (examType == null)
                {
                    examType = resu.examType;
                }

                var user = db.Users.Where(s=>s.CNIC==cnic.Trim()).FirstOrDefault();
                if (user.userType == "1")
                {
                    string section = db.students.Where(s => s.cnic == cnic).Select(s => s.section).FirstOrDefault();
                    string sec = section.Split('-')[0].Trim().ToLower().Replace("b", "");
                    string sem = section.Split('-')[1].Trim()[0].ToString();
                    var res = db.DateeSheets.Where(s =>s.examType.Contains(examType)&& s.section.ToLower().Contains(sec) && s.section.ToLower().Contains(sem)).ToList();
                    var re = res.GroupBy(s => s.Time).ToList().OrderBy(s => s.Key).Select(s => new { time = s.Key, dateSheet = s.ToList(), venue = s.ToList()[0].venue,days= s.Select(m => m.day.Trim()).ToList().Distinct() });
                    return Request.CreateResponse(HttpStatusCode.OK, re);
                }
                else 
                {   

                    var res = db.DateeSheets.Where(s=>s.examType.Contains(examType)).ToList();
                    var re = res.GroupBy(s => s.Time).ToList().OrderBy(s => s.Key).Select(s => new { time = s.Key, dateSheet = s.ToList().DistinctBy(m => m.paper.Trim()), venue = s.ToList().ToList()[0].venue , days = s.Select(m=>m.day.Trim()).ToList().Distinct() });
                    return Request.CreateResponse(HttpStatusCode.OK, re);
                }
                

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something went wrong try Again!");

            }
        }
       /* [HttpGet]
        public HttpResponseMessage getDateSheetByExamType(string cnic,string examType)
        {

            try
            {
                BIITSOCIOEntities db = new BIITSOCIOEntities();
                var user = db.Users.Where(s => s.CNIC == cnic.Trim()).FirstOrDefault();
                if (user.userType == "1")
                {
                    string section = db.students.Where(s => s.cnic == cnic).Select(s => s.section).FirstOrDefault();
                    string sec = section.Split('-')[0].Trim().ToLower().Replace("b", "");
                    string sem = section.Split('-')[1].Trim()[0].ToString();
                    var res = db.DateeSheets.Where(s => s.section.ToLower().Contains(sec) && s.section.ToLower().Contains(sem));
                    var re = res.GroupBy(s => s.Time).ToList().OrderBy(s => s.Key).Select(s => new { time = s.Key, dateSheet = s.ToList(), venue = s.ToList()[0].venue, days = s.Select(m => m.day.Trim()).ToList().Distinct() });
                    return Request.CreateResponse(HttpStatusCode.OK, re);
                }
                else
                {

                    var res = db.DateeSheets.ToList();
                    var re = res.GroupBy(s => s.Time).ToList().OrderBy(s => s.Key).Select(s => new { time = s.Key, dateSheet = s.ToList().DistinctBy(m => m.paper.Trim()), venue = s.ToList().ToList()[0].venue, days = s.Select(m => m.day.Trim()).ToList().Distinct() });
                    return Request.CreateResponse(HttpStatusCode.OK, re);
                }


            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something went wrong try Again!");

            }
        }*/
        [HttpGet]
        public HttpResponseMessage getNotificatinosData(string cnic,string fromWall) {
            try
            {

                BIITSOCIOEntities db = new BIITSOCIOEntities();
                var user = db.Users.Where(s => s.CNIC == cnic).FirstOrDefault() ;
                if (user.userType == "1")
                {
                    user.section = db.students.Where(s => s.cnic==cnic).Select(s=>s.section).FirstOrDefault();
                    }
                DateTime dt = DateTime.Now;
                string targetDateTimeString = DateTime.Now.AddMinutes(-2).ToString("yyyy-MM-dd HH:mm:ss");

                var posts = db.Posts.AsEnumerable()
                    .Where(p => DateTime.Parse(p.dateTime) >= DateTime.Parse(targetDateTimeString) &&
                                DateTime.Parse(p.dateTime) < DateTime.Now)
                    .ToList();
                int classCount = 0;
                int biitCount = 0;
                int teacherCount = 0;
                if (user.userType == "1")
                {
                    classCount=posts.Where(s=>s.fromWall=="5"&&s.postFor.ToLower().Contains(user.section.ToLower())).ToList().Count();
                    var courses = db.Allocations.Where(s => s.section == user.section).Select(s=>s.course_no).Distinct().ToList();
                    List<Post> dummy = new List<Post>();
                    foreach (var item in courses)
                    {
                        dummy.AddRange(posts.Where(s => s.fromWall == "5" && s.postFor.ToLower().Contains(item.ToLower())).ToList());

                    }
                    teacherCount = dummy.DistinctBy(s=>s.id).ToList().Count();
                    biitCount=posts.Where(s=> s.fromWall=="3"&&( s.postFor.ToLower().Contains(user.section.ToLower())|| s.postFor.ToLower().Contains(cnic.ToLower()))).ToList().Count();
                }


                var data = new{
                 notificationsCount =db.notifications.AsEnumerable().Where(s=> (DateTime.Parse(s.dateTime) >= DateTime.Parse(targetDateTimeString) &&
                                DateTime.Parse(s.dateTime) < DateTime.Now) &&s.fromWall.ToString()==fromWall && s.NotificationTo.Contains(cnic)).ToList().Count() ,
                 classPostsCount = user.userType == "1" ?classCount: posts.Where(s => s.fromWall == "5" ).ToList().Count(),
                 personalCount =  posts.Where(s=>s.fromWall==user.userType&& s.postFor.ToLower().Contains(cnic.ToLower())).ToList().Count(),
                 teacherCount = user.userType=="1"?teacherCount :posts.Where(s => s.fromWall == "2" &&s.postFor.ToLower().Contains(cnic.ToLower())).ToList().Count(),
                 societiesCount = posts.Where(s => s.fromWall == "6").ToList().Count(),
                 biitCount =user.userType=="1"?biitCount: posts.Where(s => s.fromWall == "3"&&s.postFor.Contains(cnic)).ToList().Count(),
                 studentCount = posts.Where(s => s.fromWall == "1").ToList().Count(),
                };

                return Request.CreateResponse(HttpStatusCode.OK, data);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something went wrong try Again!");
            }
        }
        [HttpGet]
        public HttpResponseMessage getDeviceToken(string cnic)
        {
            try
            {
                BIITSOCIOEntities db = new BIITSOCIOEntities();
                var user = db.Users.Where(s => s.CNIC == cnic).FirstOrDefault();
                
                return Request.CreateResponse(HttpStatusCode.OK, user.token);


            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something went wrong try Again!");

            }
        }

        [HttpGet]
        public HttpResponseMessage updateToken(string token,string cnic)
        {
            try
            {
                BIITSOCIOEntities db = new BIITSOCIOEntities();
                var user = db.Users.Where(s=>s.CNIC==cnic).FirstOrDefault();
                user.token = token;
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "updated");


            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something went wrong try Again!");

            }
        }

        [System.Web.Http.HttpGet]
        public HttpResponseMessage getAllUsers(int pageNo,string cnic,string fromWall)
        {
            try
            {
                BIITSOCIOEntities db = new BIITSOCIOEntities();
                var result = db.Users.Where(s=>s.CNIC!=cnic && s.userType==fromWall).ToList();
              /*  bool isFriend = db.FriendRequests.Any(f => ((f.RequestedBy == cnic &&
                      f.RequestedTo == cnic)&& f.status.ToLower() == "accepted"));*/
                int startIndex = (pageNo - 1) * 10;
                int count = Math.Min(10, result.Count - startIndex);
                if (count > 0)
                {
                    // Return the sublist of posts for the given page
                    //result = result.GetRange(startIndex, count);
                    List<UserHelper> users = new List<UserHelper>();
                    result = result.GetRange(startIndex,count);
                    foreach (var item in result)
                    {
                        UserHelper u = new UserHelper();
                        u.user= item;
                        u.user.section = u.user.userType == "1" ? db.students.Where(s => s.cnic == item.CNIC).Select(s => s.section).FirstOrDefault() : "";
                        u.isFriend=  db.FriendRequests.Any(s => (s.RequestedBy.Trim() == cnic.Trim() || s.RequestedTo.Trim() == cnic.Trim()) && s.status.ToLower() == "accepted");
                        u.postCount = db.Posts.Where(s => s.postedBy == item.CNIC).Count();
                        u.countFriends= (db.FriendRequests.Where(s => (s.RequestedBy.Trim() == item.CNIC.Trim() || s.RequestedTo.Trim() == item.CNIC.Trim()) && s.status.ToLower() == "accepted")).Count();

                        users.Add(u);
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, users);

                }
                else
                {

                    return Request.CreateResponse(HttpStatusCode.OK, "No more users");

                }

            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something went wrong try Again!");

            }
        }

        [System.Web.Http.HttpGet]
        public HttpResponseMessage getUser(string friendof,string friend) {
            try
            {
                BIITSOCIOEntities db = new BIITSOCIOEntities();
                var result = db.Users.Where(s => s.CNIC == friend).FirstOrDefault();
                bool isFriend = db.FriendRequests.Any(f => ((f.RequestedBy == friendof.Replace("'", "") &&
                      f.RequestedTo == friend) ||
                      (f.RequestedBy == friend &&
                      f.RequestedTo == friendof.Replace("'", ""))) && f.status.ToLower() == "accepted");
                int count = db.Posts.Where(s=>s.postedBy==friend).Count();
                int countFriends = db.FriendRequests.Where(s=>(s.RequestedTo==friend||s.RequestedBy==friend) && s.status.ToLower()=="accepted").Count();
                var res = new { 
                    user=result,
                    postsCount=count,
                    isFriend=isFriend,
                    countFriends=countFriends,
                };
                return Request.CreateResponse(HttpStatusCode.OK,res);
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something went wrong try Again!");

            }
        }

        
    }
}
