using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Microsoft.AnalysisServices.AdomdClient;
using System.Configuration;
using System.IO;
using System.Xml.Serialization;

namespace api.standard.ejucloud.com
{
    public class MDXHelper
    {

        public static readonly string MDXConnectString = ConfigurationManager.ConnectionStrings["MDXConnectString"].ConnectionString;
        public static CellSet ExecuteCellSet(string connectionString, string strMdx)
        {
            using (Microsoft.AnalysisServices.AdomdClient.AdomdConnection conn = new AdomdConnection())
            {
                conn.ConnectionString = connectionString;
                conn.Open();

                using (AdomdCommand command = conn.CreateCommand())
                {
                    try
                    {
                        command.CommandText = strMdx;

                        return command.ExecuteCellSet();
                    }
                    catch (Exception ex)
                    {
                        string str = ex.Message;
                        return null;
                    }
                }
            }
        }

        public static DataTable CellSetToDataTable(CellSet cs, bool nulltoZero)
        {

            DataTable dt = new DataTable();

            if (cs.Axes[1].Positions.Count == 0)
                return dt;

            DataColumn dc = new DataColumn();

            DataRow dr = null;
            // 第一列：必有为维度描述（行头） 
            //dt.Columns.Add(new DataColumn("Description"));
            for (int i = 0; i < cs.Axes[1].Positions[0].Members.Count; i++)
            {
                string[] strName = cs.Axes[1].Positions[0].Members[i].LevelName.Split('.');
                string strTitle = strName[strName.Length - 1].Trim(new char[] { '[', ']' });
                if (strTitle.ToLower() == "(all)")
                    strTitle = strName[strName.Length - 2].Trim(new char[] { '[', ']' });
                dc = new DataColumn();
                dc.ColumnName = strTitle;
                dt.Columns.Add(dc);

            }
            // 生成数据列对象 
            string name;

            foreach (Position p in cs.Axes[0].Positions)
            {
                dc = new DataColumn();
                name = "";
                foreach (Member m in p.Members)
                {
                    name = name + m.Caption + "$";
                }
                name = name.Trim('$');
                dc.ColumnName = name;
                dc.DataType = typeof(float);
                dc.AllowDBNull = true;
                dt.Columns.Add(dc);

            }
            // 添加行数据 
            int pos = 0;
            foreach (Position py in cs.Axes[1].Positions)
            {
                dr = dt.NewRow();
                // 维度描述列数据（行头） 
                name = "";

                int rowFixCount = py.Members.Count;
                for (int i = 0; i < rowFixCount; i++)
                {

                    dr[i] = py.Members[i].Caption;
                    //name = name + m.Caption + "\r\n";
                }


                // 数据列 
                for (int x = 0; x < cs.Axes[0].Positions.Count; x++)
                {
                    object value = null;

                    try
                    {
                        value = cs[pos++].Value;
                    }
                    catch
                    {
                        value = DBNull.Value;
                    }
                    if (value == null)
                    {
                        if (nulltoZero == true)
                        {
                            value = 0;
                        }
                        else
                        {
                            value = DBNull.Value;
                        }
                    }
                    else
                    {
                        try
                        {
                            double dValue = double.Parse(value.ToString());
                        }
                        catch
                        {
                            value = DBNull.Value;
                        }
                    }
                    dr[x + rowFixCount] = value;
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }

        private static DataTable CellSetToDataTable(CellSet cs)
        {

            DataTable dt = new DataTable();

            if (cs.Axes[1].Positions.Count == 0)
                return dt;

            DataColumn dc = new DataColumn();

            DataRow dr = null;
            // 第一列：必有为维度描述（行头） 
            //dt.Columns.Add(new DataColumn("Description"));
            for (int i = 0; i < cs.Axes[1].Positions[0].Members.Count; i++)
            {
                string[] strName = cs.Axes[1].Positions[0].Members[i].LevelName.Split('.');
                string strTitle = strName[strName.Length - 1].Trim(new char[] { '[', ']' });
                if (strTitle.ToLower() == "(all)")
                    strTitle = strName[strName.Length - 2].Trim(new char[] { '[', ']' });
                dc = new DataColumn();
                dc.ColumnName = strTitle;
                dt.Columns.Add(dc);

            }
            // 生成数据列对象 
            string name;

            foreach (Position p in cs.Axes[0].Positions)
            {
                dc = new DataColumn();
                name = "";
                foreach (Member m in p.Members)
                {
                    name = name + m.Caption + "$";
                }
                name = name.Trim('$');
                dc.ColumnName = name;
                // dc.DataType = typeof(float);
                dc.AllowDBNull = true;
                dt.Columns.Add(dc);

            }
            // 添加行数据 
            int pos = 0;
            foreach (Position py in cs.Axes[1].Positions)
            {
                dr = dt.NewRow();
                // 维度描述列数据（行头） 
                name = "";

                int rowFixCount = py.Members.Count;
                for (int i = 0; i < rowFixCount; i++)
                {

                    dr[i] = py.Members[i].Caption;
                    //name = name + m.Caption + "\r\n";
                }


                // 数据列 
                for (int x = 0; x < cs.Axes[0].Positions.Count; x++)
                {
                    object value = null;

                    try
                    {
                        value = cs[pos++].Value;
                    }
                    catch
                    {
                        value = "-";
                    }
                    if (value == null)
                    {
                        value = 0;
                    }
                    else
                    {
                        try
                        {
                            double dValue = double.Parse(value.ToString());
                        }
                        catch
                        {
                            value = "-";
                        }
                    }
                    dr[x + rowFixCount] = value;
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }

        public static DataSet ExecuteDataSet(string connectionString, string strMdx)
        {
            using (Microsoft.AnalysisServices.AdomdClient.AdomdConnection conn = new AdomdConnection())
            {
                conn.ConnectionString = connectionString;
                conn.Open();

                using (AdomdCommand command = conn.CreateCommand())
                {
                    command.CommandText = strMdx;
                    command.CommandTimeout = 60;
                    CellSet cellSet = command.ExecuteCellSet();
                    System.Data.DataTable table = CellSetToDataTable(cellSet);

                    System.Data.DataSet ds = new DataSet();
                    ds.Tables.Add(table);

                    return ds;
                }
            }
        }

        public static DataTable FomartTable(DataTable dt)
        {
            DataTable dt_temp = dt.Clone();
            foreach (DataRow dr in dt.Rows)
            {
                bool flag = false;
                for (int i = 0; i < dr.ItemArray.Length; i++)
                {
                    int count = 0;
                    if (int.TryParse(dr[i].ToString(), out count) && count > 0)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    dt_temp.ImportRow(dr);
                }
            }
            return dt_temp;
        }

        


    }
}