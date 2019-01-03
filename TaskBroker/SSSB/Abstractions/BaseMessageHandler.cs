using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TaskCoordinator.SSSB;

namespace TaskBroker.SSSB
{
    public abstract class BaseMessageHandler<T>: IMessageHandler<T>
    {
        protected object SyncRoot = new object();

        protected virtual string GetName()
        {
            return nameof(BaseMessageHandler<T>);
        }

        protected static NameValueCollection ExtractParameters(XElement message_xml)
        {
            NameValueCollection nvcol = new NameValueCollection();

            XElement params_xml = message_xml.Element("params");
            if (params_xml != null && params_xml.HasElements)
            {
                foreach (XElement param in params_xml.Elements("param"))
                {
                    string name = param.Attribute("name").Value;
                    string value = null;
                    if (param.Attribute("value") != null)
                        value = param.Attribute("value").Value;
                    nvcol.Add(name, value);
                }
            }
            return nvcol;
        }

        /// <summary>
        /// Конвертирует байты в XML сообщение
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        protected static XElement GetMessageXML(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            using (StreamReader sr = new StreamReader(ms, Encoding.Unicode, true))
            {
               return XElement.Load(sr);
            }
        }

        protected static void GetMessageAttributes(XElement message_xml, out int? taskID, out bool isMultiStepTask, out DateTime eventDate, out NameValueCollection parameters)
        {
            taskID = null;
            isMultiStepTask = false;
            eventDate = DateTime.Now;
            parameters = null;
            taskID = int.Parse(message_xml.Element("task").Value);
            string multyStep = message_xml.Element("multy-step").Value;
            if (!string.IsNullOrEmpty(multyStep))
                bool.TryParse(multyStep, out isMultiStepTask);
            string str_date = message_xml.Element("date").Value;
            eventDate = DateTime.ParseExact(str_date, "yyyy-MM-dd HH:mm:ss", null);
            parameters = ExtractParameters(message_xml);
        }

        public abstract Task<T> HandleMessage(ISSSBService sender, T args);
    }
}
