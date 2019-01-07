using System.IO;
using System.Text;
using System.Xml.Linq;

namespace TaskCoordinator.SSSB
{
    public static class SSSBMessageExtension
    {
        public static XElement GetMessageXML(this SSSBMessage message)
        {
            using (MemoryStream ms = new MemoryStream(message.Body))
            using (StreamReader sr = new StreamReader(ms, Encoding.Unicode, true))
            {
                return XElement.Load(sr);
            }
        }

        public static byte[] ConvertToBytes(this XElement xml)
        {
            return Encoding.Unicode.GetBytes(xml.ToString());
        }
    }
}
