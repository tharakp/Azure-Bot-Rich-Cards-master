using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;

namespace Microsoft.Bot.Sample.QnABot
{
    [Serializable]
    public class RootDialog : QnAMakerDialog
    {
        public RootDialog() : base(new QnAMakerService(new QnAMakerAttribute(ConfigurationManager.AppSettings["QnAAuthKey"], ConfigurationManager.AppSettings["QnAKnowledgebaseId"], "No good match in FAQ.", 0.5, 1, ConfigurationManager.AppSettings["QnAEndpointHostName"])))
        {
        }
        public async Task StartAsync(IDialogContext context)
        {
            /* Wait until the first message is received from the conversation and call MessageReceviedAsync 
            *  to process that message. */
            context.Wait(this.MessageReceivedAsync);
        }

        private new async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            /* When MessageReceivedAsync is called, it's passed an IAwaitable<IMessageActivity>. To get the message,
            *  await the result. */
            var message = await result;
            
            var qnaSubscriptionKey = ConfigurationManager.AppSettings["QnASubscriptionKey"];
            var qnaKBId = ConfigurationManager.AppSettings["QnAKnowledgebaseId"];

            // QnA Subscription Key and KnowledgeBase Id null verification
            if (!string.IsNullOrEmpty(qnaSubscriptionKey) && !string.IsNullOrEmpty(qnaKBId))
            {
                await context.Forward(new BasicQnAMakerDialog(), AfterAnswerAsync, message, CancellationToken.None);
            }
            else
            {
                await context.PostAsync("Please set QnAKnowledgebaseId and QnASubscriptionKey in App Settings. Get them at https://qnamaker.ai.");
            }
            
        }
        public static string GetSetting(string key)
        {
           // var value;//ConfigurationManager.AppSettings(key);
          //  if (String.IsNullOrEmpty(value) && key == "QnAAuthKey")
          //  {
              var  value = ConfigurationManager.AppSettings["QnASubscriptionKey"]; // QnASubscriptionKey for backward compatibility with QnAMaker (Preview)
           // }
            return value;
        }
        private async Task AfterAnswerAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            // wait for the next user message
            context.Wait(MessageReceivedAsync);
        }
        protected override async Task RespondFromQnAMakerResultAsync(IDialogContext context, IMessageActivity message, QnAMakerResults result)
        {
            var answer = result.Answers.First().Answer;
            Activity reply = ((Activity)context.Activity).CreateReply();

            string[] qnaAnswerData = answer.Split(';');
            int dataSize = qnaAnswerData.Length;
            //image and video card
            
            if (dataSize <= 2){
                // > 3 && dataSize <= 6            
                //await context.Forward(new BasicQnAMakerDialog(), AfterAnswerAsync, message, CancellationToken.None);
                //reply.Text = answer;
                await context.PostAsync(answer);
                //context.Wait(MessageReceived);

            }
            else
            {
                var attachment = GetSelectedCard(answer);
                reply.Attachments.Add(attachment);
                await context.PostAsync(reply);
            }
        }

        private static Attachment GetSelectedCard(string answer)
        {
            int len = answer.Split(';').Length;
            switch (len)
            {
                case 4: return GetHeroCard(answer);                    
                case 6: return GetVideoCard(answer);
                default: return GetHeroCard(answer);
            }
        }

      
        private static Attachment GetHeroCard(string answer)
        {
            string[] qnaAnswerData = answer.Split(';');
            string title = qnaAnswerData[0];
            string description = qnaAnswerData[1];
            string url = qnaAnswerData[2];
            string imageURL = qnaAnswerData[3];

            HeroCard card = null;
            
            {
                card = new HeroCard
                {
                    Title = title,
                    Subtitle = description,
                };

                card.Buttons = new List<CardAction>
            {
                new CardAction(ActionTypes.OpenUrl, "Learn More", value: url)
            };

                card.Images = new List<CardImage>
            {
                new CardImage( url = imageURL)
            };
            }
            return card.ToAttachment();
        }
       
        private static Attachment GetVideoCard(string answer)
        {
            string[] qnaAnswerData = answer.Split(';');
            string title = qnaAnswerData[0];
            string subtitle = qnaAnswerData[1];
            string description = qnaAnswerData[2];
            string thumbimageurl = qnaAnswerData[3];
            string mediaUrl = qnaAnswerData[4];
            string url = qnaAnswerData[5];

            VideoCard card = new VideoCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = description,
            };
            card.Image = new ThumbnailUrl
            {
                Url = thumbimageurl
            };
            card.Media = new List<MediaUrl>
                    {
                        new MediaUrl()
                        {
                            Url = mediaUrl
                        }
                    };

            card.Buttons = new List<CardAction>
                    {
                        new CardAction()
                        {
                            Title = "Yes, this is what I’m looking for",
                            Type = ActionTypes.OpenUrl,
                            Value = url
                        }
                    };
            return card.ToAttachment();

        }
    }

     // Dialog for QnAMaker Preview service
    [Serializable]
    public class BasicQnAMakerPreviewDialog : QnAMakerDialog
    {
        // Go to https://qnamaker.ai and feed data, train & publish your QnA Knowledgebase.
        // Parameters to QnAMakerService are:
        // Required: subscriptionKey, knowledgebaseId, 
        // Optional: defaultMessage, scoreThreshold[Range 0.0 – 1.0]
        public BasicQnAMakerPreviewDialog() : base(new QnAMakerService(new QnAMakerAttribute(RootDialog.GetSetting("QnAAuthKey"), Utils.GetAppSetting("QnAKnowledgebaseId"), "No good match in FAQ.", 0.5)))
        { }
    }

    // Dialog for QnAMaker GA service
    [Serializable]
    public class BasicQnAMakerDialog : QnAMakerDialog
    {
        // Go to https://qnamaker.ai and feed data, train & publish your QnA Knowledgebase.
        // Parameters to QnAMakerService are:
        // Required: qnaAuthKey, knowledgebaseId, endpointHostName
        // Optional: defaultMessage, scoreThreshold[Range 0.0 – 1.0]
        public BasicQnAMakerDialog() : base(new QnAMakerService(new QnAMakerAttribute(RootDialog.GetSetting("QnAAuthKey"), Utils.GetAppSetting("QnAKnowledgebaseId"), "No good match in FAQ.", 0.5, 1, Utils.GetAppSetting("QnAEndpointHostName"))))
        { }

    }
}