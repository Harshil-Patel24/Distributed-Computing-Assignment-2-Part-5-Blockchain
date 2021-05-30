using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PeerToPeerServer.Models;

namespace PeerToPeerServer.Controllers
{
    public class AddPeerController : ApiController
    {
        [Route("api/AddPeer")]
        [HttpPost]
        public void AddPeer([FromBody] string encodedIP)
        {
            if (String.IsNullOrEmpty(encodedIP))
            {
                HttpResponseMessage respEr = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("Bad Input")
                };
                throw new HttpResponseException(respEr);
            }
            else
            {
                PeerList.peers.Add(encodedIP);
            }
        }
    }
}