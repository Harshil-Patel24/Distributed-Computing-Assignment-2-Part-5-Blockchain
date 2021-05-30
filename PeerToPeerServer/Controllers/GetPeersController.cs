using PeerToPeerServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace PeerToPeerServer.Controllers
{
    public class GetPeersController : ApiController
    {
        [Route("api/GetPeers")]
        [HttpGet]
        public List<string> GetPeers()
        {
            try
            {
                List<string> peerlist = PeerList.peers;

                // Shuffle up the clients for equal opportunity
                if (peerlist != null && peerlist.Count != 0)
                {
                    peerlist = peerlist.OrderBy(x => Guid.NewGuid()).ToList();
                    PeerList.peers = peerlist;
                }
                return peerlist;
            }
            catch(Exception)
            {
                HttpResponseMessage respEr = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("Bad Input")
                };
                throw new HttpResponseException(respEr);
            }
        }
    }
}