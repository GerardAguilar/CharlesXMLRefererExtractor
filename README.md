# CharlesXMLRefererExtractor


1. Export Charles Log File (.chls) as an XML Session File (.xml)

2a. Rename as testlog.xml and place into Debug folder of the XMLReferrerExtractor project

2b. CharlesXMLRefererExtractor-master\XMLReferrerExtractor\bin\Debug

2c. If the debug folder doesn't show up, make sure you built the project at least once in Visual Studio.

3a. Replace: <?xml version="1.0" encoding="UTF-8"?><!DOCTYPE charles-session SYSTEM "http://www.charlesproxy.com/dtd/charles-session-1_0.dtd">

3b. With: <?xml version="1.0" encoding="iso-8859-1"?>

4a. Run XMLReferrerExtractor.exe (found in the same folder) or with Visual Studio open, choose "Start without Debugging"

4b. Open the resulting reformated.xml file in Charles Proxy


Issues:

1. While unknown endpoints do not have issues, Unknown HOSTS (which come out as blue folders in Charles Proxy) have not been accounted for and may break the application.

2. This application should be used for Charles Proxy v3. Charles Proxy v4 has a different export file type for XML Session File (.chlsx). In this case, just export it with the name testlog.xml, and it should still be in the proper .xml format.
