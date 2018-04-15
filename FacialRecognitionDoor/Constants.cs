using System;
using System.Diagnostics;

namespace FacialRecognitionDoor
{
    /// <summary>
    /// General constant variables
    /// </summary>
    public static class GeneralConstants
    {
        // This variable should be set to false for devices, unlike the Raspberry Pi, that have GPU support
        public const bool DisableLiveCameraFeed = false;

        // Oxford Face API Primary should be entered here
        // You can obtain a subscription key for Face API by following the instructions here: https://azure.microsoft.com/en-us/try/cognitive-services/
        public const string OxfordAPIKey = "7f3ac8ab760d4d56a7cc7fc2e7ba0b80";

        // Enter the API endpoint address.
        // If you have a 'free trial' key, you can find the here: https://azure.microsoft.com/en-us/try/cognitive-services/my-apis/
        // If you have a key from Azure, find your account here: https://portal.azure.com/#blade/HubsExtension/Resources/resourceType/Microsoft.CognitiveServices%2Faccounts.
        public const string FaceAPIEndpoint = "https://southcentralus.api.cognitive.microsoft.com/face/v1.0";
        
        // Name of the folder in which all Whitelist data is stored
        public const string WhiteListFolderName = "Facial Recognition Door Whitelist";

    }

    /// <summary>
    /// Constant variables that hold messages to be read via the SpeechHelper class
    /// </summary>
    public static class SpeechContants
    {
        public const string InitialGreetingMessage = "Hey! I am Sanya!. I'll be your security manager. Just so you know securtiy Protocols has been activated.";

        public const string VisitorNotRecognizedMessage = "Sorry! I don't recognize you, so I cannot open the door.";
        public const string NoCameraMessage = "Sorry! It seems like your camera has not been fully initialized.";

        public const string newUsermessage = "Okay! Please stand infront of the camera so that I can register you. ";

        public static string GeneralGreetigMessage(string visitorName)
        {
            return "Welcome " + visitorName + "! I will open the door for you.";
        }
    }

    /// <summary>
    /// Constant variables that hold values used to interact with device Gpio
    /// </summary>
    public static class GpioConstants
    { 
        // The GPIO pin that the doorbell button is attached to
        public const int ButtonPinID = 5;

        // The GPIO pin that the door lock is attached to
        public const int DoorLockPinID = 4;

        // The amount of time in seconds that the door will remain unlocked for
        public const int DoorLockOpenDurationSeconds = 10;
    }

    public class SanyaResponses
    {
        

        public string returnResponse()
        {
            
            Random random = new Random();
            string[] SanyaWelcomeMessage = { "HELLO! How are you?", "Haaye, How may I help you?", "Hey","Howdy", "Hey yaa" };
            string a = SanyaWelcomeMessage[random.Next(0,3)];
            return a; 
        }
    }
}
