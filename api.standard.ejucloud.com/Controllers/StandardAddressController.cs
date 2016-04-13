using System;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using System.Web.Mvc;
using System.Runtime.Serialization.Json;
using System.Data;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Configuration;

namespace api.standard.ejucloud.com.Controllers
{
    public class StandardAddressController : Controller
    {
        static string getHttp(string url, string queryString)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url + queryString);

            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";
            httpWebRequest.Timeout = 20000;

            string responseContent = "";
            HttpWebResponse httpWebResponse;
            StreamReader streamReader;
            try
            {
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                streamReader = new StreamReader(httpWebResponse.GetResponseStream());
                responseContent = streamReader.ReadToEnd();
                httpWebResponse.Close();
                streamReader.Close();
            }
            catch
            {
                responseContent = "";

            }
            finally
            {
            }

            return responseContent;
        }

        static string getAddrRoadName(string addr)
        {
            return Regex.Match(addr, @"(\w*市)*(\w*区)*(?'A'\w*?)(\d+|$)").Success ? Regex.Match(addr, @"(\w*市)*(\w*区)*(?'A'\w*?)(\d+|$)").Result(@"${A}") : "";
        }

        static string getAddrNum(string addr)
        {
            return Regex.Match(addr, @"(?'A'\d+)").Success ? Regex.Match(addr, @"(?'A'\d+)").Result(@"${A}") : "";
        }

        // GET: AddressMatch
        //api/addressmatch/?address=bbc&city=xxx&ak=fdads
        public JsonpResult AddressMatch(string address, string city, string ak)
        {
            Common.AddResponseHeader();

            int returnCode = 0x00;
            string errMessage = "";
            string lng = "";
            string lat = "";
            string unit = "";
            int unitConfidence = 0;
            string EstateID = "";
            int EstateIDConfidence = 0;

            try
            {
                address = Regex.Match(address,"^"+city).Success?address:city+address;

                string geocoderURL = @"http://api.map.baidu.com/geocoder/v2/";
                string geocoderQueryString = @"?address=" + address + "&output=json&ak=" + ak;

                string rtn = getHttp(geocoderURL, geocoderQueryString);

                returnCode = 0x04;
                errMessage = "get geocoder fail";

                if (rtn.Length > 0)
                {
                    JObject jo = JObject.Parse(rtn);
                    if (((int)jo["status"]) == 0)
                    {
                        lng = (string)jo["result"]["location"]["lng"];
                        lat = (string)jo["result"]["location"]["lat"];

                        int pagesize = 20;
                        string placeURL = @"http://api.map.baidu.com/place/v2/search";
                        string placeQueryString = @"?output=json&query=%E5%B0%8F%E5%8C%BA&page_size=" + pagesize.ToString() + "&page_num=0&scope=1&location=" + lat + "," + lng + "&radius=500&ak=" + ak;

                        string rtn2 = getHttp(placeURL, placeQueryString);

                        returnCode = 0x04;
                        errMessage = "get place fail";

                        if (rtn2.Length > 0)
                        {
                            JObject jo2 = JObject.Parse(rtn2);

                            if (((int)jo2["status"]) == 0)
                            { 
                                int total = (int)jo2["total"];
                            
                                for (int k = 0; k < (total < pagesize ? total : pagesize); k++)
                                {
                                    if (k == 0)
                                    {
                                        unit = (string)jo2["results"][k]["name"];
                                        unitConfidence = 10;

                                    }
                                    if ((Regex.Match((string)jo2["results"][k]["address"], getAddrRoadName(address)).Success) && (Regex.Match((string)jo2["results"][k]["address"], getAddrNum(address)).Success))
                                    {
                                        unit = (string)jo2["results"][k]["name"];
                                        unitConfidence = 80;
                                        break;
                                    }
                                }

                                returnCode = 0x02;
                                errMessage = "get unit fail";

                                if (unit.Length > 0)
                                {
                                    string selectURL = ConfigurationManager.AppSettings["SelectURL"].ToString();
                                    string selecttQueryString = "?q=MultiName:" + unit + "&wt=json&indent=true&rows=5&fq=CityName:" + city;

                                    string rtn3 = getHttp(selectURL, selecttQueryString);

                                    returnCode = 0x02;
                                    errMessage = "get select fail";

                                    if (rtn3.Length > 0)
                                    {
                                        JObject jo3 = JObject.Parse(rtn3);

                                        int rows = (int)jo3["responseHeader"]["params"]["rows"];

                                        try
                                        {
                                            for (int j = 0; j < rows; j++)
                                            {
                                                if (j == 0)
                                                {
                                                    EstateID = (string)jo3["response"]["docs"][j]["EstateID"];
                                                    EstateIDConfidence = 10;
                                                }
                                                if ((Regex.Match((string)jo3["response"]["docs"][j]["EstateName"], unit).Success) || ((Regex.Match((string)jo3["response"]["docs"][j]["Address"], getAddrRoadName(address)).Success) && (Regex.Match((string)jo3["response"]["docs"][j]["Address"], getAddrNum(address)).Success)))
                                                {
                                                    EstateID = (string)jo3["response"]["docs"][j]["EstateID"];
                                                    EstateIDConfidence = 80;


                                                    break;
                                                }

                                            }

                                            if (EstateID.Length > 0)
                                            {
                                                returnCode = 0x00;
                                                errMessage = "";

                                            }

                                        }
                                        catch
                                        {
                                            returnCode = 0x03;
                                            errMessage = "select return data error";

                                        }

                                    }
                                }
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                returnCode = 0x01;
                errMessage = ex.Message;
            }

            return this.Jsonp(new
            {
                ErrorCode = returnCode
                ,ErrMsg = errMessage
                ,ADDRESS = address
                ,LNG = lng
                ,LAT = lat
                ,UNIT = unit
                ,UNITConfidence = unitConfidence
                ,ESTATEID = EstateID
                ,ESTATEIDConfidence = EstateIDConfidence

            });
        }

        // GET: StandardAddress
        //api/standarduseraddress/?city=abc&address=bbc&ak=fdads
        public JsonpResult StandardUserAddress(string city,string address,string ak)
        {
            Common.AddResponseHeader();
            try
            {
                string sql1 = "select top 10 unit_id as unitid,display_project_name as displayprojectname,address,region from [TB_UNIT_MAIN] with(nolock) where state=1";
                DataSet result1 = SimpleDataHelper.Query(SimpleDataHelper.MSConnectionString, sql1);
                var EstateInfo = DataTableToListModel<EstateInfo>.ConvertToModel(result1.Tables[0]);



                return this.Jsonp(new
                {
                    ErrorCode = 0x00,
                    EstateInfo
                });
            }
            catch (Exception ex)
            {
                return this.Jsonp(new { ErrorCode = 0x01, ErrMsg = ex.Message });
            }

        }

        //用户信息
        class EstateInfo
        {
            public string unitid { get; set; }

            public string displayprojectname { get; set; }

            public string address { get; set; }

            public string region { get; set; }

        }

    }
}