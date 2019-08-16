using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using WebERP;
using System.Data;
using System.Data.Common;

namespace InpatientTPRWS
{
    /// <summary>
    ///InpatientTPRWS 的摘要描述
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允許使用 ASP.NET AJAX 從指令碼呼叫此 Web 服務，請取消註解下列一行。
    [System.Web.Script.Services.ScriptService]
    public class InpatientTPRWS : System.Web.Services.WebService
    {
        public class PatientInfo
        {
            public string PatNO { get; set; } //病歷號
            public string PatID { get; set; } //身份證號
            public string PatName { get; set; } //病人姓名
            public string Sex { get; set; } //性別 M/F
            public string Birthday { get; set; }  //出生年月日  0800601 (民國年月日)
        }

        [WebMethod]
        public PatientInfo GetPatientData(string patNO)
        {
            //資料的格式
            PatientInfo info = new PatientInfo();

            //connect to database
            TPECHDBService tpech = TPECHDBService.getInstance();
            DbConnection conn = tpech.GetTPECHConnection("B");
            conn.Open();

            //selectSQL
            string sqlselect = "SELECT * FROM MEDRECM WHERE PAT_NO = :PAT_NO";
            DbCommand command = conn.CreateCommand();
            command.CommandText = sqlselect;
            command.CommandType = System.Data.CommandType.Text;

            DbParameter param = command.CreateParameter();
            param.Value = patNO;
            param.DbType = DbType.String;
            param.ParameterName = "PAT_NO";
            command.Parameters.Add(param);
            //資料庫交易
            DbTransaction trans = conn.BeginTransaction();
            try
            {
                DbDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        info.PatNO = reader["PAT_NO"].ToString();
                        info.PatID = reader["PAT_ID"].ToString();
                        info.PatName = reader["PAT_NAME"].ToString();
                        info.Sex = reader["SEX"].ToString();
                        info.Birthday = reader["BIRTHDAY"].ToString();
                    }
                }
                DataTable table = new DataTable();
                table.Load(reader);
                reader.Close();
                trans.Commit();
            }
            catch
            {
                trans.Rollback();
            }
            finally
            {
                conn.Close();
            }
            return info;
        }

        public class TPRData
        {
            public string PatNO { get; set; }
            public string MonitorDate { get; set; }
            public string MonitorTime { get; set; }
            public double Temperature { get; set; }
            public double Weight { get; set; }
            public double Pulse { get; set; }
            public double Breath { get; set; }
            public double DBP { get; set; }
            public double SBP { get; set; }
            public double SPO2 { get; set; }
        }

        [WebMethod]
        public bool SaveTPR(TPRData tprData)
        {
            TPECHDBService tpech = TPECHDBService.getInstance();
            DbConnection conn = tpech.GetTPECHConnection("B");
            conn.Open();

            string sqlinsert =
                "INSERT INTO INPTPRM (PAT_NO, MONITOR_DATE, MONITOR_TIME, TEMPERATURE, WEIGHT, PULSE, BREATH, DBP, SBP, SPO2, OP_DATE) " +
                "VALUES (:PAT_NO, :MONITOR_DATE, :MONITOR_TIME, :TEMPERATURE, :WEIGHT, :PULSE, :BREATH, :DBP, :SBP, :SPO2, sysdate)";
            DbCommand command = conn.CreateCommand();
            command.CommandText = sqlinsert;
            command.CommandType = System.Data.CommandType.Text;

            DbTransaction trans = conn.BeginTransaction();
            try
            {
                DbParameter param_no = command.CreateParameter();
                param_no.Value = tprData.PatNO;
                param_no.DbType = DbType.String;
                param_no.ParameterName = "PAT_NO";
                command.Parameters.Add(param_no);

                DbParameter param_date = command.CreateParameter();
                param_date.Value = tprData.MonitorDate;
                param_date.DbType = DbType.String;
                param_date.ParameterName = "MONITOR_DATE";
                command.Parameters.Add(param_date);

                DbParameter param_time = command.CreateParameter();
                param_time.Value = tprData.MonitorTime;
                param_time.DbType = DbType.String;
                param_time.ParameterName = "MONITOR_TIME";
                command.Parameters.Add(param_time);

                DbParameter param_temp = command.CreateParameter();
                param_temp.Value = tprData.Temperature;
                param_temp.DbType = DbType.Double;
                param_temp.ParameterName = "TEMPERATURE";
                command.Parameters.Add(param_temp);

                DbParameter param_weight = command.CreateParameter();
                param_weight.Value = tprData.Weight;
                param_weight.DbType = DbType.Double;
                param_weight.ParameterName = "WEIGHT";
                command.Parameters.Add(param_weight);

                DbParameter param_pulse = command.CreateParameter();
                param_pulse.Value = tprData.Pulse;
                param_pulse.DbType = DbType.Double;
                param_pulse.ParameterName = "PULSE";
                command.Parameters.Add(param_pulse);

                DbParameter param_breath = command.CreateParameter();
                param_breath.Value = tprData.Breath;
                param_breath.DbType = DbType.Double;
                param_breath.ParameterName = "BREATH";
                command.Parameters.Add(param_breath);

                DbParameter param_dbp = command.CreateParameter();
                param_dbp.Value = tprData.DBP;
                param_dbp.DbType = DbType.Double;
                param_dbp.ParameterName = "DBP";
                command.Parameters.Add(param_dbp);

                DbParameter param_sbp = command.CreateParameter();
                param_sbp.Value = tprData.SBP;
                param_sbp.DbType = DbType.Double;
                param_sbp.ParameterName = "SBP";
                command.Parameters.Add(param_sbp);

                DbParameter param_spo2 = command.CreateParameter();
                param_spo2.Value = tprData.SPO2;
                param_spo2.DbType = DbType.Double;
                param_spo2.ParameterName = "SPO2";
                command.Parameters.Add(param_spo2);

                return command.ExecuteNonQuery() > 0;
            }
            catch
            {
                trans.Rollback();
                return false;
            }
            finally
            {
                trans.Commit();
                conn.Close();
            }
        }

        [WebMethod]
        public List<TPRData> GetTPRs(string patNO, string begDate, string endDate)
        {
            TPECHDBService tpech = TPECHDBService.getInstance();
            DbConnection conn = tpech.GetTPECHConnection("B");
            conn.Open();

            string sqlselect = "SELECT * FROM INPTPRM WHERE PAT_NO = :PAT_NO AND MONITOR_DATE BETWEEN :begDate AND :endDate ORDER BY MONITOR_DATE ASC, MONITOR_TIME ASC";
            DbCommand command = conn.CreateCommand();
            command.CommandText = sqlselect;
            command.CommandType = System.Data.CommandType.Text;

            DbParameter param_no = command.CreateParameter();
            param_no.Value = patNO;
            param_no.DbType = DbType.String;
            param_no.ParameterName = "PAT_NO";
            command.Parameters.Add(param_no);

            DbParameter param_beg = command.CreateParameter();
            param_beg.Value = begDate;
            param_beg.DbType = DbType.String;
            param_beg.ParameterName = "begDate";
            command.Parameters.Add(param_beg);

            DbParameter param_end = command.CreateParameter();
            param_end.Value = endDate;
            param_end.DbType = DbType.String;
            param_end.ParameterName = "endDate";
            command.Parameters.Add(param_end);

            DbTransaction trans = conn.BeginTransaction();

            //use list to put tprdata
            List<TPRData> json = new List<TPRData>();
            json.Clear();

            try
            {
                DbDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        TPRData data = new TPRData();
                        data.PatNO = reader["PAT_NO"].ToString();
                        data.MonitorDate = reader["MONITOR_DATE"].ToString();
                        data.MonitorTime = reader["MONITOR_TIME"].ToString();
                        data.Temperature = Math.Round(Convert.ToDouble(reader["TEMPERATURE"]), 2);
                        data.Weight = Convert.ToDouble(reader["WEIGHT"]);
                        data.Pulse = Convert.ToDouble(reader["PULSE"]);
                        data.Breath = Convert.ToDouble(reader["BREATH"]);
                        data.DBP = Convert.ToDouble(reader["DBP"]);
                        data.SBP = Convert.ToDouble(reader["SBP"]);
                        data.SPO2 = Convert.ToDouble(reader["SPO2"]);
                        json.Add(data);
                    }
                }
                reader.Close();
            }
            finally
            {
                conn.Close();
            }
            return json;
        }

        [WebMethod]
        public TPRData GetTPRData(string patNO, string date, string time)
        {
            TPRData tpr = new TPRData();

            //connect to database
            TPECHDBService tpech = TPECHDBService.getInstance();
            DbConnection conn = tpech.GetTPECHConnection("B");
            conn.Open();

            //selectSQL
            string sqlselect = "SELECT * FROM INPTPRM WHERE PAT_NO = :PAT_NO AND MONITOR_DATE = :MONITOR_DATE AND MONITOR_TIME = :MONITOR_TIME";
            DbCommand command = conn.CreateCommand();
            command.CommandText = sqlselect;
            command.CommandType = System.Data.CommandType.Text;

            DbParameter param_no = command.CreateParameter();
            param_no.Value = patNO;
            param_no.DbType = DbType.String;
            param_no.ParameterName = "PAT_NO";
            command.Parameters.Add(param_no);

            DbParameter param_date = command.CreateParameter();
            param_date.Value = date;
            param_date.DbType = DbType.String;
            param_date.ParameterName = "MONITOR_DATE";
            command.Parameters.Add(param_date);

            DbParameter param_time = command.CreateParameter();
            param_time.Value = time;
            param_time.DbType = DbType.String;
            param_time.ParameterName = "MONITOR_TIME";
            command.Parameters.Add(param_time);

            //資料庫交易
            DbTransaction trans = conn.BeginTransaction();
            try
            {
                DbDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        tpr.Temperature = Math.Round(Convert.ToDouble(reader["TEMPERATURE"]), 2);
                        tpr.Weight = Convert.ToDouble(reader["WEIGHT"]);
                        tpr.Pulse = Convert.ToDouble(reader["PULSE"]);
                        tpr.Breath = Convert.ToDouble(reader["BREATH"]);
                        tpr.DBP = Convert.ToDouble(reader["DBP"]);
                        tpr.SBP = Convert.ToDouble(reader["SBP"]);
                        tpr.SPO2 = Convert.ToDouble(reader["SPO2"]);
                    }
                }
                DataTable table = new DataTable();
                table.Load(reader);
                reader.Close();
                trans.Commit();
            }
            catch
            {
                trans.Rollback();
            }
            finally
            {
                conn.Close();
            }
            return tpr;
        }

        [WebMethod]
        public bool UpdateTPR(TPRData tprData)
        {
            TPECHDBService tpech = TPECHDBService.getInstance();
            DbConnection conn = tpech.GetTPECHConnection("B");
            conn.Open();

            string sqlinsert = "UPDATE INPTPRM SET TEMPERATURE = :TEMPERATURE, WEIGHT = :WEIGHT, PULSE = :PULSE, BREATH = :BREATH, DBP = :DBP, SBP = :SBP, SPO2 = :SPO2, OP_DATE = sysdate " +
                "WHERE PAT_NO = :PAT_NO AND MONITOR_DATE = :MONITOR_DATE AND MONITOR_TIME = :MONITOR_TIME";

            DbCommand command = conn.CreateCommand();
            command.CommandText = sqlinsert;
            command.CommandType = System.Data.CommandType.Text;

            DbTransaction trans = conn.BeginTransaction();
            try
            {
                DbParameter param_temp = command.CreateParameter();
                param_temp.Value = tprData.Temperature;
                param_temp.DbType = DbType.Double;
                param_temp.ParameterName = "TEMPERATURE";
                command.Parameters.Add(param_temp);

                DbParameter param_weight = command.CreateParameter();
                param_weight.Value = tprData.Weight;
                param_weight.DbType = DbType.Double;
                param_weight.ParameterName = "WEIGHT";
                command.Parameters.Add(param_weight);

                DbParameter param_pulse = command.CreateParameter();
                param_pulse.Value = tprData.Pulse;
                param_pulse.DbType = DbType.Double;
                param_pulse.ParameterName = "PULSE";
                command.Parameters.Add(param_pulse);

                DbParameter param_breath = command.CreateParameter();
                param_breath.Value = tprData.Breath;
                param_breath.DbType = DbType.Double;
                param_breath.ParameterName = "BREATH";
                command.Parameters.Add(param_breath);

                DbParameter param_dbp = command.CreateParameter();
                param_dbp.Value = tprData.DBP;
                param_dbp.DbType = DbType.Double;
                param_dbp.ParameterName = "DBP";
                command.Parameters.Add(param_dbp);

                DbParameter param_sbp = command.CreateParameter();
                param_sbp.Value = tprData.SBP;
                param_sbp.DbType = DbType.Double;
                param_sbp.ParameterName = "SBP";
                command.Parameters.Add(param_sbp);

                DbParameter param_spo2 = command.CreateParameter();
                param_spo2.Value = tprData.SPO2;
                param_spo2.DbType = DbType.Double;
                param_spo2.ParameterName = "SPO2";
                command.Parameters.Add(param_spo2);

                DbParameter param_no = command.CreateParameter();
                param_no.Value = tprData.PatNO;
                param_no.DbType = DbType.String;
                param_no.ParameterName = "PAT_NO";
                command.Parameters.Add(param_no);

                DbParameter param_date = command.CreateParameter();
                param_date.Value = tprData.MonitorDate;
                param_date.DbType = DbType.String;
                param_date.ParameterName = "MONITOR_DATE";
                command.Parameters.Add(param_date);

                DbParameter param_time = command.CreateParameter();
                param_time.Value = tprData.MonitorTime;
                param_time.DbType = DbType.String;
                param_time.ParameterName = "MONITOR_TIME";
                command.Parameters.Add(param_time);

                return command.ExecuteNonQuery() > 0;
            }
            catch
            {
                trans.Rollback();
                return false;
            }
            finally
            {
                trans.Commit();
                conn.Close();
            }
        }

        [WebMethod]
        public bool DeleteTPR(TPRData tprData)
        {
            TPECHDBService tpech = TPECHDBService.getInstance();
            DbConnection conn = tpech.GetTPECHConnection("B");
            conn.Open();

            string sqlinsert = "DELETE FROM INPTPRM WHERE PAT_NO = :PAT_NO AND MONITOR_DATE = :MONITOR_DATE AND MONITOR_TIME = :MONITOR_TIME";

            DbCommand command = conn.CreateCommand();
            command.CommandText = sqlinsert;
            command.CommandType = System.Data.CommandType.Text;

            DbTransaction trans = conn.BeginTransaction();
            try
            {
                DbParameter param_no = command.CreateParameter();
                param_no.Value = tprData.PatNO;
                param_no.DbType = DbType.String;
                param_no.ParameterName = "PAT_NO";
                command.Parameters.Add(param_no);

                DbParameter param_date = command.CreateParameter();
                param_date.Value = tprData.MonitorDate;
                param_date.DbType = DbType.String;
                param_date.ParameterName = "MONITOR_DATE";
                command.Parameters.Add(param_date);

                DbParameter param_time = command.CreateParameter();
                param_time.Value = tprData.MonitorTime;
                param_time.DbType = DbType.String;
                param_time.ParameterName = "MONITOR_TIME";
                command.Parameters.Add(param_time);

                return command.ExecuteNonQuery() > 0;
            }
            catch
            {
                trans.Rollback();
                return false;
            }
            finally
            {
                trans.Commit();
                conn.Close();
            }
        }
    }
}