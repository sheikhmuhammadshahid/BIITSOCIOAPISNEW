using BiitSocioApis.classes;
using BiitSocioApis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BiitSocioApis.Controllers
{
    public class ReactsController : ApiController
    {
        BIITSOCIOEntities db = new BIITSOCIOEntities();
        [HttpPost]
        public HttpResponseMessage addReaction(React react)
        {
           
            try
            {
                var user = db.Users.Where(s=>s.CNIC==react.userid).FirstOrDefault();

               // SendSms.SendSmss(fromPhoneNumber: "+15672922944",toPhoneNumber:"+923061523157",message:"You are required to visit Admin!");
                db.Reacts.Add(react);
                var p=db.Posts.Where(s => s.id == react.postId).SingleOrDefault() ;
                if (p != null)
                {
                    if (p.likesCount == null)
                        p.likesCount = 0;
                    p.likesCount++;
                }
                db.SaveChanges();
                
                notification n = new notification();
                n.type = "like";
                n.NotificationTo = p.postedBy;
                n.notificationFrom = react.userid;
                n.status = 0;
                n.post_id=react.postId;
                n.dateTime = DateTime.Now.ToShortDateString();
                n.fromWall = int.Parse(p.fromWall);

                db.notifications.Add(n);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Reacted Successfully!");
            }
            catch (Exception ex)
            {
                return new PostController().getExceptionMessage();
            }

        }
        [HttpGet]
        public HttpResponseMessage getReactions(int post_id)
        {

            try
            {
                List<User> reacts = db.Reacts.AsEnumerable().Where(s => s.postId == post_id).ToList().AsEnumerable().Join(db.Users.AsEnumerable(), re => re.userid, us => us.CNIC, (re, us) =>us).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, reacts);
            }
            catch (Exception ex)
            {
                return new PostController().getExceptionMessage();
            }

        }
        [HttpPost]
        public HttpResponseMessage deleteReact(React react_id)
        {

            try
            {
                React react = db.Reacts.Where(s => s.userid ==react_id.userid && s.postId==react_id.postId ).FirstOrDefault();
                var p = db.Posts.Where(s => s.id == react.postId).SingleOrDefault();
                if (p != null)
                {
                    p.likesCount--;
                }
                db.Reacts.Remove(react);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK, "Removed Successfully!");
            }
            catch (Exception ex)
            {
                return new PostController().getExceptionMessage();
            }

        }
    }
}
