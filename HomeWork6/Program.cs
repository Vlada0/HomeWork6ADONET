using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace HomeWork6
{
    class Program
    {
        static string connectionString = ConfigurationManager.ConnectionStrings["sqlConnection"].ConnectionString;
        public static DataTable patientTable;
        public static SqlDataAdapter dataAdapter;

        static void Main(string[] args)
        {
            //CreatePatientTable();
            //CreatePatientDiagnosisTable();

            patientTable = GetPatientTable();
            dataAdapter = GetPatientAdapter();
            dataAdapter.Fill(patientTable);
            PrintPatientData();
            InsertRowInPatientTable(new Patient { FirstName = "Denis", LastName = "Petrov", Age = 22, Gender = "M", PhoneNumber = "067458934" });
            UpdateRowInPatientTable(new Patient { FirstName = "Ivan", LastName = "Ivanov" , Age = 23, Gender = "M"}, 1);
            DeleteRowInPatientTable(2);
            var patientsList = SelectRowsFromPatientTable();
        }

        private static void CreatePatientTable()
        {
            using(var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                var sqlCommandText = "CREATE TABLE Patient(PatientID INT NOT NULL IDENTITY(1,1) PRIMARY KEY, " +
                    "FirstName NVARCHAR(25) NOT NULL," +
                    "LastName NVARCHAR(25) NOT NULL," +
                    "Age INT NOT NULL," +
                    "Gender VARCHAR(1) NOT NULL," +
                    "PhoneNumber VARCHAR(20))" ;
                using(var sqlCommand = new SqlCommand(sqlCommandText, sqlConnection))
                {
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        private static void CreatePatientDiagnosisTable()
        {
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                var sqlCommandText = "CREATE TABLE PatientDiagnosis(DiagnosisID INT NOT NULL IDENTITY(1,1) PRIMARY KEY," +
                    "PatientID INT NOT NULL," +
                    "Details NVARCHAR(100) NOT NULL," +
                    "DiagnosisDate DATETIME DEFAULT GETDATE()," +
                    "Remark NVARCHAR(50)," +
                    "CONSTRAINT[FK_Patient_Diagnosis_Patient_ID] FOREIGN KEY([PatientID]) REFERENCES[Patient]([PatientID]) ON DELETE CASCADE)";
                using (var sqlCommand = new SqlCommand(sqlCommandText, sqlConnection))
                {
                    sqlCommand.ExecuteNonQuery();
                }
            }
        }

        public static void PrintPatientData()
        {
            foreach (DataRow row in patientTable.Rows)
            {
                Console.WriteLine($"{row["PatientID"]} {row["FirstName"]} {row["LastName"]} {row["Age"]} {row["Gender"]} {row["PhoneNumber"]}");
            }
        }

        private static DataTable GetPatientTable()
        {
            DataTable tmpTable = new DataTable("Patient");
            tmpTable.Columns.Add("PatientID", typeof(int));
            tmpTable.Columns.Add("FirstName", typeof(string));
            tmpTable.Columns.Add("LastName", typeof(string));
            tmpTable.Columns.Add("Age", typeof(int));
            tmpTable.Columns.Add("Gender", typeof(string));
            tmpTable.Columns.Add("PhoneNumber", typeof(string));

            return tmpTable;
        }

        private static SqlDataAdapter GetPatientAdapter()
        {
            SqlDataAdapter dataAdapter = new SqlDataAdapter("SELECT * FROM Patient;", connectionString);

            dataAdapter.InsertCommand = new SqlCommand("INSERT INTO Patient(FirstName, LastName, Age, Gender, PhoneNumber) " +
                "VALUES (@FirstName, @LastName, @Age, @Gender, @PhoneNumber)");
            dataAdapter.InsertCommand.Parameters.Add("@FirstName", SqlDbType.NVarChar, 25, "FirstName");
            dataAdapter.InsertCommand.Parameters.Add("@LastName", SqlDbType.NVarChar, 25, "LastName");
            dataAdapter.InsertCommand.Parameters.Add("@Age", SqlDbType.Int, 150, "Age");
            dataAdapter.InsertCommand.Parameters.Add("@Gender", SqlDbType.VarChar, 1, "Gender");
            dataAdapter.InsertCommand.Parameters.Add("@PhoneNumber", SqlDbType.VarChar, 20, "PhoneNumber");

            dataAdapter.UpdateCommand = new SqlCommand("UPDATE Patient " +
                "SET FirstName = @FirstName, LastName = @LastName, Age = @Age, Gender = @Gender, PhoneNumber = @PhoneNumber " +
                "where PatientID = @PatientID");
            dataAdapter.UpdateCommand.Parameters.Add("@FirstName", SqlDbType.NVarChar, 25, "FirstName");
            dataAdapter.UpdateCommand.Parameters.Add("@LastName", SqlDbType.NVarChar, 25, "LastName");
            dataAdapter.UpdateCommand.Parameters.Add("@Age", SqlDbType.Int, 150, "Age");
            dataAdapter.UpdateCommand.Parameters.Add("@Gender", SqlDbType.VarChar, 1, "Gender");
            dataAdapter.UpdateCommand.Parameters.Add("@PhoneNumber", SqlDbType.VarChar, 20, "PhoneNumber");

            SqlParameter updateParameter = dataAdapter.UpdateCommand.Parameters.Add("@PatientID", SqlDbType.Int);
            updateParameter.SourceColumn = "PatientID";
            updateParameter.SourceVersion = DataRowVersion.Original;

            dataAdapter.DeleteCommand = new SqlCommand("DELETE FROM Patient WHERE PatientID = @PatientID");
            SqlParameter deleteParameter = dataAdapter.DeleteCommand.Parameters.Add(
                "@PatientID", SqlDbType.Int);
            deleteParameter.SourceColumn = "PatientID";
            deleteParameter.SourceVersion = DataRowVersion.Original;

            return dataAdapter;
        }

        public static void InsertRowInPatientTable(Patient insertPatient)
        {
            var row = patientTable.NewRow();
            row["FirstName"] = insertPatient.FirstName;
            row["LastName"] = insertPatient.LastName;
            row["Age"] = insertPatient.Age;
            row["Gender"] = insertPatient.Gender;
            row["PhoneNumber"] = insertPatient.PhoneNumber;
            patientTable.Rows.Add(row);

            using (var sqlConnection = new SqlConnection(connectionString))
            {
                dataAdapter.InsertCommand.Connection = sqlConnection;
                dataAdapter.Update(patientTable);
                patientTable.Clear();
                dataAdapter.Fill(patientTable);
            }
        }

        public static IEnumerable<Patient> SelectRowsFromPatientTable()
        {
            var patientsList = new List<Patient>();
            foreach (DataRow patientRow in patientTable.Rows)
            {
                var patient = new Patient
                {
                    FirstName = (string)patientRow["FirstName"],
                    LastName = (string)patientRow["LastName"],
                    Age = (int)patientRow["Age"],
                    Gender = (string)patientRow["Gender"]
                };
                if (! (patientRow["PhoneNumber"] is DBNull))
                {
                    patient.PhoneNumber = (string)patientRow["PhoneNumber"];
                }
                patientsList.Add(patient);
            }
            return patientsList;
        }

        public static void UpdateRowInPatientTable(Patient updatePatient, int id)
        {
            foreach (DataRow row in patientTable.Rows)
            { 
                if ((int)row["PatientID"] == id)
                {
                    row["FirstName"] = updatePatient.FirstName;
                    row["LastName"] = updatePatient.LastName;
                    row["Age"] = updatePatient.Age;
                    row["Gender"] = updatePatient.Gender;
                    row["PhoneNumber"] = updatePatient.PhoneNumber;

                    using(var sqlConnection = new SqlConnection(connectionString))
                    {
                        dataAdapter.UpdateCommand.Connection = sqlConnection;
                        dataAdapter.Update(patientTable);
                    }
                    break;
                }
            }
        }

        public static void DeleteRowInPatientTable(int id)
        {
            foreach (DataRow row in patientTable.Rows)
            {
                if ((int)row["PatientID"] == id)
                {
                    row.Delete();

                    using (var sqlConnection = new SqlConnection(connectionString))
                    {
                        dataAdapter.DeleteCommand.Connection = sqlConnection;
                        dataAdapter.Update(patientTable);
                    }
                    break;
                }
            }
        }
    }

    class Patient
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public Patient(string firstName, string lastName, int age, string gender, string phoneNumber)
        {
            FirstName = firstName;
            LastName = lastName;
            Age = age;
            Gender = gender;
            PhoneNumber = phoneNumber;
        }

        public Patient() { }
    }
}
