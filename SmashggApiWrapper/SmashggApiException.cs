using System;
using System.Linq;
using System.Xml.Linq;
using RestSharp;

namespace Fizzi.Libraries.SmashggApiWrapper;

public class SmashggApiException : Exception
{
	public RestResponse RestResponse { get; private set; }

	public string[] Errors { get; private set; }

	public SmashggApiException(RestResponse response)
		: this(null, response)
	{
	}

	public SmashggApiException(string message, RestResponse response)
		: base(message)
	{
		RestResponse = response;
		XDocument xDocument = null;
		try
		{
			xDocument = XDocument.Parse(response.Content);
		}
		catch
		{
		}
		if (xDocument != null)
		{
			Errors = (from x in xDocument.Root.Descendants(XName.Get("error"))
				select x.Value).ToArray();
		}
	}
}
