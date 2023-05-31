using BiitSocioApis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BiitSocioApis.Controllers
{
    public class EventController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage addEvent(Event evnt)
        {
            try
            {
                BIITSOCIOEntities entities = new BIITSOCIOEntities();
                evnt = entities.Events.Add(evnt);
                
                entities.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, evnt);
            }
            catch (Exception ex) {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something gone wrong");
            }
            
        }
        [HttpGet]
        public HttpResponseMessage getEvent()
        {
            try
            {
                BIITSOCIOEntities entities = new BIITSOCIOEntities();
                var events = entities.Events.ToList();

                return Request.CreateResponse(HttpStatusCode.OK, events);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something gone wrong");
            }

        }
        [HttpGet]
        public HttpResponseMessage deleteEvent(int id)
        {
            try
            {
                BIITSOCIOEntities entities = new BIITSOCIOEntities();
                Event events = entities.Events.Find(id);
                entities.Events.Remove(events);
                entities.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, events);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something gone wrong");
            }

        }
        [HttpPost]
        public HttpResponseMessage updateEvent(Event eventt)
        {
            try
            {
                BIITSOCIOEntities entities = new BIITSOCIOEntities();
                Event events = entities.Events.Find(eventt.id);
                events.endDate=eventt.endDate;
                events.startDate=eventt.startDate;
                events.Name = eventt.Name;
                entities.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "updated successfully!");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Something gone wrong");
            }

        }
    }
}
