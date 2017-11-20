//using System;
//using System.Xml;
//using System.IO;
//using System.Text;
//using System.Collections.Generic;

//namespace XMLReferrerExtractor
//{
//    public class Test2
//    {
//        //The below static strings need to be converted to the Charles Proxy elements
//        static string ipKey;
//        static string httpMethodKey;
//        static string fileKey;
//        static string dateKey;
//        static string referrerKey;


//        public static void WriteAttributes(XmlReader reader, XmlWriter writer)
//        {
//            if (reader.MoveToFirstAttribute())
//            {
//                do
//                {
//                    //Do these attribute strings need to be changed?
//                    //From the looks of it, this is actually per attribute
//                    writer.WriteAttributeString(reader.Prefix,
//                                 reader.LocalName,
//                                 reader.NamespaceURI,
//                                 reader.Value);
//                } while (reader.MoveToNextAttribute());
//                reader.MoveToElement();
//            }
//        }

//        //public static void WriteEvent(XmlWriter writer, string ip,
//        //                               string httpMethod, string file,
//        //                               string date, string referrer)
//        //{
//        //    //Here is where we rewrite the transaction. Will have to adjust the parameters before the method, but combine them in here
//        //    writer.WriteStartElement("event");
//        //    writer.WriteElementString("ip", ip);
//        //    writer.WriteElementString("http_method", httpMethod);
//        //    writer.WriteElementString("file", file);
//        //    writer.WriteStartElement("event2");
//        //    writer.WriteElementString("date", date);
//        //    if (referrer != null) writer.WriteElementString("referrer", referrer);
//        //    writer.WriteEndElement();
//        //    writer.WriteEndElement();

//        //}

//        ////This will be used to pass values into the write event
//        ////Will have to rewrite the write element to consider non-standard templates
//        //public static void ReadEvent(XmlReader reader, out string elementStartTag, out string elementInternal, out string elementEndTag) {
//        //    elementStartTag = elementInternal = elementEndTag = "";


//        //    //ReadEvent will have to read in chunks and output in chunks
//        //    while (reader.Read())
//        //    {
//        //        if (reader.NodeType == XmlNodeType.Element)
//        //        {
//        //            if (reader.Name == "name")
//        //            {
//        //                reader.Read();
//        //                if (reader.Value == "Referer")
//        //                {
//        //                    //combine reader.Value with host
//        //                    if (reader.Name.Equals("transaction"))
//        //                    {

//        //                    }
//        //                }
//        //            }
//        //        }
//        //        else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "transaction") {

//        //            break;
//        //        }
//        //    }
//        //}

//        public static void ExtractReferers(XmlReader reader, Dictionary<string, string> hostRefLocal, XmlWriter xmlWriter)
//        {
//            string path = "";
//            reader.MoveToContent();


//            do
//            {
//                if (reader.NodeType == XmlNodeType.Element)
//                {
//                    if (reader.Name.Equals("transaction"))
//                    {
//                        path = reader.GetAttribute("path").ToString();
//                    }
//                    else if (reader.Name.Equals("name"))
//                    {
//                        //the next element should be the Text value
//                        reader.Read();
//                        if (reader.Value.Equals("Referer"))
//                        {
//                            reader.Read();//name endtag
//                            reader.Read();//whitespace
//                            reader.Read();//value starttag
//                            reader.Read();//value innerText
//                            //Console.WriteLine(reader.Value);
//                            //Stores the referer with the unique value of the full path
//                            //Would really long paths break this? Theoretically, yes. Practically, maybe?
//                            //Also, a dictionary may not be a good idea since our paths aren't unique (think api.disney.com v1 calls)
//                            hostRefLocal.Add(path, reader.Value);
//                        }
//                    }
//                }
//            } while (reader.Read());
//        }

//        public static void ReadEvent(XmlReader reader, out int rCount, out int tCount, XmlWriter writer)
//        {
//            rCount = 0;
//            tCount = 0;

//            writer.WriteRaw("<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>");
//            do
//            {
//                if (reader.NodeType == XmlNodeType.Element)
//                {
//                    Console.WriteLine("1");
//                    writer.WriteStartElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
//                    //The bottom WriteAttributes is where we rewrite the host to contain the referrer
//                    if (reader.Name.Equals("transaction"))
//                    {
//                        //will have to move one by one through attributes to copy all, and replace path with host/path and host with referrer
//                        string hostToBeReplaced = reader.GetAttribute("host").ToString();
//                        string referer = "No referer";
//                        //this is where we'll have to use our dictionary

//                        for (int i = 0; i < reader.AttributeCount; i++)
//                        {
//                            if (!reader.Name.Equals("transaction"))
//                            {
//                                if (reader.Name.Equals("host"))
//                                {
//                                    writer.WriteAttributeString(reader.Name, "    " + referer);
//                                }
//                                else if (reader.Name.Equals("path"))
//                                {
//                                    writer.WriteAttributeString(reader.Name, hostToBeReplaced + reader.GetAttribute(reader.Name));
//                                }
//                                else
//                                {
//                                    writer.WriteAttributeString(reader.Name, reader.GetAttribute(reader.Name));
//                                }
//                            }
//                            reader.MoveToNextAttribute();
//                        }

//                    }
//                    //can we just move this writeattributes at the end of the transaction element?
//                    //no, since there are nested elements that may have their own attributes

//                    //can we go backwards from the very beginning?
//                    //We can, but that would take time to flip things, and XMLReader/Writer may hiccup

//                    //can we delay the writing by storing the xml into a large string and writeRaw?
//                    //storing the values may be an issue

//                    //can we read the referrer earlier?
//                    //no, it's read forward only

//                    //can we extract referers first, while tagging it with the the endpoint
//                    //AND THEN run the read write again? With this info, we can check for path, and if the path matches, tag it with the referrer
//                    writer.WriteAttributes(reader, true);
//                    //The below ensures that ssl tags have a short end tag
//                    if (reader.Name.Equals("ssl"))
//                    {
//                        writer.WriteEndElement();
//                    }
//                }
//                else if (reader.NodeType == XmlNodeType.EndElement)
//                {
//                    Console.Write("\n3");
//                    writer.WriteEndElement();
//                    //WriteAttributes(reader, writer);//is this line neccessary? No it isn't.
//                }
//                //takes care of all other elements' attributes
//                else if (reader.NodeType == XmlNodeType.Attribute)
//                {
//                    writer.WriteAttributes(reader, true);
//                }
//                //CDATA is being read as a Nodetype when it isn't
//                else if (reader.NodeType == XmlNodeType.Text)
//                {
//                    Console.Write("2");
//                    writer.WriteString(reader.Value);
//                }
//                else if (reader.NodeType.Equals(XmlNodeType.CDATA))
//                {
//                    writer.WriteRaw("<![CDATA[" + reader.Value + "]]>");
//                }



//            } while (reader.Read());



//            //while (reader.Read()) {
//            //    if (reader.NodeType == XmlNodeType.Element) {
//            //        start = reader.Name;
//            //        reader.Read();
//            //        if (reader.NodeType == XmlNodeType.Text)
//            //        {
//            //            middle = reader.Value;
//            //            reader.Read();
//            //            if (reader.NodeType == XmlNodeType.EndElement) {
//            //                end = reader.Name;
//            //            }
//            //        }
//            //        else if (reader.NodeType == XmlNodeType.EndElement) {
//            //            end = reader.Name;
//            //        }
//            //        else
//            //        {
//            //            end = reader.Name;
//            //        }
//            //    }
//            //    Console.WriteLine("<"+start+">" + middle + "</"+end+">");
//            //}

//            //while (reader.Read())
//            //{
//            //    rCount++;
//            //    if (reader.NodeType == XmlNodeType.Element && reader.Name == "transaction")
//            //    {

//            //    }
//            //    else if (reader.NodeType == XmlNodeType.Element && reader.Name == "name") {
//            //        //the next element should be the Text value
//            //        reader.Read();
//            //        if (reader.NodeType == XmlNodeType.Text)
//            //        {
//            //            if (reader.Value == "Referer")
//            //            {
//            //                reader.Read();//name endtag
//            //                reader.Read();//value starttag
//            //                reader.Read();//value innertext
//            //                Console.WriteLine(reader.Value);
//            //            }
//            //        }
//            //    }
//            //    else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "transaction") {
//            //        tCount++;
//            //        break;
//            //    }
//            //}
//        }

//        //public static void ReadEvent(XmlReader reader, out string ip,
//        //                            out string httpMethod, out string file,
//        //                            out string date, out string referrer, out int rCount)
//        //{

//        //    ip = httpMethod = file = date = referrer = null;
//        //    rCount = 0;
//        //    //while (reader.Read() && reader.NodeType != XmlNodeType.EndElement)
//        //    while (reader.Read())
//        //    {
//        //        //This is where we'll have to traverse the log. Supposedly, this method will perform this in chunks, so the process shouldn't freeze up
//        //        //Probably a good idea to introduce a visible loading bar here
//        //        rCount++;
//        //        if (reader.NodeType == XmlNodeType.Element)
//        //        {

//        //            if (reader.Name == ipKey)
//        //            {
//        //                ip = reader.ReadString();
//        //            }
//        //            else if (reader.Name == httpMethodKey)
//        //            {
//        //                httpMethod = reader.ReadString();
//        //            }
//        //            else if (reader.Name == fileKey)
//        //            {
//        //                file = reader.ReadString();
//        //            }
//        //            else if (reader.Name == dateKey)
//        //            {
//        //                date = reader.ReadString();
//        //                // reader.Read(); // consume end tag
//        //            }
//        //            else if (reader.Name == referrerKey)
//        //            {
//        //                referrer = reader.ReadString();
//        //            }
//        //            else if (reader.Name == "transaction") {

//        //            }
//        //        }//if 
//        //    }//while   
//        //}


//        //1. Run through the xml file while ID'ing each transaction (change the METHOD attribute to a value and increment)
//        //2. Run through the xml file a second time gathering the referers into a dictionary with the corresponding ID
//        //3. Run through the xml file a third time, if transaction key is present in the dictionary, rewrite path into host with path, and replace the old host with the referer
//        public static void Main(string[] args)
//        {
//            //string ip, httpMethod, file, date, referrer;
//            int readCount = 0;
//            int transactionCount = 0;
//            //setup XmlNameTable with strings we'll be using for comparisons
//            //looks like this initializes our static string values
//            //XmlNameTable xnt = new NameTable();
//            //ipKey = xnt.Add("ip");
//            //httpMethodKey = xnt.Add("http_method");
//            //fileKey = xnt.Add("file");
//            //dateKey = xnt.Add("date");
//            //referrerKey = xnt.Add("referrer");

//            string xmlFile = "test2.xml";
//            Dictionary<string, string> hostRef = new Dictionary<string, string>();

//            //load XmlTextReader using XmlNameTable above 
//            XmlTextReader xr2 = new XmlTextReader(xmlFile);
//            //xr2.WhitespaceHandling = WhitespaceHandling.Significant;
//            xr2.WhitespaceHandling = WhitespaceHandling.All;

//            XmlValidatingReader vr2 = new XmlValidatingReader(xr2);
//            vr2.ValidationType = ValidationType.None;
//            vr2.EntityHandling = EntityHandling.ExpandEntities;

//            StreamWriter sw2 =
//              new StreamWriter("temp.xml", false, Encoding.UTF8);
//            XmlWriter xw2 = new XmlTextWriter(sw2);

//            ExtractReferers(vr2, hostRef, xw2);

//            //load XmlTextReader using XmlNameTable above 
//            XmlTextReader xr = new XmlTextReader(xmlFile);
//            //xr.WhitespaceHandling = WhitespaceHandling.Significant;
//            xr.WhitespaceHandling = WhitespaceHandling.All;

//            XmlValidatingReader vr = new XmlValidatingReader(xr);
//            vr.ValidationType = ValidationType.None;
//            vr.EntityHandling = EntityHandling.ExpandEntities;


//            StreamWriter sw =
//              new StreamWriter("logfile-archive.xml", false, Encoding.UTF8);
//            XmlWriter xw = new XmlTextWriter(sw);

//            vr.MoveToContent(); // Move to document element   

//            do
//            {
//                ReadEvent(vr, out readCount, out transactionCount, xw);
//                vr.Read(); //move to next <event> element or end tag of <logfile>
//                //should only cycle once
//            } while (vr.NodeType == XmlNodeType.Element);

//            Console.WriteLine("Done");

//            vr.Close();
//            xw.Close();
//        }
//    }


//}
