using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml.Serialization;

namespace api.standard.ejucloud.com.Controllers
{
    public class MDXQueryController : Controller
    {
        // GET: MDXQuery
        public ContentResult MDXQuery(string MDXsql)
        {

            try
            {
                int i = 0;

                DataSet ds = MDXHelper.ExecuteDataSet(MDXHelper.MDXConnectString, MDXsql);

                string content = ds.ToXml();

                return Content(content);
            }
            catch(Exception ex)
            {
                return Content(ex.Message);

            }
        }

       

    }

    public static class Extensions
    {
        public static string ToXml(this DataSet ds)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (TextWriter streamWriter = new StreamWriter(memoryStream))
                {
                    var xmlSerializer = new XmlSerializer(typeof(DataSet));
                    xmlSerializer.Serialize(streamWriter, ds);
                    return Encoding.UTF8.GetString(memoryStream.ToArray());
                }
            }
        }
    }


}