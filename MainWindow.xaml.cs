using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Xml;

namespace FCP_XML_SRT_Syncer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        string fcpXml = "";
        XmlDocument xmlDoc = null;

        private void BtnLoadFCPXML_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Final Cut Pro XML Files (.xml)|*.xml";
            if (ofd.ShowDialog() == true)
            {
                fcpXml = ofd.FileName;
                xmlDoc = new XmlDocument();
                xmlDoc.Load(fcpXml);
                //analyzeXML();

                //Hardcode for now
                //<file id="file-5">
                string mainSequenceId = xmlDoc.SelectSingleNode("xmeml/sequence").Attributes["id"].InnerText;
                findSnippets("file-5", mainSequenceId);
            }
        }

        private void findSnippets(string fileId, string sequenceId)
        {
            XmlNodeList nodeList = xmlDoc.SelectNodes("//file[@id='"+fileId+"']");

            List <Snippet> confirmedSnippets = new List<Snippet>();

            foreach (XmlNode node in nodeList)
            {
                // get clipitem
                XmlNode clipitem = node.ParentNode;

                // should always be the case I think, just to be sure
                if(clipitem.Name == "clipitem")
                {
                    XmlNode track = clipitem.ParentNode;
                    XmlNode audioOrVideo = track.ParentNode;
                    XmlNode media = audioOrVideo.ParentNode;
                    XmlNode sequence = media.ParentNode;
                    XmlNode timebase = sequence.SelectNodes("rate/timebase")[0]; //framerate
                    double framerateSequence = double.Parse(timebase.FirstChild.InnerText);

                    int start = int.Parse(clipitem.SelectSingleNode("start").FirstChild.InnerText);
                    int end = int.Parse(clipitem.SelectSingleNode("end").FirstChild.InnerText);
                    int inV = int.Parse(clipitem.SelectSingleNode("in").FirstChild.InnerText);
                    int outV = int.Parse(clipitem.SelectSingleNode("out").FirstChild.InnerText);
                    XmlNode timebaseClipItem = clipitem.SelectNodes("rate/timebase")[0]; //framerate
                    double framerateClipItem = double.Parse(timebaseClipItem.FirstChild.InnerText);

                    // Milliseconds
                    double sourceStartsAt = start / framerateSequence;
                    double sourceEndsAt = end / framerateSequence;
                    double absoluteSourceIn = inV / framerateClipItem;
                    double absoluteSourceOut = outV / framerateClipItem;

                    //foundSeqs_txt.Text += start + " " + end +  " " + absoluteSourceIn + " " + absoluteSourceOut + " " + sequence.Attributes["id"].InnerText + " " + framerateSequence + "\n";

                    string currentSequenceId = sequence.Attributes["id"].InnerText;

                    Snippet sequenceSnippet = new Snippet(currentSequenceId, sourceStartsAt, sourceEndsAt, absoluteSourceIn, absoluteSourceOut);

                    if (currentSequenceId == sequenceId)
                    {
                        confirmedSnippets.Add(sequenceSnippet);
                    } else
                    {
                        confirmedSnippets.AddRange(followSnippet(sequenceSnippet, sequenceId));
                    }

                    //Snippet snippet = new Snippet(parent, );
                }
            }

            foundSeqs_txt.Text = confirmedSnippets.Count.ToString();
        }

        private List<Snippet> followSnippet(Snippet snippet, string sequenceId)
        {
            List<Snippet> confirmedSnippets = new List<Snippet>();
            
            XmlNodeList nodeList = xmlDoc.SelectNodes("//sequence[@id='" + snippet.sequenceId + "']");

            foreach(XmlNode node in nodeList)
            {

                // get clipitem
                XmlNode clipitem = node.ParentNode;

                // should always be the case I think, just to be sure
                if (clipitem.Name == "clipitem")
                {
                    XmlNode track = clipitem.ParentNode;
                    XmlNode audioOrVideo = track.ParentNode;
                    XmlNode media = audioOrVideo.ParentNode;
                    XmlNode sequence = media.ParentNode;
                    XmlNode timebase = sequence.SelectNodes("rate/timebase")[0]; //framerate
                    double framerateSequence = double.Parse(timebase.FirstChild.InnerText);

                    int start = int.Parse(clipitem.SelectSingleNode("start").FirstChild.InnerText);
                    int end = int.Parse(clipitem.SelectSingleNode("end").FirstChild.InnerText);
                    int inV = int.Parse(clipitem.SelectSingleNode("in").FirstChild.InnerText);
                    int outV = int.Parse(clipitem.SelectSingleNode("out").FirstChild.InnerText);
                    XmlNode timebaseClipItem = clipitem.SelectNodes("rate/timebase")[0]; //framerate
                    double framerateClipItem = double.Parse(timebaseClipItem.FirstChild.InnerText);

                    // Milliseconds
                    double clipStartsAt = start / framerateSequence;
                    double clipEndsAt = end / framerateSequence;
                    double clipIn = inV / framerateClipItem;
                    double clipOut = outV / framerateClipItem;

                    ///foundSeqs_txt.Text += start + " " + end + " " + absoluteSourceIn + " " + absoluteSourceOut + " " + sequence.Attributes["id"].InnerText + " " + framerateSequence + "\n";

                    /*
                     * 
                    File in sequence2:
                    starts at 2 sec (snippet.sourceStartsAt)
                    ends at 4 sec (snippet.sourceEndsAt)
                    in point 50 sec (snippet.sourceAbsoluteInPoint)
                    out point 52 sec (snippet.sourceAbsoluteOutPoint)

                    that sequence2 in another sequence1:
                    starts2 at 40 sec (clipStartsAt)
                    ends2 at 50 sec (clipEndsAt)
                    in2 point 3 sec (clipIn)
                    out2 point 13 sec (clipOut)

                    expected collapsed result of file:
                    startsAbs at 40 sec 
                    endsAbs at 41 sec
                    inAbs point 51 sec
                    outAbs point 52 sec

                    in other words, starts/ends relates to the outermost sequence
                    in and out always relates to the file itself

                    in and out point of sequence2 in sequence1 crop/trim the original range.
                    start and end define where it's played back

                    so in and out need to be processed first

                    in and out of sequence2 in sequence1 first interact with start and end of file in sequence2
                     * 
                     * 
                    if(in2 point > ends at){ completely lost }
                    if(out2 point < starts at) {completely lost}
                    else{
                    // calculate speed ratio of file in sequence2
                    currentratio = ends-starts/out-in; // in this case 1
                    croppedStarts = Math.Max(in2,starts) //3
                    croppedEnds = Math.Min(out2,ends) //4

                    startCrop = croppedStarts - starts; // 1
                    endCrop = ends - croppedEnds;  // 0

                    startCropSigned = in2-starts; // 1
                    endCropSigned = ends-out2;  // -9

                    // Now factor in speed ratio of file in sequence2:
                    // here we divide through currentratio because we are translating inwards
                    // so for example if the file in sequence2 is sped up 200%, then currentratio is 2
                    // and thus for cropping translated onto file itself, we need to multiply by 0.5
                    // OR, as here, divide by 2, which is the currentratio.
                    croppedTimeTranslatedIn = in point + startCrop/currentratio // 51
                    croppedTimeTranslatedOut = out point - endCrop/currentratio // 52

                    inAbs = croppedTimeTranslatedIn //done 
                    outAbs = croppedTimeTranslatedOut // done

                    // Now the starts and ends point 
                    outerRatio = ends2-starts1/out2-in2 // here it's again 1
                    // The following basically does only one thing: It narrows the start and end range to only encompass the deeply encapsulated file.
                    collapsedstart = Math.Max(starts2,starts2 - startCropSigned*outerRatio) // if the cropping at start is negative, it will start later as - minus - = +
                    collapsedend  = Math.Min(ends2,ends2 + endCropSigned*outerRatio) // If the cropping at end is negative, it will end earlier
                    //collapsedstart should now be: 40
                    //collaspedend should now be: 50 - 9*1 = 41
                    }
                     */
                    if (clipIn > snippet.sourceEndsAt){
                        // completely lost 
                        //Console.WriteLine("test");
                    }
                    else if (clipOut < snippet.sourceStartsAt) {
                        //completely lost 
                        //Console.WriteLine("test");
                    }
                    else{
                        // calculate speed ratio of file in sequence2
                        double currentratio = (snippet.sourceEndsAt - snippet.sourceStartsAt) /(snippet.sourceAbsoluteOutPoint-snippet.sourceAbsoluteInPoint);
                        double croppedStarts = Math.Max(clipIn, snippet.sourceStartsAt);
                        double croppedEnds = Math.Min(clipOut, snippet.sourceEndsAt); 

                        double startCrop = croppedStarts - snippet.sourceStartsAt; 
                        double endCrop = snippet.sourceEndsAt - croppedEnds;  

                        double startCropSigned = clipIn - snippet.sourceStartsAt;
                        double endCropSigned = snippet.sourceEndsAt - clipOut;

                        // Now factor in speed ratio of file in sequence2:
                        // here we divide through currentratio because we are translating inwards
                        // so for example if the file in sequence2 is sped up 200%, then currentratio is 2
                        // and thus for cropping translated onto file itself, we need to multiply by 0.5
                        // OR, as here, divide by 2, which is the currentratio.
                        double croppedTimeTranslatedIn = snippet.sourceAbsoluteInPoint + startCrop / currentratio;
                        double croppedTimeTranslatedOut = snippet.sourceAbsoluteOutPoint - endCrop / currentratio;

                        double inAbs = croppedTimeTranslatedIn; //done 
                        double outAbs = croppedTimeTranslatedOut; // done

                        // Now the starts and ends point 
                        double outerRatio = (clipEndsAt - clipStartsAt) / (clipOut - clipIn); // here it's again 1

                        // The following basically does only one thing: It narrows the start and end range to only encompass the deeply encapsulated file.
                        double collapsedstart = Math.Max(clipStartsAt, clipStartsAt - startCropSigned * outerRatio); // if the cropping at start is negative, it will start later as - minus - = +
                        double collapsedend = Math.Min(clipEndsAt, clipEndsAt + endCropSigned * outerRatio); // If the cropping at end is negative, it will end earlier
                                                                                                             //collapsedstart should now be: 40
                                                                                                             //collaspedend should now be: 50 - 9*1 = 41

                        string currentSequenceId = sequence.Attributes["id"].InnerText;

                        Snippet sequenceSnippet = new Snippet(currentSequenceId, collapsedstart, collapsedend, inAbs, outAbs);

                        if (currentSequenceId == sequenceId)
                        {
                            foundSeqs_txt.Text += sequenceSnippet.ToString() + "\n";
                            confirmedSnippets.Add(sequenceSnippet);
                        }
                        else
                        {
                            confirmedSnippets.AddRange(followSnippet(sequenceSnippet, sequenceId));
                        }
                    }
                    
                }

            }
            return confirmedSnippets;
        }

        private void analyzeXML()
        {
            XmlNodeList nodeList = xmlDoc.SelectNodes("//file/pathurl");
            string stuff = "";

            stuff = nodeList.Count.ToString();
            stuff = xmlDoc.ToString();

            foreach(XmlNode node in nodeList)
            {
                stuff += "blah\n";
                stuff += node.FirstChild.InnerText;
            }
            foundSeqs_txt.Text = stuff;
        }
    }
}
