using CLRIQTR_EMP.Models;
using Google.Protobuf.WellKnownTypes;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using Dapper;

namespace CLRIQTR_EMP.Data.Repositories.Implementations
{
    public class EmployeeRepository
    {
        private readonly string _connStr = ConfigurationManager.ConnectionStrings["MySqlConn"].ConnectionString;

        public EmpMast GetEmployeeByEmpNo(string empNo)
        {
            if (string.IsNullOrEmpty(empNo))
            {
                Debug.WriteLine("EmployeeRepository: empNo is null or empty");
                return null;
            }

            Debug.WriteLine($"EmployeeRepository: Searching for empNo: {empNo}");

            const string sql = @"
                SELECT e.*, d.desdesc, l.labname
                FROM empmast e
                LEFT JOIN desmast d ON e.designation = d.desid
                LEFT JOIN labmast l ON e.labcode = l.labcode
                WHERE e.empno = @empno";

            try
            {
                using (var conn = new MySqlConnection(_connStr))
                {
                    conn.Open();
                    Debug.WriteLine("EmployeeRepository: Database connection opened successfully");

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@empno", empNo);

                        using (var reader = cmd.ExecuteReader())
                        {
                            Debug.WriteLine($"EmployeeRepository: Query executed, has rows: {reader.HasRows}");

                            if (reader.Read())
                            {
                                Debug.WriteLine("EmployeeRepository: Reader found data, mapping employee");
                                var employee = MapEmployeeFromReader(reader);
                                return employee;
                            }
                            else
                            {
                                Debug.WriteLine("EmployeeRepository: No data found for the given empNo");
                                return null;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EmployeeRepository Error: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return null;
            }
        }

        private EmpMast MapEmployeeFromReader(MySqlDataReader reader)
        {
            return new EmpMast
            {
                EmpNo = reader["empno"].ToString(),
                EmpName = reader["empname"] as string,
                LabCode = Convert.ToInt32(reader["labcode"]),
                LabName = reader["labname"] as string,
                Gender = reader["gender"] as string,
                PayLvl = reader["paylvl"] as string,
                Designation = reader["designation"] as string,
                DesDesc = reader["desdesc"] as string,
                DOB = reader["dob"] as string,
                DOJ = reader["doj"] as string,
                BasicPay = reader["basicpay"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["basicpay"]),
                Category = reader["category"] as string,
                DOP = reader["dop"] as string,
                DOR = reader["dor"] as string,
                Email = reader["email"] as string,
                EmpGroup = reader["empgroup"] as string,
                Grade = reader["grade"] as string,
                Active = reader["active"] as string,
                Phy = reader["phy"] as string,
                Checked = reader["checked"] as string,
                ChkDtte = reader["chkdtte"] as string,
                MobileNumber = reader["mobilenumber"] as string,
                DOB_dt = reader["dob_dt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["dob_dt"]),
                DOJ_dt = reader["doj_dt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["doj_dt"]),
                DOP_dt = reader["dop_dt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["dop_dt"]),
                DOR_dt = reader["dor_dt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["dor_dt"]),
                // CatNew = reader["catnew"] as string,
            };
        }

        public string GetQuarterTypeByEmpNo(string empNo)
        {
            const string sql = @"
        SELECT t.qtrtype 
        FROM typeligibility t 
        INNER JOIN empmast e ON t.paylvl = e.paylvl 
        WHERE e.empno = @empNo 
        LIMIT 1";

            try
            {
                using (var conn = new MySqlConnection(_connStr))
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@empNo", empNo);
                    conn.Open();

                    var result = cmd.ExecuteScalar();
                    return result != null ? result.ToString() : "Not Available";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetQuarterTypeByEmpNo Error: {ex.Message}");
                return "Error fetching data";
            }
        }


        private string GetQtrResValue(string empNo, MySqlConnection conn)
        {
            var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM qtrupd WHERE empno = @empno AND qtrstatus = 'O'", conn);
            checkCmd.Parameters.AddWithValue("@empno", empNo);

            long count = (long)checkCmd.ExecuteScalar();

            return count > 0 ? "IQ" : "OQ";
        }


        public bool InsertEqtrApply(EqtrApply entity)
        {
            try
            {
                using (var conn = new MySqlConnection(_connStr))
                {
                    conn.Open();


                    string qtrresValue = GetQtrResValue(entity.EmpNo, conn);

                   

                    var cmd = new MySqlCommand(@"
         INSERT INTO eqtrapply
         (qtrappno, empno, ownhouse, ownname, ownadd, isrent, ownrent, ishouseeightkm, neworcor,
          cpaccom, lowertypesel, saint, doa, toe, qtrres, empmobno, appstatus, permtemp,
          surname, surpost, surdesig, labcode, eqtrtypesel, ess, cco, disdesc,SpouseWorking, SpouseOffice)
         VALUES
         (@qtrappno, @empno, @ownhouse, @ownname, @ownadd, @isrent, @ownrent, @ishouseeightkm, @neworcor,
          @cpaccom, @lowertypesel, @saint, @doa, @toe, @qtrres, @empmobno, @appstatus, @permtemp,
          @surname, @surpost, @surdesig, @labcode, @eqtrtypesel, @ess, @cco, @disdesc,@SpouseWorking, @SpouseOffice)", conn);

                    // Add parameters with null-to-NA or default handling
                    cmd.Parameters.AddWithValue("@qtrappno", entity.QtrAppNo);
                    cmd.Parameters.AddWithValue("@empno", entity.EmpNo ?? "NA");
                    cmd.Parameters.AddWithValue("@ownhouse", entity.OwnHouse ?? "NA");
                    cmd.Parameters.AddWithValue("@ownname", entity.OwnName ?? "NA");
                    cmd.Parameters.AddWithValue("@ownadd", entity.OwnAdd ?? "NA");
                    cmd.Parameters.AddWithValue("@isrent", entity.IsRent ?? "NA");
                    cmd.Parameters.AddWithValue("@ownrent", entity.OwnRent ?? "0");
                    cmd.Parameters.AddWithValue("@ishouseeightkm", entity.IsHouseEightKm ?? "NA");
                    cmd.Parameters.AddWithValue("@neworcor", entity.NewOrCor ?? "NA");
                    cmd.Parameters.AddWithValue("@cpaccom", entity.CpAccom ?? "NA");
                    cmd.Parameters.AddWithValue("@lowertypesel", entity.LowerTypeSel ?? "NA");
                    cmd.Parameters.AddWithValue("@saint", entity.Saint ?? "NA");
                    cmd.Parameters.AddWithValue("@doa", entity.Doa ?? DateTime.Now.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@toe", entity.Toe ?? "NA");
                    cmd.Parameters.AddWithValue("@qtrres", qtrresValue ?? "NA");
                    cmd.Parameters.AddWithValue("@empmobno", entity.EmpMobNo ?? "NA");
                    cmd.Parameters.AddWithValue("@appstatus", entity.AppStatus ?? "D");
                    cmd.Parameters.AddWithValue("@permtemp", entity.PermTemp ?? "NA");
                    cmd.Parameters.AddWithValue("@surname", entity.Surname ?? "NA");
                    cmd.Parameters.AddWithValue("@surpost", entity.SurPost ?? "NA");
                    cmd.Parameters.AddWithValue("@surdesig", entity.SurDesig ?? "NA");
                    cmd.Parameters.AddWithValue("@labcode", entity.LabCode ?? "NA");
                    cmd.Parameters.AddWithValue("@eqtrtypesel", entity.EqtrTypeSel ?? "NA");
                    cmd.Parameters.AddWithValue("@ess", entity.Ess ?? "NA");
                    cmd.Parameters.AddWithValue("@cco", entity.Cco ?? "NA");
                    cmd.Parameters.AddWithValue("@disdesc", entity.DisDesc ?? "NA");
                    cmd.Parameters.AddWithValue("@SpouseWorking", entity.SpouseWorking ?? "NA");
                    cmd.Parameters.AddWithValue("@SpouseOffice", entity.SpouseOffice ?? "NA");

                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("InsertEqtrApply error: " + ex.Message);
                return false;
            }
        }


        public bool InsertSaEqtrApply(SaEqtrApply entity)
        {
            try
            {
                using (var conn = new MySqlConnection(_connStr))
                {
                    conn.Open();

                    string qtrresValue = GetQtrResValue(entity.EmpNo, conn);

                   

                    var cmd = new MySqlCommand(@"
                INSERT INTO saeqtrapply
                (saqtrappno, empno, ownhouse, ownname, ownadd, isrent, ownrent, ishouseeightkm, neworcor,
                 cpaccom, lowertypesel, saint, doa, toe, qtrres, empmobno, appstatus, permtemp,
                 surname, surpost, surdesig, labcode,SpouseWorking, SpouseOffice)
                VALUES
                (@saqtrappno, @empno, @ownhouse, @ownname, @ownadd, @isrent, @ownrent, @ishouseeightkm, @neworcor,
                 @cpaccom, @lowertypesel, @saint, @doa, @toe, @qtrres, @empmobno, @appstatus, @permtemp,
                 @surname, @surpost, @surdesig, @labcode,@SpouseWorking, @SpouseOffice)", conn);

                    // Add parameters with default/null-safe values
                    cmd.Parameters.AddWithValue("@saqtrappno", entity.SaQtrAppNo);
                    cmd.Parameters.AddWithValue("@empno", entity.EmpNo ?? "NA");
                    cmd.Parameters.AddWithValue("@ownhouse", entity.OwnHouse ?? "No");
                    cmd.Parameters.AddWithValue("@ownname", entity.OwnName ?? "NA");
                    cmd.Parameters.AddWithValue("@ownadd", entity.OwnAdd ?? "NA");
                    cmd.Parameters.AddWithValue("@isrent", entity.IsRent ?? "No");
                    cmd.Parameters.AddWithValue("@ownrent", entity.OwnRent ?? "0");
                    cmd.Parameters.AddWithValue("@ishouseeightkm", entity.IsHouseEightKm ?? "No");
                    cmd.Parameters.AddWithValue("@neworcor", entity.NewOrCor ?? "NA");
                    cmd.Parameters.AddWithValue("@cpaccom", entity.CpAccom ?? "NA");
                    cmd.Parameters.AddWithValue("@lowertypesel", entity.LowerTypeSel ?? "Not Interested");
                    cmd.Parameters.AddWithValue("@saint", entity.Saint ?? "NA");
                    cmd.Parameters.AddWithValue("@doa", entity.Doa ?? DateTime.Now.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@toe", entity.Toe ?? "NA");
                    cmd.Parameters.AddWithValue("@qtrres", qtrresValue ?? "NA");
                    cmd.Parameters.AddWithValue("@empmobno", entity.EmpMobNo ?? "NA");
                    cmd.Parameters.AddWithValue("@appstatus", entity.AppStatus ?? "D");
                    cmd.Parameters.AddWithValue("@permtemp", entity.PermTemp ?? "NA");
                    cmd.Parameters.AddWithValue("@surname", entity.Surname ?? "NA");
                    cmd.Parameters.AddWithValue("@surpost", entity.SurPost ?? "NA");
                    cmd.Parameters.AddWithValue("@surdesig", entity.SurDesig ?? "NA");
                    cmd.Parameters.AddWithValue("@labcode", entity.LabCode ?? "NA");
                    cmd.Parameters.AddWithValue("@SpouseWorking", entity.SpouseWorking ?? "NA");
                    cmd.Parameters.AddWithValue("@SpouseOffice", entity.SpouseOffice ?? "NA");

                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("InsertSaEqtrApply error: " + ex.Message);
                return false;
            }
        }

        public bool UpdateEqtrApply(EqtrApply entity)
        {
            try
            {
                using (var conn = new MySqlConnection(_connStr))
                {
                    conn.Open();

                    string qtrresValue = GetQtrResValue(entity.EmpNo, conn);

                    var cmd = new MySqlCommand(@"
                UPDATE eqtrapply SET
                    ownhouse = @ownhouse,
                    ownname = @ownname,
                    ownadd = @ownadd,
                    isrent = @isrent,
                    ownrent = @ownrent,
                    ishouseeightkm = @ishouseeightkm,
                    neworcor = @neworcor,
                    cpaccom = @cpaccom,
                    lowertypesel = @lowertypesel,
                    saint = @saint,
                    doa = @doa,
                    toe = @toe,
                    qtrres = @qtrres,
                    empmobno = @empmobno,
                    appstatus = @appstatus,
                    permtemp = @permtemp,
                    surname = @surname,
                    surpost = @surpost,
                    surdesig = @surdesig,
                    labcode = @labcode,
                    eqtrtypesel = @eqtrtypesel,
                    ess = @ess,
                    cco = @cco,
                    disdesc = @disdesc,
SpouseWorking = @SpouseWorking,
                    SpouseOffice = @SpouseOffice    
                WHERE qtrappno = @qtrappno AND empno = @empno", conn);

                    // Assign parameters with same null-safe/default values as in Insert
                    cmd.Parameters.AddWithValue("@ownhouse", entity.OwnHouse ?? "NA");
                    cmd.Parameters.AddWithValue("@ownname", entity.OwnName ?? "NA");
                    cmd.Parameters.AddWithValue("@ownadd", entity.OwnAdd ?? "NA");
                    cmd.Parameters.AddWithValue("@isrent", entity.IsRent ?? "NA");
                    cmd.Parameters.AddWithValue("@ownrent", entity.OwnRent ?? "0");
                    cmd.Parameters.AddWithValue("@ishouseeightkm", entity.IsHouseEightKm ?? "NA");
                    cmd.Parameters.AddWithValue("@neworcor", entity.NewOrCor ?? "NA");
                    cmd.Parameters.AddWithValue("@cpaccom", entity.CpAccom ?? "NA");
                    cmd.Parameters.AddWithValue("@lowertypesel", entity.LowerTypeSel ?? "NA");
                    cmd.Parameters.AddWithValue("@saint", entity.Saint ?? "NA");
                    cmd.Parameters.AddWithValue("@doa", entity.Doa ?? DateTime.Now.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@toe", entity.Toe ?? "NA");
                    cmd.Parameters.AddWithValue("@qtrres", qtrresValue ?? "NA");
                    cmd.Parameters.AddWithValue("@empmobno", entity.EmpMobNo ?? "NA");
                    cmd.Parameters.AddWithValue("@appstatus", entity.AppStatus ?? "Pending");
                    cmd.Parameters.AddWithValue("@permtemp", entity.PermTemp ?? "NA");
                    cmd.Parameters.AddWithValue("@surname", entity.Surname ?? "NA");
                    cmd.Parameters.AddWithValue("@surpost", entity.SurPost ?? "NA");
                    cmd.Parameters.AddWithValue("@surdesig", entity.SurDesig ?? "NA");
                    cmd.Parameters.AddWithValue("@labcode", entity.LabCode ?? "NA");
                    cmd.Parameters.AddWithValue("@eqtrtypesel", entity.EqtrTypeSel ?? "NA");
                    cmd.Parameters.AddWithValue("@ess", entity.Ess ?? "NA");
                    cmd.Parameters.AddWithValue("@cco", entity.Cco ?? "NA");
                    cmd.Parameters.AddWithValue("@disdesc", entity.DisDesc ?? "NA");
                    cmd.Parameters.AddWithValue("@SpouseWorking", entity.SpouseWorking ?? "NA");
                    cmd.Parameters.AddWithValue("@SpouseOffice", entity.SpouseOffice ?? "NA");

                    cmd.Parameters.AddWithValue("@qtrappno", entity.QtrAppNo);
                    cmd.Parameters.AddWithValue("@empno", entity.EmpNo);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UpdateEqtrApply error: " + ex.Message);
                return false;
            }
        }

        public bool UpdateSaEqtrApply(SaEqtrApply entity)
        {
            try
            {
                using (var conn = new MySqlConnection(_connStr))
                {
                    conn.Open();

                    string qtrresValue = GetQtrResValue(entity.EmpNo, conn);

                    var cmd = new MySqlCommand(@"
    UPDATE saeqtrapply SET
        ownhouse = @ownhouse,
        ownname = @ownname,
        ownadd = @ownadd,
        isrent = @isrent,
        ownrent = @ownrent,
        ishouseeightkm = @ishouseeightkm,
        neworcor = @neworcor,
        cpaccom = @cpaccom,
        lowertypesel = @lowertypesel,
        saint = @saint,
        doa = @doa,
        toe = @toe,
        qtrres = @qtrres,
        empmobno = @empmobno,
        appstatus = @appstatus,
        permtemp = @permtemp,
        surname = @surname,
        surpost = @surpost,
        surdesig = @surdesig,
        labcode = @labcode,
SpouseWorking = @SpouseWorking, 
                    SpouseOffice = @SpouseOffice   
    WHERE empno = @empno", conn);

                    // Assign parameters with null‑safe or default handling
                    cmd.Parameters.AddWithValue("@ownhouse", entity.OwnHouse ?? "NA");
                    cmd.Parameters.AddWithValue("@ownname", entity.OwnName ?? "NA");
                    cmd.Parameters.AddWithValue("@ownadd", entity.OwnAdd ?? "NA");
                    cmd.Parameters.AddWithValue("@isrent", entity.IsRent ?? "NA");
                    cmd.Parameters.AddWithValue("@ownrent", entity.OwnRent ?? "0");
                    cmd.Parameters.AddWithValue("@ishouseeightkm", entity.IsHouseEightKm ?? "NA");
                    cmd.Parameters.AddWithValue("@neworcor", entity.NewOrCor ?? "NA");
                    cmd.Parameters.AddWithValue("@cpaccom", entity.CpAccom ?? "NA");
                    cmd.Parameters.AddWithValue("@lowertypesel", entity.LowerTypeSel ?? "NA");
                    cmd.Parameters.AddWithValue("@saint", entity.Saint ?? "NA");
                    cmd.Parameters.AddWithValue("@doa", entity.Doa ?? DateTime.Now.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@toe", entity.Toe ?? "NA");
                    cmd.Parameters.AddWithValue("@qtrres", qtrresValue ?? "NA");
                    cmd.Parameters.AddWithValue("@empmobno", entity.EmpMobNo ?? "NA");
                    cmd.Parameters.AddWithValue("@appstatus", entity.AppStatus ?? "Pending");
                    cmd.Parameters.AddWithValue("@permtemp", entity.PermTemp ?? "NA");
                    cmd.Parameters.AddWithValue("@surname", entity.Surname ?? "NA");
                    cmd.Parameters.AddWithValue("@surpost", entity.SurPost ?? "NA");
                    cmd.Parameters.AddWithValue("@surdesig", entity.SurDesig ?? "NA");
                    cmd.Parameters.AddWithValue("@labcode", entity.LabCode ?? "NA");
                    cmd.Parameters.AddWithValue("@SpouseWorking", entity.SpouseWorking ?? "NA");
                    cmd.Parameters.AddWithValue("@SpouseOffice", entity.SpouseOffice ?? "NA");
                    cmd.Parameters.AddWithValue("@saqtrappno", entity.SaQtrAppNo);
                    cmd.Parameters.AddWithValue("@empno", entity.EmpNo);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UpdateSaEqtrApply error: " + ex.Message);
                return false;
            }
        }






        // Method to map a row from the eqtrapply table to the EqtrApply model
        private EqtrApply MapEqtrApplyFromReader(MySqlDataReader reader)
        {
            return new EqtrApply
            {
                QtrAppNo = reader["qtrappno"].ToString(),
                EmpNo = reader["empno"].ToString(),
                OwnHouse = reader["ownhouse"] as string,
                OwnName = reader["ownname"] as string,
                OwnAdd = reader["ownadd"] as string,
                IsRent = reader["isrent"] as string,
                OwnRent = reader["ownrent"] as string,
                IsHouseEightKm = reader["ishouseeightkm"] as string,
                NewOrCor = reader["neworcor"] as string,
                CpAccom = reader["cpaccom"] as string,
                LowerTypeSel = reader["lowertypesel"] as string,
                Saint = reader["saint"] as string,
                Doa = reader["doa"] as string,
                Toe = reader["toe"] as string,
                QtrRes = reader["qtrres"] as string,
                EmpMobNo = reader["empmobno"] as string,
                AppStatus = reader["appstatus"] as string,
                PermTemp = reader["permtemp"] as string,
                Surname = reader["surname"] as string,
                SurPost = reader["surpost"] as string,
                SurDesig = reader["surdesig"] as string,
                LabCode = reader["labcode"] as string,
                EqtrTypeSel = reader["eqtrtypesel"] as string,
                Ess = reader["ess"] as string,
                Cco = reader["cco"] as string,
                DisDesc = reader["disdesc"] as string,
                SpouseWorking = reader["SpouseWorking"] as string,
                SpouseOffice = reader["SpouseOffice"] as string
            };
        }

        public EqtrApply GetDraftByAppNo(string qtrAppNo)
        {

            Debug.WriteLine("GetDraftByAppNo");

            Debug.WriteLine(qtrAppNo);


            if (string.IsNullOrEmpty(qtrAppNo))
                return null;

const string sql = @"SELECT qtrappno, appstatus, empno, ownhouse, ownname, ownadd, isrent, ownrent, ishouseeightkm, neworcor, cpaccom, lowertypesel, saint, doa, toe, qtrres, empmobno, permtemp, surname, surpost, surdesig, labcode, eqtrtypesel, ess, cco, disdesc,SpouseWorking,SpouseOffice FROM eqtrapply WHERE qtrAppNo = @qtrAppNo AND (appstatus = 'D' OR appstatus = 'C') UNION SELECT saqtrappno AS qtrappno, appstatus, empno, ownhouse, ownname, ownadd, isrent, ownrent, ishouseeightkm, neworcor, cpaccom, lowertypesel, saint, doa, toe, qtrres, empmobno, permtemp, surname, surpost, surdesig, labcode, NULL AS eqtrtypesel, NULL AS ess, NULL AS cco, NULL AS disdesc,SpouseWorking,SpouseOffice FROM saeqtrapply WHERE saqtrAppNo = @qtrAppNo AND (appstatus = 'D' OR appstatus = 'C')";

            try
            {
                using (var conn = new MySqlConnection(_connStr))
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@qtrAppNo", qtrAppNo);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                            return MapEqtrApplyFromReader(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetDraftByAppNo Error: {ex.Message}");
            }

            return null;
        }

        //public List<EqtrApply> GetDraftsByEmpNo(string empNo)
        //{
        //    Debug.WriteLine("GetDraftsByEmpNo");
        //    Debug.WriteLine(empNo);

        //    var drafts = new List<EqtrApply>();
        //    if (string.IsNullOrEmpty(empNo))
        //        return drafts;

        //    var eqtrDrafts = new List<EqtrApply>();
        //    var saeqtrDrafts = new List<EqtrApply>();

        //    const string eqtrSql = @"
        //SELECT qtrappno, appstatus, empno, ownhouse, ownname, ownadd, isrent, ownrent, ishouseeightkm,
        //       neworcor, cpaccom, lowertypesel, saint, doa, toe, qtrres, empmobno, permtemp, surname,
        //       surpost, surdesig, labcode, eqtrtypesel, ess, cco, disdesc
        //FROM eqtrapply
        //WHERE empno = @empNo AND (appstatus = 'D' OR appstatus = 'C')";

        //    const string saSql = @"
        //SELECT saqtrappno AS qtrappno, appstatus, empno, ownhouse, ownname, ownadd, isrent, ownrent, ishouseeightkm,
        //       neworcor, cpaccom, lowertypesel, saint, doa, toe, qtrres, empmobno, permtemp, surname,
        //       surpost, surdesig, labcode, NULL AS eqtrtypesel, NULL AS ess, NULL AS cco, NULL AS disdesc
        //FROM saeqtrapply
        //WHERE empno = @empNo AND (appstatus = 'D' OR appstatus = 'C')";

        //    try
        //    {
        //        using (var conn = new MySqlConnection(_connStr))
        //        {
        //            conn.Open();

        //            // Get EQTR drafts
        //            using (var cmd = new MySqlCommand(eqtrSql, conn))
        //            {
        //                cmd.Parameters.AddWithValue("@empNo", empNo);
        //                using (var reader = cmd.ExecuteReader())
        //                {
        //                    while (reader.Read())
        //                    {
        //                        eqtrDrafts.Add(MapEqtrApplyFromReader(reader));
        //                    }
        //                }
        //            }

        //            // Get SAQTR drafts
        //            using (var cmd = new MySqlCommand(saSql, conn))
        //            {
        //                cmd.Parameters.AddWithValue("@empNo", empNo);
        //                using (var reader = cmd.ExecuteReader())
        //                {
        //                    while (reader.Read())
        //                    {
        //                        saeqtrDrafts.Add(MapEqtrApplyFromReader(reader));
        //                    }
        //                }
        //            }
        //        }

        //        // Add all EQTR drafts first
        //        drafts.AddRange(eqtrDrafts);

        //        // Prepare HashSet of existing app nos in EQTR to avoid duplicates
        //        var eqtrAppNos = new HashSet<string>(eqtrDrafts.Select(d => d.QtrAppNo));

        //        // Add SAQTR drafts only if their QtrAppNo doesn't exist in EQTR drafts
        //        foreach (var saDraft in saeqtrDrafts)
        //        {
        //            if (!eqtrAppNos.Contains(saDraft.QtrAppNo))
        //            {
        //                drafts.Add(saDraft);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"GetDraftsByEmpNo Error: {ex.Message}");
        //    }

        //    return drafts;
        //}

        public List<EqtrApply> GetDraftsByEmpNo(string empNo)
        {
            Debug.WriteLine("GetDraftsByEmpNo: " + empNo);
            var drafts = new List<EqtrApply>();

            if (string.IsNullOrEmpty(empNo))
                return drafts;

            const string sql = @"
        -- Get records that are in eq and have a match in sa
SELECT 
    CONCAT(IFNULL(eq.qtrappno, ''), IF(eq.qtrappno IS NOT NULL AND sa.saqtrappno IS NOT NULL, ' ', ''), IFNULL(sa.saqtrappno, '')) AS qtrappno,
    COALESCE(eq.doa, sa.doa) AS doa,
    COALESCE(eq.appstatus, sa.appstatus) AS appstatus,e.doj_dt,
            e.designation
FROM 
    eqtrapply eq
LEFT JOIN saeqtrapply sa ON eq.empno = sa.empno
LEFT JOIN empmast e ON e.empno = eq.empno
WHERE 
    eq.empno = @empNo
    AND (eq.appstatus IN ('D', 'C') OR sa.appstatus IN ('D', 'C'))

UNION

-- Get records that are in sa but have no match in eq
SELECT 
    CONCAT(IFNULL(eq.qtrappno, ''), IF(eq.qtrappno IS NOT NULL AND sa.saqtrappno IS NOT NULL, ' ', ''), IFNULL(sa.saqtrappno, '')) AS qtrappno,
    COALESCE(eq.doa, sa.doa) AS doa,
    COALESCE(eq.appstatus, sa.appstatus) AS appstatus,e.doj_dt,
            e.designation
FROM 
    eqtrapply eq
RIGHT JOIN saeqtrapply sa ON eq.empno = sa.empno
LEFT JOIN empmast e ON e.empno = eq.empno

WHERE 
    sa.empno = @empNo
    AND eq.empno IS NULL -- This is key: ensures no duplicates from the first query
    AND (eq.appstatus IN ('D', 'C') OR sa.appstatus IN ('D', 'C'));

        
    ";

            try
            {
                using (var conn = new MySqlConnection(_connStr))
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@empNo", empNo);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var draft = new EqtrApply
                                {
                                    QtrAppNo = reader["qtrappno"]?.ToString(),
                                    Doa = reader["doa"].ToString(),
                                    AppStatus = reader["appstatus"]?.ToString()
                                };

                                drafts.Add(draft);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetDraftsByEmpNo Error: " + ex.Message);
            }

            return drafts;
        }

        public bool CanApplyForNewQuarter(string empNo)
        {
            if (string.IsNullOrEmpty(empNo))
            {
                return false;
            }

           
            const string sql = @"
        SELECT
   -- GROUP_CONCAT(DISTINCT typeligibility.qtrtype) AS EligibleTypes,
    -- qtrupd.qtrtype AS OccupiedType,
    -- GROUP_CONCAT(DISTINCT CASE WHEN saeligibility.saeligible = 'Y' THEN 'SA' END) AS SA_Eligibility,
    CASE
        WHEN
            FIND_IN_SET(
                SUBSTRING_INDEX(qtrupd.qtrtype, ' ', 1),
                GROUP_CONCAT(DISTINCT typeligibility.qtrtype)
            ) > 0
            AND
            GROUP_CONCAT(DISTINCT CASE WHEN saeligibility.saeligible = 'Y' THEN 'SA' END) IS NULL
        THEN 0 -- Cannot apply (eligible for the same type, no SA)
        ELSE 1 -- Can apply
    END AS eligibility_flag
FROM
    qtrupd
INNER JOIN
    empmast ON qtrupd.empno = empmast.empno
INNER JOIN
    typeligibility ON empmast.paylvl = typeligibility.paylvl
LEFT JOIN
    saeligibility ON empmast.paylvl = saeligibility.paylvl
WHERE
    qtrupd.empno = @empNo
    AND qtrupd.qtrstatus = 'O'
GROUP BY
    qtrupd.qtrtype
    ";

            try
            {
                using (var conn = new MySqlConnection(_connStr))
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@empNo", empNo);

                        var result = cmd.ExecuteScalar();

                        
                        if (result == null || result is DBNull)
                        {
                            return true;
                        }

                        
                        return Convert.ToInt32(result) == 1;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HasQuarterWithStatus Error: {ex.Message}");
                return false;
            }
        }

        //public bool HasQuarterWithStatus(string empNo, string status)
        //{
        //    if (string.IsNullOrEmpty(empNo) || string.IsNullOrEmpty(status))
        //    {
        //        Debug.WriteLine("HasQuarterWithStatus: empNo or status is null or empty");
        //        return false;
        //    }

        //    Debug.WriteLine(empNo);
        //    Debug.WriteLine(status);

        //    const string sql = @"
        //SELECT COUNT(1)
        //FROM QtrUpd
        //WHERE EmpNo = @empNo AND qtrstatus = @status";

        //    try
        //    {
        //        using (var conn = new MySqlConnection(_connStr))
        //        {
        //            conn.Open();
        //            Debug.WriteLine("HasQuarterWithStatus: Database connection opened successfully");

        //            using (var cmd = new MySqlCommand(sql, conn))
        //            {
        //                cmd.Parameters.AddWithValue("@empNo", empNo);
        //                cmd.Parameters.AddWithValue("@status", status);

        //                var result = cmd.ExecuteScalar();
        //                Debug.WriteLine($"HasQuarterWithStatus: Query executed, result: {result}");

        //                return Convert.ToInt32(result) > 0;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"HasQuarterWithStatus Error: {ex.Message}");
        //        Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
        //        return false;
        //    }
        //}

        public bool HasDraftApplication(string empNo)
        {
            if (string.IsNullOrEmpty(empNo))
            {
                Debug.WriteLine("HasDraftApplication: empNo is null or empty");
                return false;
            }

            Debug.WriteLine(empNo);

            const string sql = @"
        SELECT COUNT(1)
        FROM (
            SELECT EmpNo FROM EQtrApply WHERE EmpNo = @empNo
            UNION ALL
            SELECT EmpNo FROM SAEQtrApply WHERE EmpNo = @empNo
        ) AS Combined";

            try
            {
                using (var conn = new MySqlConnection(_connStr))
                {
                    conn.Open();
                    Debug.WriteLine("HasDraftApplication: Database connection opened successfully");

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@empNo", empNo);

                        var result = cmd.ExecuteScalar();
                        Debug.WriteLine($"HasDraftApplication: Query executed, result: {result}");

                        return Convert.ToInt32(result) > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HasDraftApplication Error: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                return false;
            }
        }

        public bool GetApplyingForQuartersFromDb(string empno)
        {
            string query = "SELECT eqtrtypesel FROM eqtrapply WHERE empno = @empno";

            using (MySqlConnection conn = new MySqlConnection(_connStr))
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@empno", empno);
                conn.Open();
                object result = cmd.ExecuteScalar();
                return result != null && result.ToString() == "Y";
            }
        }

        public bool GetApplyingForScientistQuartersFromDb(string empno)
        {
            string query = "SELECT saint FROM saeqtrapply WHERE empno = @empno";

            using (MySqlConnection conn = new MySqlConnection(_connStr))
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@empno", empno);
                conn.Open();
                object result = cmd.ExecuteScalar();
                return result != null && result.ToString() == "SI";
            }
        }




        public bool HasPhysicalDisability(string empNo)
        {
            string query = "SELECT phy FROM empmast WHERE empno = @empNo";

            using (var con = new MySqlConnection(_connStr))
            {
                using (var cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@empNo", empNo);

                    con.Open();
                    var result = cmd.ExecuteScalar();

                    if (result != null && result.ToString() == "Y")
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        public QuarterApplicationPdfDto GetQuarterApplicationDetails(string qtrAppNo)
        {
            const string sql = @"
WITH FamilyDetailsCTE AS (
    SELECT
        empno,
        GROUP_CONCAT(
            -- Using CASE to handle NULL depcodes gracefully
            CASE
                WHEN depcode IS NULL THEN depname
                ELSE CONCAT(depname, ' (', depcode, rn, ')')
            END
            SEPARATOR ', '
        ) AS AggregatedFamilyDetails
    FROM (
        SELECT
            e.empno,
            e.depname,
            m.depcode,
            ROW_NUMBER() OVER(PARTITION BY e.empno, m.depcode ORDER BY e.depname) AS rn
        FROM 
            empdep e
        LEFT JOIN 
            empdepmast m ON e.depid = m.depid
    ) AS NumberedSubquery
    GROUP BY empno
)
SELECT 
    e.eqtrtypesel, e.saint, e.lowertypesel, e.qtrres, e.qtrappno, e.doa, l.labname, 
    e.empno, em.empname, d.desdesc AS designation, em.email, em.mobilenumber, 
    em.category, em.phy AS physicallyhandicapped, e.disdesc AS disabilitydetails, 
    em.paylvl, em.paylvl AS paylevelonjan, em.dop, em.dob, em.doj, em.basicpay, 
    e.cpaccom AS accommodationdetails, 
    
    fd.AggregatedFamilyDetails AS familydetails, 
    
    t.qtrtype AS entitledtype, e.qtrres, e.ownhouse, e.ownname, 
    e.ownadd AS owneraddress, e.ishouseeightkm AS ishouseletout, 
    e.ownrent AS rentreceived, e.permtemp, e.surname AS suretyname, 
    e.surpost AS suretydesignation, e.surdesig AS suretypost, e.ess, e.cco,e.SpouseWorking, e.SpouseOffice
FROM eqtrapply e
LEFT JOIN empmast em ON e.empno = em.empno
LEFT JOIN desmast d ON em.designation = d.desid
LEFT JOIN labmast l ON em.labcode = l.labcode
LEFT JOIN typeligibility t ON em.paylvl = t.paylvl
LEFT JOIN FamilyDetailsCTE fd ON e.empno = fd.empno
WHERE e.qtrappno = @qtrAppNo";

            using (var conn = new MySqlConnection(_connStr))
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@qtrAppNo", qtrAppNo);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new QuarterApplicationPdfDto
                        {
                            qtres = GetString(reader, "qtrres"),
                            eqtrtypesel = GetString(reader, "eqtrtypesel"),
                            saint = GetString(reader, "saint"),
                            lowertypesel = GetString(reader, "lowertypesel"),
                            QtrAppNo = GetString(reader, "qtrappno"),
                            Doa = GetString(reader, "doa"),
                            LabName = GetString(reader, "labname"),
                            EmpNo = GetString(reader, "empno"),
                            EmpName = GetString(reader, "empname"),
                            Designation = GetString(reader, "designation"),
                            Email = GetString(reader, "email"),
                            MobileNumber = GetString(reader, "mobilenumber"),
                            Category = GetString(reader, "category"),
                            PhysicallyHandicapped = GetString(reader, "physicallyhandicapped"),
                            DisabilityDetails = GetString(reader, "disabilitydetails"),
                            PayLevel = GetString(reader, "paylvl"),
                            DateOfPromotion = GetString(reader, "dop") ,
                            DateOfBirth = GetString(reader, "dob") ,
                            DateOfJoining = GetString(reader, "doj") ,
                            BasicPay = GetString(reader, "basicpay"),
                            AccommodationDetails = GetString(reader, "accommodationdetails"),
                            FamilyDetails = GetString(reader, "familydetails"),
                            EntitledType = GetString(reader, "entitledtype"),
                            OwnHouse = GetString(reader, "ownhouse"),
                            OwnerName = GetString(reader, "ownname"),
                            OwnerAddress = GetString(reader, "owneraddress"),
                            IsHouseLetOut = GetString(reader, "ishouseletout"),
                            RentReceived = GetString(reader, "rentreceived"),
                            PermanentOrTemporary = GetString(reader, "permtemp"),
                            SuretyName = GetString(reader, "suretyname"),
                            SuretyDesignation = GetString(reader, "suretydesignation"),
                            SuretyPost = GetString(reader, "suretypost"),
                            ServicesEssential = GetString(reader, "ess"),
                            cco=GetString(reader,"cco"),
                            SpouseWorking = GetString(reader, "SpouseWorking"),
                            SpouseOfficeName = GetString(reader, "SpouseOffice")
                        };

                    }
                }
            }

            return null;
        }




        public QuarterApplicationPdfDto GetSAQuarterApplicationDetails(string qtrAppNo)
        {
            const string sql1 = @"
WITH FamilyDetailsCTE AS (
    SELECT
        empno,
        GROUP_CONCAT(
            -- Using CASE to handle NULL depcodes gracefully
            CASE
                WHEN depcode IS NULL THEN depname
                ELSE CONCAT(depname, ' (', depcode, rn, ')')
            END
            SEPARATOR ', '
        ) AS AggregatedFamilyDetails
    FROM (
        SELECT
            e.empno,
            e.depname,
            m.depcode,
            ROW_NUMBER() OVER(PARTITION BY e.empno, m.depcode ORDER BY e.depname) AS rn
        FROM 
            empdep e
        LEFT JOIN 
            empdepmast m ON e.depid = m.depid
    ) AS NumberedSubquery
    GROUP BY empno
)
SELECT e.saint,
    e.qtrres,
    e.saqtrappno AS qtrappno, 
    e.doa, 
    l.labname, 
    e.empno, 
    em.empname, 
    d.desdesc AS designation,
    em.email, 
    em.mobilenumber, 
    em.category, 
    em.phy AS physicallyhandicapped,
    em.paylvl, 
    em.paylvl AS paylevelonjan,
    em.dop, 
    em.dob, 
    em.doj, 
    em.basicpay,
    e.cpaccom AS accommodationdetails, 
    
    fd.AggregatedFamilyDetails AS familydetails,

    t.qtrtype AS entitledtype,
    e.qtrres, 
    e.ownhouse, 
    e.ownname, 
    e.ownadd AS owneraddress,
    e.ishouseeightkm AS ishouseletout, 
    e.ownrent AS rentreceived,
    e.permtemp, 
    e.surname AS suretyname, 
    e.surpost AS suretydesignation, 
    e.surdesig AS suretypost,e.SpouseWorking, e.SpouseOffice
FROM saeqtrapply e
LEFT JOIN empmast em ON e.empno = em.empno
LEFT JOIN desmast d ON em.designation = d.desid
LEFT JOIN labmast l ON em.labcode = l.labcode
LEFT JOIN typeligibility t ON em.paylvl = t.paylvl

LEFT JOIN FamilyDetailsCTE fd ON e.empno = fd.empno

WHERE e.saqtrappno = @qtrAppNo";

            const string sql2 = @"SELECT e.eqtrtypesel, e.CCO, e.ess
    FROM eqtrapply e
    LEFT JOIN empmast em ON e.empno = em.empno
    LEFT JOIN desmast d ON em.designation = d.desid
    LEFT JOIN labmast l ON em.labcode = l.labcode
    LEFT JOIN typeligibility t ON em.paylvl = t.paylvl
    
    WHERE e.empno = @empno
    LIMIT 1";

            using (var conn = new MySqlConnection(_connStr))
            using (var cmd1 = new MySqlCommand(sql1, conn))
            {
                cmd1.Parameters.AddWithValue("@qtrAppNo", qtrAppNo);
                conn.Open();

                QuarterApplicationPdfDto dto = null;
                using (var reader1 = cmd1.ExecuteReader())
                {
                    if (reader1.Read())
                    {
                        dto = new QuarterApplicationPdfDto
                        {
                            qtres = GetString(reader1, "qtrres"),
                            saint = GetString(reader1, "saint"),
                            QtrAppNo = GetString(reader1, "qtrappno"),
                            Doa = GetString(reader1, "doa"),
                            LabName = GetString(reader1, "labname"),
                            EmpNo = GetString(reader1, "empno"),
                            EmpName = GetString(reader1, "empname"),
                            Designation = GetString(reader1, "designation"),
                            Email = GetString(reader1, "email"),
                            MobileNumber = GetString(reader1, "mobilenumber"),
                            Category = GetString(reader1, "category"),
                            PhysicallyHandicapped = GetString(reader1, "physicallyhandicapped"),
                            PayLevel = GetString(reader1, "paylvl"),
                            DateOfPromotion = GetString(reader1, "dop"),
                            DateOfBirth = GetString(reader1, "dob"),
                            DateOfJoining = GetString(reader1, "doj"),
                            BasicPay = GetString(reader1, "basicpay"),
                            AccommodationDetails = GetString(reader1, "accommodationdetails"),
                            FamilyDetails = GetString(reader1, "familydetails"),
                            EntitledType = GetString(reader1, "entitledtype"),
                            OwnHouse = GetString(reader1, "ownhouse"),
                            OwnerName = GetString(reader1, "ownname"),
                            OwnerAddress = GetString(reader1, "owneraddress"),
                            IsHouseLetOut = GetString(reader1, "ishouseletout"),
                            RentReceived = GetString(reader1, "rentreceived"),
                            PermanentOrTemporary = GetString(reader1, "permtemp"),
                            SuretyName = GetString(reader1, "suretyname"),
                            SuretyDesignation = GetString(reader1, "suretydesignation"),
                            SuretyPost = GetString(reader1, "suretypost"),
                            SpouseWorking = GetString(reader1, "SpouseWorking"),
                            SpouseOfficeName = GetString(reader1, "SpouseOffice")


                        };
                    }
                }

                if (dto != null)
                {
                    // Now get the additional data from the second query
                    using (var cmd2 = new MySqlCommand(sql2, conn))
                    {
                        cmd2.Parameters.AddWithValue("@empno", dto.EmpNo);
                        using (var reader2 = cmd2.ExecuteReader())
                        {
                            if (reader2.Read())
                            {
                                // Populate the additional fields if they exist
                                dto.eqtrtypesel = GetString(reader2, "eqtrtypesel");
                                dto.cco = GetString(reader2, "CCO");
                                dto.ServicesEssential = GetString(reader2, "ess");
                               
                                // dto.saint might already be populated, but if you want to override:
                                // dto.saint = GetString(reader2, "saint");
                            }
                        }
                    }
                }

                return dto;
            }
        }





        private string GetString(IDataRecord reader, string columnName)
        {
            var val = reader[columnName];
            return val == DBNull.Value ? string.Empty : val.ToString();
        }

        //private DateTime? GetDateTime(IDataRecord reader, string columnName)
        //{
        //    var val = reader[columnName];
        //    return val == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(val);
        //}

        private DateTime? GetDateTime(IDataRecord reader, string columnName)
        {
            var val = reader[columnName];

            // If the value is DBNull or null, return null
            if (val == DBNull.Value || val == null || string.IsNullOrEmpty(val.ToString()))
            {
                return null;
            }

            // Attempt to parse the value as a DateTime
            DateTime parsedDate;
            bool isParsed = DateTime.TryParse(val.ToString(), out parsedDate);

            if (isParsed)
            {
                return parsedDate; // Return valid parsed date
            }

            // Log the invalid date string (for debugging purposes)
            Console.WriteLine($"Invalid Date Format: {val}");

            // Return null if parsing fails
            return null;
        }


        /// <summary>
        /// Gets the lab name for an employee by joining empmast and labmast tables.
        /// </summary>
        /// <param name="empNo">The employee's number.</param>
        /// <returns>The lab name, or "CLRI" as a fallback.</returns>
        private string GetLabNameForEmployee(string empNo)
        {
            // Use "CLRI" as a default/fallback value in case of an error or no result
            string labName = "CLRI";
            try
            {
                using (var conn = new MySqlConnection(_connStr))
                {
                    conn.Open();
                    var cmd = new MySqlCommand(@"
                SELECT T2.labname
                FROM empmast T1
                JOIN labmast T2 ON T1.labcode = T2.labcode
                WHERE T1.empno = @empno", conn);
                    cmd.Parameters.AddWithValue("@empno", empNo);

                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        labName = result.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetLabNameForEmployee error: " + ex.Message);
            }
            return labName;
        }

        /// <summary>
        /// Gets the last application number for a specific lab and year.
        /// </summary>
        private string GetLastEqtrAppNo(string labName, int year)
        {
            string pattern = $"QTR/{labName}/{year}/%";
            try
            {
                using (var conn = new MySqlConnection(_connStr))
                {
                    conn.Open();
                    var cmd = new MySqlCommand(
                        "SELECT qtrappno FROM eqtrapply WHERE qtrappno LIKE @pattern ORDER BY qtrappno DESC LIMIT 1", conn);
                    cmd.Parameters.AddWithValue("@pattern", pattern);
                    return cmd.ExecuteScalar()?.ToString();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetLastEqtrAppNo error: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Gets the last SA application number for a specific lab and year.
        /// </summary>
        private string GetLastSaEqtrAppNo(string labName, int year)
        {
            string pattern = $"QTR/{labName}/{year}/SA/%";
            try
            {
                using (var conn = new MySqlConnection(_connStr))
                {
                    conn.Open();
                    var cmd = new MySqlCommand(
                        "SELECT saqtrappno FROM saeqtrapply WHERE saqtrappno LIKE @pattern ORDER BY saqtrappno DESC LIMIT 1", conn);
                    cmd.Parameters.AddWithValue("@pattern", pattern);
                    return cmd.ExecuteScalar()?.ToString();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetLastSaEqtrAppNo error: " + ex.Message);
                return null;
            }
        }

        public string GenerateNewEqtrAppNo(String empNo)
        {
            string labName = GetLabNameForEmployee(empNo);
            int year = DateTime.Now.Year;

            string lastAppNo = GetLastEqtrAppNo(labName, year);

            string prefix = $"QTR/{labName}/{year}/";

            int newNumber = 100001; // Default starting number for a new year/lab

            if (!string.IsNullOrEmpty(lastAppNo))
            {
                string lastNumberStr = lastAppNo.Split('/').Last();
                if (int.TryParse(lastNumberStr, out int lastNumber))
                {
                    newNumber = lastNumber + 1;
                }
            }

            return prefix + newNumber;
        }

        public string GenerateNewSaEqtrAppNo(String empNo)
        {
            // 1. Get the dynamic lab name for the employee
            string labName = GetLabNameForEmployee(empNo);
            int year = DateTime.Now.Year;

            // 2. Get the last SA app number for that specific lab and year
            string lastAppNo = GetLastSaEqtrAppNo(labName, year);

            // 3. Create the prefix with the dynamic lab name
            string prefix = $"QTR/{labName}/{year}/SA/";

            int newNumber = 1001; // Default starting number for a new year/lab

            if (!string.IsNullOrEmpty(lastAppNo))
            {
                string lastNumberStr = lastAppNo.Split('/').Last();
                if (int.TryParse(lastNumberStr, out int lastNumber))
                {
                    newNumber = lastNumber + 1;
                }
            }

            return prefix + newNumber;
        }

        public string GetFamilyDetailsByEmpNo(string empno)
        {
            string familyDetails = "";

           
            string sql = @"WITH NumberedDependents AS (
    SELECT
        e.empno,
        e.depname,
        m.depcode,
       
        ROW_NUMBER() OVER(PARTITION BY e.empno, m.depcode ORDER BY e.depname) AS rn
    FROM
        empdep e
    LEFT JOIN
        empdepmast m ON e.depid = m.depid
    WHERE
        e.empno = @empno
)
SELECT
    GROUP_CONCAT(
        CASE
            WHEN depcode IS NULL THEN depname 
            ELSE CONCAT(depname, ' (', depcode, rn, ')') 
        END
        SEPARATOR ', '
    ) AS FamilyDetails
FROM
    NumberedDependents
GROUP BY
    empno";

            using (var connection = new MySqlConnection(_connStr))
            {
                connection.Open();

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@empno", empno);

                    using (var reader = command.ExecuteReader())
                    {
                        
                        if (reader.Read())
                        {
                            if (reader["FamilyDetails"] != DBNull.Value)
                            {
                                familyDetails = reader["FamilyDetails"].ToString();
                            }
                        }
                    }
                }
            }

            return familyDetails;
        }

        public bool IsEntitledQuarterAlreadyOccupied(string empNo)
        {
            const string sql = @"
        SELECT COUNT(*) 
        FROM qtrupd q
        INNER JOIN empmast e ON q.empno = e.empno
        INNER JOIN typeligibility t ON e.paylvl = t.paylvl
        WHERE q.empno = @empNo
          AND q.qtrstatus = 'O'
          -- q.qtrtype is like 'III MS', entitlement is 'III', so compare the prefix
          AND SUBSTRING_INDEX(q.qtrtype, ' ', 1) = t.qtrtype";

            try
            {
                using (var conn = new MySqlConnection(_connStr))
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@empNo", empNo);
                    conn.Open();

                    var result = cmd.ExecuteScalar();
                    long count = (result == null || result is DBNull) ? 0 : Convert.ToInt64(result);
                    return count > 0;   // true = already in possession of entitled type
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("IsEntitledQuarterAlreadyOccupied error: " + ex.Message);
                return false;   // safe fallback: treat as not occupied
            }
        }

        public bool IsScientistQuarterEligible(string empNo)
        {
            const string sql = @"
        SELECT e.paylvl,
               e.designation,
               s.saeligible
        FROM empmast e
        LEFT JOIN saeligibility s ON e.paylvl = s.paylvl
        WHERE e.empno = @empNo
        LIMIT 1";

            try
            {
                using (var conn = new MySqlConnection(_connStr))
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@empNo", empNo);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            return false;

                        string paylvlRaw = reader["paylvl"]?.ToString() ?? "";
                        string desigCode = reader["designation"]?.ToString() ?? "";
                        string saFlag = reader["saeligible"]?.ToString() ?? "N";

                        // If pay-level itself is not SA-eligible, stop here
                        if (saFlag != "Y")
                            return false;

                        // Normalise pay level to integer (handles 13A, etc.)
                        var digitsOnly = new string(paylvlRaw.Where(char.IsDigit).ToArray());
                        int payLevelInt = 0;
                        int.TryParse(digitsOnly, out payLevelInt);

                        // For pay level > 7: saeligibility table alone is enough
                        if (payLevelInt > 7)
                            return true;

                        // For pay level = 7: ONLY designation 19 is allowed
                        if (payLevelInt == 7)
                            return desigCode == "19";

                        // For pay levels below 7: not eligible
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("IsScientistQuarterEligible error: " + ex.Message);
                return false; // safe default
            }
        }



        public bool IsScientistQuarterAlreadyOccupied(string empNo)
        {
            const string sql = @"
        SELECT COUNT(*)
        FROM qtrupd
        WHERE empno = @empNo
          AND qtrstatus = 'O'
          AND qtrtype = 'SA'";

            try
            {
                using (var conn = new MySqlConnection(_connStr))
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@empNo", empNo);
                    conn.Open();

                    var result = cmd.ExecuteScalar();
                    long count = (result == null || result is DBNull) ? 0 : Convert.ToInt64(result);
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("IsScientistQuarterAlreadyOccupied error: " + ex.Message);
                return false;
            }
        }



    }
}
