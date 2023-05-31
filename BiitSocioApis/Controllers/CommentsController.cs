using BiitSocioApis.classes;
using BiitSocioApis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Web.Http;

namespace BiitSocioApis.Controllers
{
    public class CommentsController : ApiController
    {
        BIITSOCIOEntities db = new BIITSOCIOEntities();
        
        [HttpPost]
        public HttpResponseMessage addComment(commentsOn comments)
        {
           
                try
                {
               
              
                comments.likeCount = 0;
                db.commentsOns.Add(comments);
                var p = db.Posts.Where(s => s.id == comments.postId).SingleOrDefault();
                if (p != null)
                {
                    if (p.CommentsCount == null)
                        p.CommentsCount = 0;
                    p.CommentsCount++;
                }
                db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, "Comment Added Successfully!");
                }
                catch (Exception ex)
                {
                    return new PostController().getExceptionMessage();
                }
  
        }
        [HttpGet]
        public HttpResponseMessage getComment(int post_id)
        {

            try
            {
                var data = db.commentsOns.Where(s => s.postId == post_id )
                    .Select(s => new
                    {
                        userData = db.Users.Where(d => d.CNIC == s.userid).Select(d => new { name = d.name, profileImage = d.profileImage }).FirstOrDefault(),
                        likeCount = s.likeCount,
                        
                        comment = s.text,
                        isCommentReply = !(s.repliedOn==0||s.repliedOn==null),
                        time = s.dateTime,
                        id = s.id
                    }).ToList();

                //var data = db.commentsOns.Where(s => s.postId == post_id && (s.repliedOn == null || s.repliedOn == 0));




                return Request.CreateResponse(HttpStatusCode.OK, data.OrderBy(s=>s.time));
            }
            catch (Exception ex)
            {
                return new PostController().getExceptionMessage();
            }

        }
        [HttpPost]
        public HttpResponseMessage deleteComment(commentsOn comment_id)
        {

            try
            {
                commentsOn comments=db.commentsOns.Where(s => s.userid == comment_id.userid && s.postId == comment_id.postId).FirstOrDefault();
                var p = db.Posts.Where(s => s.id == comment_id.postId).SingleOrDefault();
                if (p != null)
                {
                    p.CommentsCount--;
                }
                db.commentsOns.Remove(comments);
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
