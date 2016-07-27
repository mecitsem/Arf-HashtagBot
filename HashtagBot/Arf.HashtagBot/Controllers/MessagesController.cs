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
            //HttpResponseMessage response;
            var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
            try
            {

                if (activity.Type.ToLowerInvariant().Equals(ActivityTypes.Message.ToLowerInvariant()))
                {
                    string imgPath = string.Empty;
                    bool isUpload = false;

                    Relpy(connector, activity, "I'm working on it. Please wait!");

                    if (activity.Attachments != null && activity.Attachments.Count > 0)
                    {
                        imgPath = activity.Attachments.First().ContentUrl;
                    }
                    else if (string.IsNullOrEmpty(activity.Text))
                    {
                        imgPath = activity.Text.StartsWith("http") || activity.Text.StartsWith("https") ? activity.Text : string.Empty;
                    }

                    //Check ImagePath
                    if (string.IsNullOrEmpty(imgPath))
                    {
                        Relpy(connector, activity, "I'm sorry this isn't Image or ImageUrl.");
                    }
                    else
                    {
                        //Microsoft Vision Service API
                        var service = new VisionService();

                        //Working Message
                        //Relpy(connector, activity, "Almost done!");

                        var analysisResult = isUpload
                            ? await service.UploadAndDescripteImage(imgPath)
                            : await service.DescripteUrl(imgPath);

                        //Send Succcess Message
                        Relpy(connector, activity, $"Here you go! Hmmm. Let me think about that {analysisResult.Description.Captions.FirstOrDefault().Text}. :)");

                        var reply = activity.CreateReply("Tags: " + string.Join(" ", analysisResult.Description.Tags.Select(s => "#" + s)));

                        await connector.Conversations.ReplyToActivityAsync(reply);
                    }
                }
                else
                {
                    HandleSystemMessage(activity);
                }
                //response = Request.CreateResponse(HttpStatusCode.Accepted);
            }
            catch (Exception ex)
            {
                RelpyAsync(connector, activity, "Something went wrong. Please try again!");
                //response = Request.CreateResponse(HttpStatusCode.Accepted);
            }

            return Request.CreateResponse(HttpStatusCode.Accepted);
        }

        private async void RelpyAsync(ConnectorClient connector, Activity activity, string message)
        {
            if (connector != null && activity != null && !string.IsNullOrEmpty(activity.Id))
            {
                var reply = activity.CreateReply(message);
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
        }

        private void Relpy(ConnectorClient connector, Activity activity, string message)
        {
            if (connector != null && activity != null && !string.IsNullOrEmpty(activity.Id))
            {
                var reply = activity.CreateReply(message);
                connector.Conversations.ReplyToActivity(reply);
            }
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