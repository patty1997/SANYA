using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using FacialRecognitionDoor.Helpers;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.SpeechRecognition;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FacialRecognitionDoor
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Sanya : Page
    {
        private static uint HResultPrivacyStatementDeclined = 0x80045509;
        private static uint HResultRecognizerNotFound = 0x8004503a;
        // Speech Related Variables:
        private SpeechHelper speech;
        private SpeechRecognizer speechRecognizer;
        private SanyaResponses sanyaResponses;
        private IAsyncOperation<SpeechRecognitionResult> recognitionOperation;
        private ISpeechRecognitionConstraint constraint;
        private ResourceContext speechContext;
        private ResourceMap speechResourceMap;
        private SpeechRecognitionResult speechRecognitionResult;
        public Sanya()
        {
            this.InitializeComponent();
        }

        private async void speechMediaElement_Loaded(object sender, RoutedEventArgs e)
        {
            if (speech == null)
            {
                sanyaResponses = new SanyaResponses();
                speech = new SpeechHelper(speechElement);
                await speech.Read(sanyaResponses.returnResponse());
            }
            else
            {
                // Prevents media element from re-greeting visitor
                speechElement.AutoPlay = false;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (speechRecognizer != null)
            {
                if (speechRecognizer.State != SpeechRecognizerState.Idle)
                {
                    if (recognitionOperation != null)
                    {
                        recognitionOperation.Cancel();
                        recognitionOperation = null;
                    }
                }


                this.speechRecognizer.Dispose();
                this.speechRecognizer = null;
            }
        }

        private async Task InitializeSpeechRecognizer()
        {
            if (speechRecognizer != null)
            {
                //speechRecognizer.ContinuousRecognitionSession.Completed -= ContinuousRecognitionSession_Completed;
                //speechRecognizer.ContinuousRecognitionSession.ResultGenerated -= ContinuousRecognitionSession_ResultGenerated;
                speechRecognizer.StateChanged -= SpeechRecognizer_StateChanged;
                this.speechRecognizer.Dispose();
                this.speechRecognizer = null;
            }

            try
            {            //Create an instance of speech recognizer
                speechRecognizer = new SpeechRecognizer();

                speechRecognizer.StateChanged += SpeechRecognizer_StateChanged;

                //Add grammar file constraint to the recognizer.
                // string fileName = "SRGS.xml";
                //StorageFile grammarContentFile = await Package.Current.InstalledLocation.GetFileAsync(fileName);
                //SpeechRecognitionGrammarFileConstraint grammarFileConstraint = new SpeechRecognitionGrammarFileConstraint(grammarContentFile);
                var storageFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///SRGSmusic.xml"));
                var grammarfileConstraint = new Windows.Media.SpeechRecognition.SpeechRecognitionGrammarFileConstraint(storageFile, "music");

                speechRecognizer.Constraints.Add(grammarfileConstraint);
                resultTextBlock.Text = "Example play, pause";

                SpeechRecognitionCompilationResult compilationResult = await speechRecognizer.CompileConstraintsAsync();
                resultTextBlock.Text = "Yahan tak to aa hi gye";
                if (compilationResult.Status != SpeechRecognitionResultStatus.Success)
                {
                    // Disable the recognition button.
                    btnContinuousRecognize.IsEnabled = false;

                    // Let the user know that the grammar didn't compile properly.
                    resultTextBlock.Text = "Unable to compile grammar.";
                }
                else
                {

                    resultTextBlock.Text = "Compilation Successful!";
                    // Set EndSilenceTimeout to give users more time to complete speaking a phrase.
                    speechRecognizer.Timeouts.EndSilenceTimeout = TimeSpan.FromSeconds(1.2);

                    // Handle continuous recognition events. Completed fires when various error states occur. ResultGenerated fires when
                    // some recognized phrases occur, or the garbage rule is hit.
                    //speechRecognizer.ContinuousRecognitionSession.Completed += ContinuousRecognitionSession_Completed;
                    //speechRecognizer.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;


                    btnContinuousRecognize.IsEnabled = true;

                    
                }
            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == HResultRecognizerNotFound)
                {
                    btnContinuousRecognize.IsEnabled = false;

                    resultTextBlock.Visibility = Visibility.Visible;
                    resultTextBlock.Text = "Speech Language pack for selected language not installed.";
                }
                else
                {
                    var messageDialog = new Windows.UI.Popups.MessageDialog(ex.Message, "Exception");
                    await messageDialog.ShowAsync();
                }
            }

        }
    

        private async void ContinuousRecognitionSession_Completed(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionCompletedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                this.NotifyUser("Continuous Recognition Completed: " + args.Status.ToString(), NotifyType.StatusMessage);
            });
        }

        private async void SpeechRecognizer_StateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                this.NotifyUser(args.State.ToString(), NotifyType.StatusMessage);
            });
        }

        public void NotifyUser(string strMessage, NotifyType type)
        {
            // If called from the UI thread, then update immediately.
            // Otherwise, schedule a task on the UI thread to perform the update.
            if (Dispatcher.HasThreadAccess)
            {
                UpdateStatus(strMessage, type);
            }
            else
            {
                var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => UpdateStatus(strMessage, type));
            }
        }

        private void UpdateStatus(string strMessage, NotifyType type)
        {
            switch (type)
            {
                case NotifyType.StatusMessage:
                    StatusBorder.Background = new SolidColorBrush(Windows.UI.Colors.Green);
                    break;
                case NotifyType.ErrorMessage:
                    StatusBorder.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                    break;
            }

            StatusBlock.Text = strMessage;

            // Collapse the StatusBlock if it has no text to conserve real estate.
            StatusBorder.Visibility = (StatusBlock.Text != String.Empty) ? Visibility.Visible : Visibility.Collapsed;
            if (StatusBlock.Text != String.Empty)
            {
                StatusBorder.Visibility = Visibility.Visible;
                StatusPanel.Visibility = Visibility.Visible;
            }
            else
            {
                StatusBorder.Visibility = Visibility.Collapsed;
                StatusPanel.Visibility = Visibility.Collapsed;
            }

            // Raise an event if necessary to enable a screen reader to announce the status update.
            var peer = FrameworkElementAutomationPeer.FromElement(StatusBlock);
            if (peer != null)
            {
                peer.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
            }
        }

        public enum NotifyType
        {
            StatusMessage,
            ErrorMessage
        };

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {

            // Prompt the user for permission to access the microphone. This request will only happen
            // once, it will not re-prompt if the user rejects the permission.
            bool permissionGained = await AudioCapturePermissions.RequestMicrophonePermission();
            if (permissionGained)
            {
                await InitializeSpeechRecognizer();
            }
            else
            {
                resultTextBlock.Text = "Permission to access capture resources was not given by the user, reset the application setting in Settings->Privacy->Microphone.";
                btnContinuousRecognize.IsEnabled = false;
            }

            
        }

        public async void ContinuousRecognize_Click(object sender, RoutedEventArgs e)
        {
            btnContinuousRecognize.IsEnabled = false;
            // The recognizer can only start listening in a continuous fashion if the recognizer is currently idle.
            // This prevents an exception from occurring.
            if (speechRecognizer.State == SpeechRecognizerState.Idle)
            {
                // Reset the text to prompt the user.
                try
                {
                    
                    speechRecognitionResult= await speechRecognizer.RecognizeAsync();
                    //HandleRecognitionResult(speechRecognitionResult);
                    if (speechRecognitionResult.Status == SpeechRecognitionResultStatus.Success)
                    {
                        if (speechRecognitionResult.Confidence == SpeechRecognitionConfidence.Rejected /*|| speechRecognitionResult.Confidence == SpeechRecognitionConfidence.Rejected*/)
                        {
                            await speech.Read("Sorry! I didn't get that! Can you repeat again?");
                            if (speechRecognizer.State == SpeechRecognizerState.SoundEnded)
                            {
                                speechRecognitionResult = await speechRecognizer.RecognizeWithUIAsync();
                                HandleRecognitionResult(speechRecognitionResult);
                            }
                        }
                        else
                        {
                            await speech.Read("All right! Next song comming up!");
                            resultTextBlock.Text = "Playing the song...";


                        }
                    }

                }
                catch (Exception exception)
                {
                    var messageDialog = new Windows.UI.Popups.MessageDialog(exception.Message, "StartAsync Exception");
                    await messageDialog.ShowAsync();
                }
            }
            else
            {
                try
                {
                    // Reset the text to prompt the user.
                    // Cancelling recognition prevents any currently recognized speech from
                    // generating a ResultGenerated event. StopAsync() will allow the final session to 
                    // complete.
                    //await speechRecognizer.ContinuousRecognitionSession.CancelAsync();
                }
                catch (Exception ex)
                {
                    var messageDialog = new Windows.UI.Popups.MessageDialog(ex.Message, "CancelAsync Exception");
                    await messageDialog.ShowAsync();
                }
            }
            btnContinuousRecognize.IsEnabled = true;
        }
        //private async void ContinuousRecognitionSession_ResultGenerated()
        /*private async void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            // Developers may decide to use per-phrase confidence levels in order to tune the behavior of their 
            // grammar based on testing.
            if (args.Result.Confidence == SpeechRecognitionConfidence.Medium ||
                args.Result.Confidence == SpeechRecognitionConfidence.High)
            {
                //await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                //{
                    HandleRecognitionResult();
                //});
            }
            // Prompt the user if recognition failed or recognition confidence is low.
            else if (args.Result.Confidence == SpeechRecognitionConfidence.Rejected ||
            args.Result.Confidence == SpeechRecognitionConfidence.Low)
            {
                // In some scenarios, a developer may choose to ignore giving the user feedback in this case, if speech
                // is not the primary input mechanism for the application.
                //await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                //{
                    resultTextBlock.Text = "Sorry! I didn't understand that";
                //});
            }
        }*/

        private async void HandleRecognitionResult(SpeechRecognitionResult speechRecognitionResult)
        {
            if (speechRecognitionResult.Status == SpeechRecognitionResultStatus.Success)
            {
                if (speechRecognitionResult.Confidence == SpeechRecognitionConfidence.Rejected /*|| speechRecognitionResult.Confidence == SpeechRecognitionConfidence.Rejected*/)
                {
                    await speech.Read("Sorry! I didn't get that! Can you repeat again?");
                    if (speechRecognizer.State == SpeechRecognizerState.SoundEnded)
                    {
                        speechRecognitionResult = await speechRecognizer.RecognizeWithUIAsync();
                        HandleRecognitionResult(speechRecognitionResult);
                    }
                }
                else
                {
                    await speech.Read("All right! Next song comming up!");
                    resultTextBlock.Text = "Playing the song...";


                }
            }
        }

    }


}
