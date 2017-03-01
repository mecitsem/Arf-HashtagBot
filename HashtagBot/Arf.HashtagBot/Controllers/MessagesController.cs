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
using System.Net.Http.Headers;

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
                    string imgUrl = string.Empty;
                    bool isUpload = false;
                    byte[] attachmentData = null;
                    Relpy(connector, activity, "I'm working on it. Please wait!");

                    var attachment = activity.Attachments?.FirstOrDefault();

                    if (attachment?.ContentUrl != null)
                    {
                        //imgPath = activity.Attachments?.First().ContentUrl;
                        //isUpload = true;

                        //For skype
                        var token = await (connector.Credentials as MicrosoftAppCredentials).GetTokenAsync();
                        var uri = new Uri(attachment.ContentUrl);
                        using (var httpClient = new HttpClient())
                        {
                            if (uri.Host.EndsWith("skype.com") && uri.Scheme == Uri.UriSchemeHttps)
                            {
                                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
                            }
                            else
                            {
                                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(attachment.ContentType));
                            }

                            attachmentData = await httpClient.GetByteArrayAsync(uri);
                            isUpload = attachmentData != null;
                        }

                    }
                    else if (string.IsNullOrEmpty(activity.Text))
                    {
                        imgUrl = activity.Text.StartsWith("http") || activity.Text.StartsWith("https") ? activity.Text : string.Empty;
                    }

                    //Check ImagePath
                    if (!isUpload && string.IsNullOrEmpty(imgUrl))
                    {
                        Relpy(connector, activity, "I'm sorry this isn't Image or ImageUrl or doesn't support this format.");
                    }
                    else
                    {
                        //Microsoft Vision Service API
                        var service = new VisionService();

                        //Working Message
                        //Relpy(connector, activity, "Almost done!");

                        var analysisResult = isUpload ? await service.UploadAndDescripteImage(attachmentData)
                                                      : await service.DescripteUrl(imgUrl);

                        //Send Succcess Message
                        Relpy(connector, activity, $"Here you go! Hmmm. Let me think about that {analysisResult.Description?.Captions?.FirstOrDefault()?.Text}. :)");

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
                await RelpyAsync(connector, activity, "Something went wrong. Please try again!");
                //response = Request.CreateResponse(HttpStatusCode.Accepted);
            }
            finally
            {
                connector.Dispose();
            }
            return Request.CreateResponse(HttpStatusCode.Accepted);
        }

        private async Task RelpyAsync(ConnectorClient connector, Activity activity, string message)
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