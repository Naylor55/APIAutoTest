using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace BKS_Sys.Web.Models
{
    public class DBHelper
    {
        private static string DBConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["OfficeAutoDB"].ConnectionString;
        private static DBHelper dBHelper = new DBHelper();
        private DBHelper()
        {  
        }
        /// <summary>
        /// 实例化DBHelper对象
        /// </summary>
        /// <returns></returns>
        public static DBHelper Instance()
        {
            return dBHelper;
        }
        /// <summary>
        /// 打开数据库连接
        /// </summary>
        private static SqlConnection DBOpen()
        {
            SqlConnection conn = new SqlConnection(DBConnectString);
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            return conn;
        }
        /// <summary>
        /// 关闭数据库连接
        /// </summary>
        private static void DBClose(SqlConnection conn)
        {
            if (conn == null)
            {
                return;
            }
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
        }
        /// <summary>
        /// 执行SQL语句获取数据集
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>DataTable数据集</returns>
        public DataTable GetDataTableBySql(string sql)
        {
            SqlConnection conn =  DBOpen();
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(sql, conn);
            try
            {
                da.Fill(dt);
                return dt;
            }
            catch
            {
                return null;
            }
            finally
            {
                DBClose(conn);
            }
        }
        /// <summary>
        /// 执行SQL语句
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns>是否执行成功</returns>
        public bool ExcuteSql(string sql)
        {
            SqlConnection conn = DBOpen();
            SqlCommand cmd = new SqlCommand(sql, conn);
            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                DBClose(conn);
            }
        }
        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="proName">存储过程名称</param>
        /// <param name="paras">存储过程参数</param>
        /// <returns>是否执行成功</returns>
        public bool ExcuteProcedure(string proName, SqlParameter[] paras)
        {
            SqlConnection conn = DBOpen();
            SqlCommand cmd = new SqlCommand(proName, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            for (int i = 0; i < paras.Length; i++)
            {
                cmd.Parameters.Add(paras[i]);
            }


            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                DBClose(conn);
            }

        }


        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="proName">存储过程名称</param>
        /// <param name="paras">存储过程参数</param>
        /// <returns>是否执行成功</returns>
        public DataTable ExcuteProcedureDatatable(string proName)
        {
            SqlConnection conn = DBOpen();
            SqlCommand cmd = new SqlCommand(proName, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            try
            {
                da.Fill(dt);
                return dt;
            }
            catch
            {
                return null;
            }
            finally
            {
                DBClose(conn);
            }
        }


        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="proName">存储过程名称</param>
        /// <param name="paras">存储过程参数</param>
        /// <returns>是否执行成功</returns>
        public bool ExcuteProcedure(string proName)
        {
            SqlConnection conn = DBOpen();
            SqlCommand cmd = new SqlCommand(proName, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                DBClose(conn);
            }
        }

        /// <summary>
        /// 执行存储过程获得数据集
        /// </summary>
        /// <param name="proName">存储过程名</param>
        /// <param name="paras">存储过程参数</param>
        /// <returns>DataTable数据集</returns>
        public DataTable GetDataTableByProcedure(string proName, SqlParameter[] paras)
        {
            SqlConnection conn = DBOpen();
            SqlCommand cmd = new SqlCommand(proName, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            for (int i = 0; i < paras.Length; i++)
            {
                cmd.Parameters.Add(paras[i]);
            }
            try
            {
                da.Fill(dt);
                return dt;
            }
            catch
            {
                return null;
            }
            finally
            {
                DBClose(conn);
            }
        }
    }
}