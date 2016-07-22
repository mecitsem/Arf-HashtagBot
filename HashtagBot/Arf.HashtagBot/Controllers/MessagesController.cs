using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using Arf.Services;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace Arf.HashtagBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            HttpResponseMessage response;
            var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
            try
            {

                if (activity.Type.ToLowerInvariant().Equals(ActivityTypes.Message.ToLowerInvariant()))
                {


                    //// calculate something for us to return
                    var length = (activity.Text ?? string.Empty).Length;
                    if (length == 0) return null;
                    var imgPath = activity.Attachments != null && activity.Attachments.Count > 0 ? activity.Attachments.First().ContentUrl : activity.Text;
                    var isUpload = imgPath != null && !imgPath.StartsWith("http");

                    //var reply1 = activity.CreateReply(imgPath);
                    //await connector.Conversations.ReplyToActivityAsync(reply1);
                    var service = new VisionService();
                    var analysisResult = isUpload
                        ? await service.UploadAndDescripteImage(imgPath)
                        : await service.DescripteUrl(imgPath);


                    var reply2 = activity.CreateReply(string.Join(" ", analysisResult.Description.Tags.Select(s => "#" + s)));
                    await connector.Conversations.ReplyToActivityAsync(reply2);
                }
                else
                {
                    HandleSystemMessage(activity);
                }
                response = Request.CreateResponse(HttpStatusCode.Accepted);
            }
            catch (Exception ex)
            {
                var reply = activity.CreateReply($"Error : '{ex.Message}'");
                await connector.Conversations.ReplyToActivityAsync(reply);
                response = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }

            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}