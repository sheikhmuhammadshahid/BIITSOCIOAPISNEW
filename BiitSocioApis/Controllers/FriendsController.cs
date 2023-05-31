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
    public class FriendsController : ApiController
    {
        BIITSOCIOEntities db = new BIITSOCIOEntities();
        [HttpPost]
        public HttpResponseMessage sendFriendRequest(FriendRequest friend)
        {
            try
            {

               

                bool isExist = db.FriendRequests.Any(s => s.RequestedTo == friend.RequestedTo && s.RequestedBy == friend.RequestedBy);
                if (!isExist)
                {
                   
                    FriendRequest f = new FriendRequest();
                    f.status = friend.status;
                    f.RequestedBy= friend.RequestedBy;
                    f.RequestedTo= friend.RequestedTo;
                    
                    db.FriendRequests.Add(f);
                    db.SaveChanges();
                    notification n = new notification();
                    n.notificationFrom = friend.RequestedBy;
                    n.NotificationTo = friend.RequestedTo;
                    n.title = "";
                    n.dateTime = DateTime.Now.ToShortDateString();
                    //n.status = "pending";
                    n.status = 0;
                    n.body = db.FriendRequests.OrderByDescending(s => s.id).Select(s => s.id.ToString()).FirstOrDefault();
                    n.fromWall = friend.id;
                    n.type = "request";
                    db.notifications.Add(n);
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "sent successfully!");
                }
                else {
                    return Request.CreateResponse(HttpStatusCode.OK, "Already requested!");
                }
                
            }
            catch (Exception ex)
            {
                return new PostController().getExceptionMessage();
            }

        }
        [HttpGet]
        public HttpResponseMessage acceptFriendRequest(int reques_id,bool status,int noti_id)
        {
            try
            {
                var notification = db.notifications.Where(s => s.id == noti_id).FirstOrDefault();
                if (notification != null)
                {
                    notification.status = 2;
                    db.SaveChanges();
                }
                FriendRequest request= db.FriendRequests.Where(s=>s.id==reques_id).SingleOrDefault();
                if (status)
                    request.status = "accepted";
                else
                {
                    request.status = "rejected";

                }
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, ""+request.status+" successfully!");
            }
            catch (Exception ex)
            {
                return new PostController().getExceptionMessage();
            }

        }
        [HttpGet]
        public HttpResponseMessage unfriend(string sentBy,string sentTo)
        {
            try
            {

                FriendRequest request = db.FriendRequests.Where(s => s.RequestedBy == sentBy && s.RequestedTo==sentTo).FirstOrDefault();
                if (request != null)
                {
                    db.FriendRequests.Remove(request);
                    db.SaveChanges();
                }
                return Request.CreateResponse(HttpStatusCode.OK, "Removed successfully!");
            }
            catch (Exception ex)
            {
                return new PostController().getExceptionMessage();
            }

        }

        [HttpGet]
        public HttpResponseMessage getFriends(string user_id)
        {
            try
            {

                List<String> request = db.FriendRequests.Where(s => s.status.ToLower()=="accepted"&&(s.RequestedBy.Trim() == user_id || s.RequestedTo.Trim() == user_id)).Select(s=>s.RequestedTo.Trim()==user_id?s.RequestedBy.Trim():s.RequestedTo.Trim()).ToList();
                var res = (from us in db.Users join r in request on us.CNIC equals r select us).ToList(); 
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception ex)
            {
                return new PostController().getExceptionMessage();
            }

        }

        [HttpGet]
        public HttpResponseMessage getTotalFriends(string user_id)
        {
            try
            {

               int count = db.FriendRequests.Where(s => s.RequestedBy.Trim() == user_id).ToList().Count;

                return Request.CreateResponse(HttpStatusCode.OK, count);
            }
            catch (Exception ex)
            {
                return new PostController().getExceptionMessage();
            }

        }
        [HttpGet]
        public HttpResponseMessage getFriendRequests(string user_id)
        {
            try
            {

                List<FriendRequest> requests = db.FriendRequests.Where(s => s.RequestedBy.Trim() == user_id && s.status!="Accepted").ToList();

                return Request.CreateResponse(HttpStatusCode.OK, requests);
            }
            catch (Exception ex)
            {
                return new PostController().getExceptionMessage();
            }

        }




    }
}
