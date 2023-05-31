using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
namespace BiitSocioApis.classes
{
    public class SendSms
    {
        public static void SendSmss( string fromPhoneNumber, string toPhoneNumber, string message)
        {
            string accountSid = "AC26d5899953284ab94476f7e38d123cef"; string authToken = "43f0695e74f01479c094fbf76763c720";

            // Initialize the Twilio client with your account SID and auth token
            TwilioClient.Init(accountSid, authToken);

            try
            {
                // Send the SMS message using the Twilio API
                var smsMessage = MessageResource.Create(
                    body: message,
                    from: new Twilio.Types.PhoneNumber(fromPhoneNumber),
                    to: new Twilio.Types.PhoneNumber(toPhoneNumber)
                );

                Console.WriteLine($"SMS message sent: {smsMessage.Sid}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending SMS message: {ex.Message}");
            }
        }
    }
}