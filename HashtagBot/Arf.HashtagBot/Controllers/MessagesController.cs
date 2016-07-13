﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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
            if (activity.Type == ActivityTypes.Message)
            {
                var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                // calculate something for us to return
                var length = (activity.Text ?? string.Empty).Length;
                if (length == 0) return null;
                var imgPath = activity.Attachments.Count > 0 ? activity.Attachments.FirstOrDefault().ContentUrl : activity.Text;
                var isUpload = imgPath != null && !imgPath.StartsWith("http");

                var service = new VisionService();
                var analysisResult = isUpload
                    ? await service.UploadAndDescripteImage(imgPath)
                    : await service.DescripteUrl(imgPath);

                // return our reply to the user
                var reply = activity.CreateReply(string.Join(" ", analysisResult.Description.Tags.Select(s => s = "#" + s)));
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
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