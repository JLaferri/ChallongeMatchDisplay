using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RestSharp;
using System.Xml.Linq;

namespace Fizzi.Libraries.ChallongeApiWrapper
{
    public class ChallongeApiException : Exception
    {
        public RestResponse RestResponse { get; private set; }
        public string[] Errors { get; private set; }

        public ChallongeApiException(RestResponse response) : this(null, response) { }

        public ChallongeApiException(string message, RestResponse response)
            : base(message)
        {
            RestResponse = response;

            XDocument document = null;
            try { document = XDocument.Parse(response.Content); } catch { }

            if (document != null)
            {
                Errors = document.Root.Descendants(XName.Get("error")).Select(x => x.Value).ToArray();
            }
        }
    }
}
