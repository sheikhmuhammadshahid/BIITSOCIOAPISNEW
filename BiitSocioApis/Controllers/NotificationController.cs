using BiitSocioApis.classes;
using BiitSocioApis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection.Emit;
using System.Threading;
using System.Web.Http;

namespace BiitSocioApis.Controllers
{
    public class NotificationController : ApiController
    {
        BIITSOCIOEntities db = new BIITSOCIOEntities();
        private void sendNotification(string message,string title,string to)
        { 
        }
        private void checkStatus(int id,string title)
        {
            try {
                //BIITSOCIOEntities db = new BIITSOCIOEntities();
                
                SendSms.SendSmss(fromPhoneNumber: "+15672922944", toPhoneNumber: "+923061523157", message: title);
                
            }
            catch (Exception e) { }
        }
        [HttpPost]
        public HttpResponseMessage addNotification(notification notify)
        {

            try
            {
                //  sendNotification("","","");
               notify.dateTime = DateTime.Now.ToShortDateString();
                notify.type = "official";
               var res= db.notifications.Add(notify);
             
                db.SaveChanges();
                new Thread(() => {
                    checkStatus(res.id,res.body);
                }).Start();
                return Request.CreateResponse(HttpStatusCode.OK, "sent Successfully!");
            }
            catch (Exception ex)
            {
                return new PostController().getExceptionMessage();
            }

        }
        [HttpGet]
        public HttpResponseMessage getNotification(string userId,string fromWall)
        {

            try
            {
                BIITSOCIOEntities db = new BIITSOCIOEntities();
                List<notification> not = new List<notification>();
                var user = db.Users.Where(s => s.CNIC == userId).FirstOrDefault();
                if (user.userType == "1")
                {
                    user.section = db.students.Where(s=>s.cnic==user.CNIC).Select(s=>s.section).FirstOrDefault();
                   var d= db.notifications.Where(s => s.fromWall.ToString() == fromWall && (s.NotificationTo.Contains(user.section))).ToList();
                   not.AddRange(d.Distinct());
                }

                not.AddRange(db.notifications.Where(s => s.fromWall.ToString() == fromWall && (s.NotificationTo.Contains(userId))).ToList());
                not = not.Distinct().ToList();
                var notifications = not.OrderByDescending(n => n.id)
                              .Select(n => new
                              {
                               n,
                              
                               user = db.Users.Where(s=>s.CNIC==n.notificationFrom).Select(s=>new { name=s.name,profileImage=s.profileImage}).FirstOrDefault(),
                              // profileImage = db.Users.Where(s => s.CNIC == userId).Select(s => s.).FirstOrDefault(),
                               postImage = (n.type == "like" || n.type =="newPost") ? db.Posts.Where(s=>s.id==n.post_id).Select(s=>s.user).FirstOrDefault() : null
                             }).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, notifications);
            }
            catch (Exception ex)
            {
                return new PostController().getExceptionMessage();
            }

        }
       

        [HttpGet]
        public HttpResponseMessage deleteNotification(int notification_id)
        {

            try
            {
                notification noti = db.notifications.Where(s => s.id == notification_id).SingleOrDefault();
                db.notifications.Remove(noti);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, "Removed Successfully!");
            }
            catch (Exception ex)
            {
                return new PostController().getExceptionMessage();
            }

        }

        [HttpGet]
        public HttpResponseMessage ReadedNotification(int notification_id)
        {

            try
            {
                notification noti = db.notifications.Where(s => s.id == notification_id).SingleOrDefault();
                noti.status = 1;
                db.SaveChanges();
                
                return Request.CreateResponse(HttpStatusCode.OK, "Readed Successfully!");
            }
            catch (Exception ex)
            {
                return new PostController().getExceptionMessage();
            }

        }
    }
}
