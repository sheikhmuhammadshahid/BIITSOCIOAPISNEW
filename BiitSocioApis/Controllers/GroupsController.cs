using BiitSocioApis.classes;
using BiitSocioApis.Models;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Contexts;

using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using HttpGetAttribute = System.Web.Mvc.HttpGetAttribute;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;

namespace BiitSocioApis.Controllers
{
    public class GroupsController : ApiController
    {
        BIITSOCIOEntities db = new BIITSOCIOEntities();
        [HttpPost]
        public HttpResponseMessage addGroup()
        {
            try
            {
                Group group = new Group();
                HttpRequest request = HttpContext.Current.Request;
                group.name = request["name"];
                if (db.Groups.Any(s => s.name == group.name))
                {
                    var data = new {
                        message = "Group name already exists \n try another one!",
                        statusCode=202,

                    };

                    return Request.CreateResponse(HttpStatusCode.OK, data);
                }
                group.Admin = request["Admin"];
                group.date = DateTime.Now.ToShortDateString();
                group.description = request["description"];

                var d = request["users"];
                List<String> users = d.Split(',').ToList();
                group.isOfficial = bool.Parse(request["isOfficial"]);
                group.memberCount = int.Parse(request["memberCount"]);
                HttpPostedFile imagefile = request.Files["profile"];

                if (imagefile!=null)
                {
                    group.profile = new PostController().saveImage(imagefile, group.name, "Images");
                }
                else {
                    group.profile = ""; }
                db.Groups.Add(group);
                db.SaveChanges();
                int id = db.Groups.OrderByDescending(s => s.id).Select(s=>s.id).FirstOrDefault();
                foreach (var item in users)
                {
                    UserGroup gr = new UserGroup();
                    gr.groupId = id;
                    gr.userId = item;
                    db.UserGroups.Add(gr);
                }
                db.SaveChanges();
                var data1 = new
                {
                    message = "Group created successfully!",
                    statusCode = 200,

                };
                return Request.CreateResponse(HttpStatusCode.OK, data1);
            }
            catch (Exception ex)
            {
                return new PostController().getExceptionMessage();
            }

        }
        [HttpGet]
        public HttpResponseMessage getGroups(String cnic,string userType) {
            try
            {

                List<Group> groups = new List<Group>();
                User user = db.Users.Where(s=>s.CNIC==cnic).FirstOrDefault();
                if (userType == "1")
                {
                    student std = db.students.Where(s => s.cnic == cnic).FirstOrDefault();
                    user.aridNo = std.aridNo;
                    user.section = std.section;
                    
                }
                else if (userType == "2")
                {
                    Teacher std = db.Teachers.Where(s => s.cnic == cnic).FirstOrDefault();
                    //user.isTeachingTo = std.isTeachingTo;
                    
                }
                var res=db.Allocations.Where(s => s.section==user.section|| s.emp_no==user.CNIC).Select(s=>s.course_no).Distinct().ToList();
                groups = (from g in db.Groups join all in res on g.name equals all select g).ToList();
                if(user.userType=="2")
                groups.AddRange(db.Groups.Where(s=>s.name.ToLower().Contains("faculty")).ToList());
                List<Group> result = db.Groups.Where(s=>s.Admin==user.CNIC||s.Admin.Trim()==user.name).ToList();
                groups.AddRange(result);
                result = (from g in db.Groups join gu in db.UserGroups on g.id equals gu.groupId where gu.userId == cnic select g).ToList();
                groups.AddRange(result);
                var results=groups.Distinct();

                
                    return Request.CreateResponse(HttpStatusCode.OK, results);
            
            }
            catch (Exception ex)
            {
                 return new PostController().getExceptionMessage();

            }
        }
        [HttpGet]
        public HttpResponseMessage deleteGroup(Group group)
        {
            try
            {

                db.Groups.Remove(group);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Group removed successfully!");
            }
            catch (Exception ex)
            {
                return new PostController().getExceptionMessage();
            }

        }
        [HttpPost]
        public HttpResponseMessage updateProfile(Group group)
        {
            try
            {

                Group g= db.Groups.Where(s=>s.id==group.id).SingleOrDefault();
                g.profile = group.profile;
                g.Admin = group.Admin;
                g.name = group.name;
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Updated successfully!");
            }
            catch (Exception ex)
            {
                return new PostController().getExceptionMessage();
            }

        }
        /*[HttpPost]
        public HttpResponseMessage addChatOfGroup(int idd,chat c)
        {
            try
            {
                db.chats.Add(c);
                db.SaveChanges();
                int id = db.chats.OrderBy(s => s.id).Select(s => s.id).FirstOrDefault();
                GroupChat chat = new GroupChat();
                chat.groupId= idd;
                chat.chatId = id;
                db.GroupChats.Add(chat);

                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Added successfully!");
            }
            catch (Exception ex)
            {
                return new PostController().getExceptionMessage();
            }



        }*/
        [HttpGet]
        public HttpResponseMessage getChatOfGroup(int id,string loggedInUserId)
        {
            try
            {
                var res = db.chats.Where(s=>s.chat_id==id.ToString()).Select(s=>new {
                    id = s.id,
                    type = s.type,
                    message = s.text,
                    url = s.url,
                    fromFile = false,
                    date = s.Date,
                    dateTime = s.dateTime,
                    sender = s.userid == loggedInUserId ? true : false,
                    senderImage = db.Users.Where(us => us.CNIC == s.userid.Trim()).Select(us => us.profileImage).FirstOrDefault()
                }).ToList();
           
                
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception ex)
            {
                return new PostController().getExceptionMessage();
            }



        }

        [HttpGet]
        public HttpResponseMessage getGroupDetail(int id)
        {
            BIITSOCIOEntities db = new BIITSOCIOEntities();
            try
            {
                Group g = db.Groups.Where(s=>s.id==id).FirstOrDefault();
                List<User> user=new List<User>();
                if (g.isOfficial == true) {
                    if (g.name.ToLower().Contains("facu"))
                    {
                        user = db.Users.Where(s => s.userType == "2").ToList();
                    }
                    else
                    {
                        var res = db.Allocations.Where(s => s.course_no == g.name).Select(s => s.section).Distinct().ToList();
                        foreach (var item in res)
                        {
                            var r = (from std in db.students join us in db.Users on std.cnic equals us.CNIC where std.section == item select us).ToList();
                            user.AddRange(r);
                            user.DistinctBy(s => s.CNIC);
                        }
                    }
                }
                else
                 user = (from ug in db.UserGroups join us in db.Users on ug.userId equals us.CNIC where ug.groupId==id select us ).ToList();
                var admin = db.Users.Where(s=>s.CNIC==g.Admin).FirstOrDefault();
                user.Insert(0,admin);
                for (int i = 0; i < user.Count; i++)
                {
                    var u = user[i];
                    if (u != null)
                    {
                        var v = db.students.Where(s => s.cnic == u.CNIC).FirstOrDefault();
                        if (v != null)
                        {
                            user[i].aridNo = v.aridNo;
                            user[i].section = v.section;

                        }
                    }
                    else {
                        user.RemoveAt(i);
                        i--;
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, user);
            }
            catch (Exception ex)
            {
                return new PostController().getExceptionMessage();
            }



        }


        [HttpPost]
        public HttpResponseMessage deleteChatOfGroup(int idd)
        {
            try
            {
                GroupChat chat = db.GroupChats.Where(s => s.chatId == idd).SingleOrDefault();
                db.GroupChats.Remove(chat);
                db.SaveChanges();
                chat ch= db.chats.Where(s => s.id == idd).SingleOrDefault();
                db.chats.Remove(ch);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Removed successfully!");
            }
            catch (Exception ex)
            {
                return new PostController().getExceptionMessage();
            }



        }
        [HttpPost]
        public HttpResponseMessage addMembers(List<UserGroup> members)
        {
            try
            {
                foreach (UserGroup g in members)
                {
                    db.UserGroups.Add(g);
                }
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Added successfully!");
            }
            catch (Exception ex)
            {
                return new PostController().getExceptionMessage();
            }



        }


    }
}
