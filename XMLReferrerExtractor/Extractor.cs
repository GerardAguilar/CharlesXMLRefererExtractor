using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

//todo: account for endpoints marked as "unknown"

namespace XMLReferrerExtractor
{
    //1. Run through the xml file while ID'ing each transaction (change the METHOD attribute to a value and increment)
    //2. Run through the xml file a second time gathering the referers into a dictionary with the corresponding ID
    //3. Run through the xml file a third time, if transaction key is present in the dictionary, rewrite path into host with path, and replace the old host with the referer
    class Extractor
    {
        /// <summary>
        /// Goes through a charlesLog.xml file and changes the Method attribute to IDs
        /// <param name="xmlFile">Charles Log XML file name</param>
        /// </summary>
        /// <returns>Count of Transactions</returns>
        internal static int GenerateTransactionIdentifiers(string xmlFile) {
            int transactionCount=0;
            #region Xml Reader and Writer initialization
            XmlTextReader reader = new XmlTextReader(xmlFile);
            Encoding utf8withoutbom = new UTF8Encoding(false);
            reader.WhitespaceHandling = WhitespaceHandling.All;
            XmlValidatingReader vr = new XmlValidatingReader(reader);
            //XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
            vr.ValidationType = ValidationType.None;
            vr.EntityHandling = EntityHandling.ExpandEntities;
            StreamWriter sw = new StreamWriter("identifier.xml", false, utf8withoutbom);
            XmlWriter writer = new XmlTextWriter(sw);

            #endregion
            #region Read log step by step and write to identifiers.xml 
            writer.WriteRaw("<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>");
            reader.MoveToContent();
            do
            {
                #region Encounter start elements
                if (reader.NodeType == XmlNodeType.Element)
                {
                    writer.WriteStartElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
                    if (reader.Name.Equals("transaction"))
                    {
                        //cycle through all attributes of the transaction
                        for (int i = 0; i < reader.AttributeCount; i++)
                        {
                            //look for the method attribute
                            if (reader.Name.Equals("method"))
                            {
                                //change it to transaction Count
                                writer.WriteAttributeString(reader.Name, transactionCount.ToString("00000"));
                                transactionCount++;
                            }
                            else if (reader.Name.Equals("transaction"))
                            {
                                //skip
                            }
                            else
                            {
                                //write the rest of the attributes
                                writer.WriteAttributeString(reader.Name, reader.GetAttribute(reader.Name));
                            }
                            reader.MoveToNextAttribute();
                        }
                    }
                    writer.WriteAttributes(reader, true);
                    if (reader.IsEmptyElement)
                    {
                        writer.WriteEndElement();
                    }
                    ////The below checks short end tags
                    //if (reader.Name.Equals("ssl")
                    //    || reader.Name.Equals("alpn"))
                    //{
                    //    writer.WriteEndElement();
                    //}
                    //else if (reader.Name.Equals("first-line"))
                    //{
                    //    //if the first-line has a short end tag, maintain it as a short end tag
                    //    if (reader.IsEmptyElement)
                    //    {
                    //        writer.WriteEndElement();
                    //    }
                    //}



                }
                #endregion
                #region Encounter end elements
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    //Console.Write("\n3");
                    writer.WriteEndElement();
                    //WriteAttributes(reader, writer);//is this line neccessary? No it isn't.
                }
                //else if (reader.IsEmptyElement) {
                //    writer.WriteEndElement();
                //}
                #endregion
                #region Encounter attributes outside of elements
                //takes care of all other elements' attributes, but this shouldn't be reached.
                else if (reader.NodeType == XmlNodeType.Attribute)
                {
                    Console.WriteLine("Attribute: " + reader.Name.ToString() + " found outside of element.");
                    writer.WriteAttributes(reader, true);
                }
                #endregion
                #region Encounter inner text
                else if (reader.NodeType == XmlNodeType.Text)
                {
                    //Console.Write("2");
                    writer.WriteString(reader.Value);
                }
                #endregion
                #region Encounter CDATA
                else if (reader.NodeType.Equals(XmlNodeType.CDATA))
                {
                    writer.WriteRaw("<![CDATA[" + reader.Value + "]]>");
                }
                #endregion
            } while (reader.Read());
            #endregion

            writer.Close();
            reader.Close();

            return transactionCount;
        }

        /// <summary>
        /// Create a referer dictionary using the identifier.xml generated by GenerateTransactionIdentifiers()
        /// </summary>
        /// <returns></returns>
        internal static int GenerateRefererDictionary(Dictionary <string,string> dictionary) {
            int refererCount = 0;
            string xmlFile = "identifier.xml";
            string id = "00000";
            #region Xml Reader initialization
            XmlTextReader reader = new XmlTextReader(xmlFile);
            reader.WhitespaceHandling = WhitespaceHandling.All;
            XmlValidatingReader vr = new XmlValidatingReader(reader);
            //XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
            vr.ValidationType = ValidationType.None;
            vr.EntityHandling = EntityHandling.ExpandEntities;
            #endregion
            #region Cycle through transaction, if there's a referer, store id with referer
            reader.MoveToContent();
            do
            {
                if (reader.NodeType == XmlNodeType.Element) {
                    if (reader.Name.Equals("transaction"))
                    {
                        //method has been rewritten as ID by GenerateTransactionIdentifiers() 
                        id = reader.GetAttribute("method");
                    }
                    else if (reader.NodeType == XmlNodeType.Element && reader.Name == "name")
                    {
                        //the next element should be the Text value
                        reader.Read();
                        if (reader.NodeType == XmlNodeType.Text)
                        {
                            if (reader.Value == "Referer")
                            {
                                reader.Read();//name endtag
                                reader.Read();//value starttag
                                reader.Read();//value innertext
                                //Console.WriteLine("Referer: " + reader.Value);
                                refererCount++;
                                dictionary.Add(id, reader.Value.ToString());
                            }
                        }
                    }
                }
                
            } while (reader.Read());
            #endregion

            reader.Close();

            return refererCount;
        }


        /// <summary>
        /// Using the dictionary, if transaction id is in the dictionary, rewrite the host and path
        /// </summary>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        internal static bool RewriteRootPathAsReferer(Dictionary<string, string> dictionary, int refererCount, int transactionCount)
        {
            bool matchTransactionAndRefererCount = false;
            string xmlFile = "identifier.xml";
            int rCount = 0;
            int tCount = 0;
            string tempId = "0000";
            string originalHost = "";
            bool foundInDictionary = false;
            #region Xml Reader and Writer initialization
            Encoding utf8withoutbom = new UTF8Encoding(false);
            XmlTextReader reader = new XmlTextReader(xmlFile);
            reader.WhitespaceHandling = WhitespaceHandling.All;
            XmlValidatingReader vr = new XmlValidatingReader(reader);
            //XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
            vr.ValidationType = ValidationType.None;
            vr.EntityHandling = EntityHandling.ExpandEntities;
            StreamWriter sw = new StreamWriter("reformated.xml", false, utf8withoutbom);
            XmlWriter writer = new XmlTextWriter(sw);
            #endregion

            writer.WriteRaw("<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>");
            reader.MoveToContent();
            do
            {
                #region Encounter start elements
                if (reader.NodeType == XmlNodeType.Element)
                {
                    writer.WriteStartElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
                    if (reader.Name.Equals("transaction"))
                    {
                        //cycle through all attributes of the transaction
                        tCount++;
                        //method was turned to ID from GenerateTransactionIdentifiers()                        
                        tempId = reader.GetAttribute("method");
                        for (int i = 0; i < reader.AttributeCount; i++)
                        {
                            //look for the method attribute
                            if (reader.Name.Equals("host"))
                            {
                                //will be used for appending path
                                originalHost = reader.Value;

                                if (dictionary.ContainsKey(tempId))
                                {
                                    //rewrite host into referer
                                    if (reader.GetAttribute("protocol").Equals("https"))
                                    {
                                        writer.WriteAttributeString(reader.Name, "    Referer: " + dictionary[tempId] + " ");
                                    }
                                    else {
                                        writer.WriteAttributeString(reader.Name, "     Referer: " + dictionary[tempId] + " ");
                                    }
                                    rCount++;
                                }
                                else {
                                    if (reader.GetAttribute("protocol").Equals("https"))
                                    {
                                        writer.WriteAttributeString(reader.Name, "    " + "No Referer ");
                                    }
                                    else
                                    {
                                        writer.WriteAttributeString(reader.Name, "      " + "No Referer ");
                                    }
                                }
                            }
                            else if (reader.Name.Equals("path")) {
                                //rewrite path into originalHost with referer
                                writer.WriteAttributeString(reader.Name, originalHost + reader.GetAttribute(reader.Name));
                            }
                            else if (reader.Name.Equals("transaction"))
                            {
                                //skip
                            }
                            else
                            {
                                //write the rest of the attributes
                                writer.WriteAttributeString(reader.Name, reader.GetAttribute(reader.Name));
                            }
                            reader.MoveToNextAttribute();
                        }
                    }
                    writer.WriteAttributes(reader, true);

                    ////The below ensures that ssl tags have a short end tag
                    //if (reader.Name.Equals("ssl")
                    //    || reader.Name.Equals("alpn"))
                    //{
                    //    writer.WriteEndElement();
                    //}
                    //else if (reader.Name.Equals("first-line"))
                    //{
                    //    //if the first-line has a short end tag, maintain it as a short end tag
                    //    if (reader.IsEmptyElement)
                    //    {
                    //        writer.WriteEndElement();
                    //    }
                    //}
                    if (reader.IsEmptyElement) {
                        writer.WriteEndElement();
                    }
                }
                #endregion
                #region Encounter end elements
                //else if ((reader.NodeType == XmlNodeType.EndElement) || reader.IsEmptyElement)
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    //Console.Write("\n3");
                    writer.WriteEndElement();
                    //WriteAttributes(reader, writer);//is this line neccessary? No it isn't.
                }
                #endregion
                #region Encounter attributes outside of elements
                //takes care of all other elements' attributes, but this shouldn't be reached.
                else if (reader.NodeType == XmlNodeType.Attribute)
                {
                    Console.WriteLine("Attribute: " + reader.Name.ToString() + " found outside of element.");
                    writer.WriteAttributes(reader, true);
                }
                #endregion
                #region Encounter inner text
                else if (reader.NodeType == XmlNodeType.Text)
                {
                    //Console.Write("2");
                    writer.WriteString(reader.Value);
                }
                #endregion
                #region Encounter CDATA
                else if (reader.NodeType.Equals(XmlNodeType.CDATA))
                {
                    writer.WriteRaw("<![CDATA[" + reader.Value + "]]>");
                }
                #endregion
            } while (reader.Read());

            writer.Close();
            reader.Close();

            if (rCount == refererCount) {
                if (tCount == transactionCount)
                    matchTransactionAndRefererCount = true;
            }

            return matchTransactionAndRefererCount;
        }
        internal static void ScanForEmpty(string xmlFile) {
            #region Xml Reader initialization
            XmlTextReader reader = new XmlTextReader(xmlFile);
            reader.WhitespaceHandling = WhitespaceHandling.All;
            XmlValidatingReader vr = new XmlValidatingReader(reader);
            //XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
            vr.ValidationType = ValidationType.None;
            vr.EntityHandling = EntityHandling.ExpandEntities;
            #endregion

            reader.MoveToContent();
            do
            {
                if (reader.IsEmptyElement) {
                    if (reader.Name.Equals("ssl"))
                    {

                    }
                    else if (reader.Name.Equals("first-line"))
                    {

                    }
                    else if (reader.Name.Equals("alpn"))
                    {

                    }
                    else {
                        Console.WriteLine("Empty: " + reader.Name);
                    }
                }

            } while (reader.Read());
        }

        public static void Main(string[] args) {
            int transactionCount = 0;
            int refererCount = 0;
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            bool transactionAndRefererIntegrity = false;
            string xmlFile = "testlog.xml";

            //ScanForEmpty(xmlFile);

            transactionCount = GenerateTransactionIdentifiers(xmlFile);
            Console.WriteLine("Transaction Count: {0}", transactionCount);

            refererCount = GenerateRefererDictionary(dictionary);
            Console.WriteLine("Referer Count: {0}", refererCount);

            transactionAndRefererIntegrity = RewriteRootPathAsReferer(dictionary, refererCount, transactionCount);
            Console.WriteLine("Transaction/Referer check: " + transactionAndRefererIntegrity);

        }
    }
}
