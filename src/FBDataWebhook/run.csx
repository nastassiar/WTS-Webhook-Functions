#r "Newtonsoft.Json"

using System.Net;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, IAsyncCollector<string> output, TraceWriter log)
{
    if (req.Method == HttpMethod.Get)
    {
        log.Info("Handling a new subscription request.");
        // handle new subscription request
        var query = req.GetQueryNameValuePairs();
        string mode = query
            .FirstOrDefault(q => string.Compare(q.Key, "hub.mode", true) == 0)
            .Value;
        string challenge = query
            .FirstOrDefault(q => string.Compare(q.Key, "hub.challenge", true) == 0)
            .Value;
        string verify = query
            .FirstOrDefault(q => string.Compare(q.Key, "hub.verify_token", true) == 0)
            .Value;

        // If verification matches, return challenge
        if (verify == GetEnvironmentVariable("FB_Verify_Token"))
        {
            log.Info("Get received. Token OK. New subscription created");
            var resp = new HttpResponseMessage(HttpStatusCode.OK);
            resp.Content = new StringContent(challenge, System.Text.Encoding.UTF8, "text/plain");
            return resp;
        }
        else
        {
            log.Info("Get received. Token not good! : "+ verify);
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }
    }
    else if (req.Method == HttpMethod.Post) 
    {
        // Just pass on the content to the queue to ve parsed
        string jsonContent = await req.Content.ReadAsStringAsync();

        // Put on queue
         await output.AddAsync(jsonContent);

        return req.CreateResponse(HttpStatusCode.OK);
    }
    else 
    {
        // Shouldn't have made it here! Something went wrong :(
        log.Info("Invalid Request Method");
        return req.CreateResponse(HttpStatusCode.BadRequest);

    }
}

public static string GetEnvironmentVariable(string name)
{
    return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
}
