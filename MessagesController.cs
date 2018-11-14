using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.DirectoryServices;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Web.Http.Description;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.ConnectorEx;
using System.Text;
using System.Linq;
using System.DirectoryServices.AccountManagement;
using System.Security.Principal;
using System.Security.Claims;

namespace Microsoft.Bot.Sample.QnABot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// receive a message from a user and send replies
        /// </summary>
        /// <param name="activity"></param>
        [ResponseType(typeof(void))]
        public virtual async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            // check if activity is of type message
            if (activity.GetActivityType() == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new RootDialog());
            }
            else
            {
                HandleSystemMessage(activity);
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
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
                var domain = new PrincipalContext(ContextType.Domain);
                var currentUser = UserPrincipal.FindByIdentity(domain, User.Identity.Name);
                //Activity reply1 = message.CreateReply("Hi  " + currentUser + "");
                if (message.MembersAdded.Any(o => o.Id == message.Recipient.Id))
                {
                    //string fullName = null;
                    //using (PrincipalContext context = new PrincipalContext(ContextType.Domain))
                    //{
                    //    using (UserPrincipal user = UserPrincipal.FindByIdentity(context, User.Identity.Name))
                    //    {
                    //        if (user != null)
                    //        {
                    //            fullName = user.DisplayName;
                    //        }
                    //    }
                    //}
                    StringBuilder stringb = new StringBuilder();
                    string loggedUserName = ClaimsPrincipal.Current.Identity.Name;//WindowsIdentity.GetCurrent().Name.ToString();//System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString();//Environment.UserName.ToUpper();
                    stringb.Append("Hi ");
                    stringb.AppendLine();
                    stringb.Append("<b>");
                    stringb.Append("+ loggedUserName ! +");
                    stringb.Append("</b>");
                    stringb.AppendLine();
                    string finalstring = stringb.ToString();
                    ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
                    Activity reply1 = message.CreateReply("Hi  " + currentUser + "");
                    Activity reply2 = message.CreateReply("This is HR Bot, an I'm your Virtual assistant. How can I help you with your learning today? ");
                    connector.Conversations.ReplyToActivityAsync(reply1);
                    connector.Conversations.ReplyToActivityAsync(reply2);

                }


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

        public static string GetFullName(string strLogin)
        {
            string str = "";
            string strDomain;
            string strName;

            // Parse the string to check if domain name is present.
            int idx = strLogin.IndexOf('\\');
            if (idx == -1)
            {
                idx = strLogin.IndexOf('@');
            }

            if (idx != -1)
            {
                strDomain = strLogin.Substring(0, idx);
                strName = strLogin.Substring(idx + 1);
            }
            else
            {
                strDomain = Environment.MachineName;
                strName = strLogin;
            }

            DirectoryEntry obDirEntry = null;
            try
            {
                obDirEntry = new DirectoryEntry("WinNT://" + strDomain + "/" + strName);
                System.DirectoryServices.PropertyCollection coll = obDirEntry.Properties;
                object obVal = coll["FullName"].Value;
                str = obVal.ToString();
            }
            catch (Exception ex)
            {
                str = ex.Message;
            }
            return str;
        }
    }
}