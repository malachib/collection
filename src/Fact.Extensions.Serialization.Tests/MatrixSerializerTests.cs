#define SUPPRESS_OFFICE
#define MATRIXSERIALIZERTESTS_FULL

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Reflection;

using System;
using System.Linq;
#if FEATURE_SQLCE_TEST
using System.Data.SqlServerCe;
#endif
using System.IO;

using Fact.Extensions.Collection;
using Fact.Extensions.Configuration;
using Fact.Extensions.Serialization.Matrix;


#if !MATRIXSERIALIZERTESTS_FULL
#warning "Disabling some matrix serializer tests"
#endif

namespace TestProject1
{


    /// <summary>
    ///This is a test class for MatrixSerializerTest and is intended
    ///to contain all MatrixSerializerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MatrixSerializerTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

#region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
#endregion

        public class Contact
        {
            public string Last { get; set; }
            public string First { get; set; }

            public DateTime? DOB { get; set; }

            public Contact(Contact copyFrom)
            {
                Last = copyFrom.Last;
                First = copyFrom.First;
                DOB = copyFrom.DOB;
            }
        }

#if FEATURE_APPRENTICE_ORM
        IEnumerable<Contact> GenerateList()
        {
            /*
            Contact contact1 = new Contact() { ID = 1 },
                    contact2 = new Contact() { ID = 2 };
            List<Contact> contacts = new List<Contact>();

            contact1.populateDebugValues();
            contact2.populateDebugValues();

            contacts.Add(contact1);
            contacts.Add(contact2);
            return contacts;
            */
            string sql = "select * from Contact"; // TODO: Initialize to an appropriate value
            var actual = ORM.Default.ReadFast<Contact>(sql);
            return actual;
        }
#endif

        public class USZipCodeRecord
        {
            public int Zip { get; set; }
            [Length(13)]
            public string City { get; set; }
            public string State { get; set; }
            public int Timezone { get; set; }
            public int DST { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }

        public class TechNetProductRecord
        {
            public string Product { get; set; }
            public string TechNetPro { get; set; }
            public string TechNetStd { get; set; }
        }


#if FEATURE_MATRIXSERIALIZER_EXCEL
        [TestMethod]
        [DeploymentItem("Data\\minizip.csv")]
        [DeploymentItem("Data\\empty.xls")]
        public void ExcelMatrixSerializerTest()
        {
            //using (var tx = new TransactionScope())
            // JET doesn't support transactions
            {
                var d = new CSVMatrixDeserializer("minizip.csv", true);
                var s = new ExcelMatrixSerializer("empty.xls");

                var rows = d.List<USZipCodeRecord>().ToArray();
                var row = rows.First();

                s.ORM.DbClient.ConnectionManager.OpenConnectionBehavior = DbClient.OpenConnectionBehaviorEnum.NeverHold;

                // working basically, but column order still an issue
                s.ORM.CreateTable(typeof(USZipCodeRecord), "Zippies", ORM.TableExistsAction.Abort);
                //var schemas = s.ORM.GetSchema().ToArray();
                s.ORM.DbClient.ExecuteNonQuery("DROP TABLE [Sheet1$]");
                //s.ORM.Create(row);
                s.ORM.CreateBatch(rows);
                System.Data.OleDb.OleDbConnection.ReleaseObjectPool();
                //System.Threading.Thread.Sleep(1000);
                //GC.Collect();
                //s.ORM.CloseConnection(null);

                // OLEDB not flushing right away, don't know how to force it.  Unit tests evaporate after completion, so difficult 
                // to inspect fully generated file
                //tx.Complete();
                //System.Threading.Thread.Sleep(1000);
            }
            //d.CopyTo(s);
        }

        // If this generates the ACE driver error, download it from here:
        // http://www.microsoft.com/en-us/download/details.aspx?id=13255
        [TestMethod]
        [DeploymentItem("Data\\TechNet_Product_List.xlsx")]
        public void ExcelMatrixDeserializerTest()
        {
            ExcelMatrixDeserializer.IsXLSXSupported = true;

            ExcelMatrixDeserializer d;

            try
            {
                d = new ExcelMatrixDeserializer("TechNet_Product_List.xlsx", "TechNet");

            }
            catch (InvalidOperationException ioe)
            {
                Assert.AreEqual("System.Data", ioe.Source);
                Assert.Inconclusive("Needs MS ACE driver installed");
                return;
            }

            var items = d.List.Skip(1).Take(20).Select(x => x.ToArray()).ToArray();
            var items2 = from i in items
                         let product = i[0]
                         where product != null && product != DBNull.Value
                         select new TechNetProductRecord()
                         {
                             Product = (string)product,
                             TechNetPro = i[2].ToString(),
                             TechNetStd = i[3].ToString()
                         };

            Fact.Apprentice.Core.UnitTest.Utility.CompareStored(items2, "DeserializeExcelTest");
        }
#endif
        // VS2012 doesn't have the 3.5 CE stuff built in
#if FEATURE_SQLCE_TEST
        [TestMethod]
        public void SerializeFixedLengthTest()
        {
            var conn = new SqlCeConnection(ORM.Default.DbClient.ConnectionString);
            var cmd = new SqlCeCommand("select Last, First, DOB, ID from Contact", conn);

            var outputFile = new FileStream("serializeFixedLengthTest.txt", FileMode.Create);
            var writer = new StreamWriter(outputFile);

            conn.Open();

            var input = new DataReaderMatrixDeserializer(cmd.ExecuteReader());
            var output = new StringMatrixSerializer(writer);

            output[0] = new FixedLengthMatrixSerializer.ColumnDescriptor(30, false, '#') { Name = "Last" };

            var col2 = new FixedLengthMatrixSerializer.ColumnDescriptor(30);
            col2.AddFormatter(s => s.ObscureLeft(2, '%'));
            col2.Name = "First";

            output[1] = col2;

            input.CopyTo(output);
        }
#endif

        [TestMethod]
        [DeploymentItem("Data\\minizip.csv")]
        public void EntityBuilderTest()
        {
            var baseDirectory = System.AppContext.BaseDirectory;
            var filename = baseDirectory + "\\Data\\minizip.csv";
            IMatrixDeserializer input = new CSVMatrixDeserializer(filename, true);
            var eb = new PropertyEntityBuilder(input, typeof(USZipCodeRecord));

            foreach (var item in input.List)
            {
                var entity = (USZipCodeRecord)eb.RowToEntity(item);
            }

            input = new CSVMatrixDeserializer(filename, true);

            var list = input.List<USZipCodeRecord>().ToArray();

            input = new CSVMatrixDeserializer(filename, true);

            list = input.List<USZipCodeRecord>("zip", "city", "state").ToArray();

            // ---
            input = new CSVMatrixDeserializer(filename, true);

            list = input.List<USZipCodeRecord>().ToArray();

            input = new CSVMatrixDeserializer(filename, true);

            list = input.List<USZipCodeRecord>("zip", "city", "state").ToArray();
        }

        [TestMethod]
        [DeploymentItem("Data\\minizip.txt")]
        public void DeserializeFixedLength2Test()
        {
            var baseDirectory = System.AppContext.BaseDirectory;
            var filename = baseDirectory + "\\Data\\minizip.txt";
            var d = new FixedLengthMatrixDeserializer(filename, true, 6, 13, 3, 11, 12, 4, 1);
            var list = d.List<USZipCodeRecord>("zip", "city", "state");

            foreach (var row in list)
            {
            }

            d = new FixedLengthMatrixDeserializer(filename, true);

            d.AddColumn("zip", 6);
            d.AddColumn("city", 13);
            d.AddColumn("state", 3);

            list = d.List<USZipCodeRecord>();

            foreach (var row in list)
            {
            }
        }


        [TestMethod]
        [DeploymentItem("Data\\minizip.txt")]
        public void DeserializeFixedLengthTest()
        {
            var baseDirectory = System.AppContext.BaseDirectory;
            var filename = baseDirectory + "\\Data\\minizip.txt";
            var d = new FixedLengthMatrixDeserializer(filename, true, 6, 13, 3, 11, 12, 4, 1);

            foreach (var row in d.List)
            {
            }
        }

        [TestMethod]
        [DeploymentItem("Data\\minizip.csv")]
        public void SerializeFixedLengthCopyTest()
        {
            var baseDirectory = System.AppContext.BaseDirectory;
            var filename = baseDirectory + "\\Data\\minizip.csv";
            var d = new CSVMatrixDeserializer(filename, true);
            var s = new FixedLengthMatrixSerializer("minizip_test.txt", 20, 20, 20, 20, 20, 20, 20);

            // a bit clumsy, needed so that columns will match up
            var eb = new PropertyEntityBuilder(d, typeof(USZipCodeRecord));
            var cb = new PropertyColumnBuilder(s, typeof(USZipCodeRecord));

            foreach (var row in d.List)
            {
                object entity = eb.RowToEntity(row);
                var newRow = cb.EntityToRow(entity);

                s.Append(newRow);
            }

            s.Commit();
            s.Close();
        }


        [TestMethod]
        [DeploymentItem("Data\\minizip.csv")]
        public void SerializeFixedLengthCopy2Test()
        {
            var baseDirectory = System.AppContext.BaseDirectory;
            var filename = baseDirectory + "\\Data\\minizip.csv";
            var d = new CSVMatrixDeserializer(filename, true);
            var s = new FixedLengthMatrixSerializer("minizip_test.txt");

            // Set up columns in serializer to roughly mimic columns from deserializer
            foreach (var column in d.GetColumnNames())
                s.AddColumn(column, 20);

            d.CopyTo(s);
        }


        [TestMethod]
        [DeploymentItem("Data\\minizip.csv")]
        public void DeserializeCSV2Test()
        {
            var baseDirectory = System.AppContext.BaseDirectory;
            var filename = baseDirectory + "\\Data\\minizip.csv";
            var d = new CSVMatrixDeserializer(filename, true);

            foreach (var row in d.List)
            {
            }
        }


        [TestMethod]
        [DeploymentItem("Data\\empty.csv")]
        public void DeserializeEmptyCSVTest()
        {
            var baseDirectory = System.AppContext.BaseDirectory;
            var filename = baseDirectory + "\\Data\\empty.csv";
            var d = new CSVMatrixDeserializer(filename, true);

            foreach (var row in d.List)
            {
                var row2 = row.ToArray();
            }
        }


#if FEATURE_APPRENTICE_ORM
        [TestMethod]
        [DeploymentItem("Data\\minizip.csv")]
        public void DeserializeCSVTest()
        {
            var orm = ORM.Default;

            // this one converts the property types as a part of the cast (List<>)
            using (var ts = new TransactionScope())
            {
                var file = new System.IO.StreamReader("minizip.csv");

                IMatrixDeserializer input = new CSVMatrixDeserializer(file, true);

                orm.CreateTable(typeof(USZipCodeRecord), "ZipCode", ORM.TableExistsAction.Abort);
                orm.CreateBatch(input.List<USZipCodeRecord>());
                var result = orm.Read<USZipCodeRecord>();
            }

            {
                var file = new System.IO.StreamReader("minizip.csv");
                var memoryStream = new MemoryStream();
                var stream = new System.IO.Compression.DeflateStream(memoryStream, System.IO.Compression.CompressionMode.Compress);

                IMatrixDeserializer input = new CSVMatrixDeserializer(file, true);

                var sorted = new SortedList<int, USZipCodeRecord>();

                var list = input.List<USZipCodeRecord>();

                var filtered = from n in list
                               where n.Zip % 2 == 0
                               select n;

                var ser = new BinarySerializer(typeof(USZipCodeRecord));

                ser.Serialize(memoryStream, filtered.First());

                //stream.Flush();
                memoryStream.Seek(0, SeekOrigin.Begin);

                //stream = new System.IO.Compression.DeflateStream(memoryStream, System.IO.Compression.CompressionMode.Decompress);

                var obj = ser.Deserialize(memoryStream);

                var bytes = BinarySerializer.Serialize(filtered.First());
                var obj2 = BinarySerializer.Deserialize<USZipCodeRecord>(bytes);
            }
        }
#endif

        // troubles using SQL CE 3.5, VS11 will only link to SQL CE 4.0 drivers, which can't read
        // 3.5 SDF files
#if FEATURE_MATRIXSERIALIZER_EXCEL
        /// <summary>
        ///A test for Serialize
        ///</summary>
        [TestMethod()]
        public void SerializeCSVTest()
        {
            var file = new System.IO.StreamWriter("testout.csv");
            var file2 = new System.IO.StreamWriter("testout2.csv");
            var contacts = GenerateList();

            var conn = new SqlCeConnection(ORM.Default.DbClient.ConnectionString);
            var cmd = new SqlCeCommand("select * from Contact", conn);

            conn.Open();
            IMatrixDeserializer input2 = new DataReaderMatrixDeserializer(cmd.ExecuteReader());
            IMatrixSerializer output2 = new CSVMatrixSerializer(file2) { DelimitAll = true };

            IMatrixDeserializer input = new EnumerableMatrixDeserializer<Contact>(contacts);
            IMatrixSerializer output = new CSVMatrixSerializer(file);

            MatrixSerializer target = new MatrixSerializer(input, output, NotifyDelegate);
            target.Serialize();


            target = new MatrixSerializer(input2, output2, NotifyDelegate);
            target.Serialize();

            file.Close();
            file2.Close();
            conn.Close();
        }
#endif

        void NotifyDelegate(string message, System.Diagnostics.TraceEventType type)
        {
            Console.Out.WriteLine(message);
        }

        public class Contact2 : Contact
        {
            public object MLID { get; set; }
            //public string FailStr { get { return Resource.EXPORT_FAILING_STR_DEBUG; } }
            public string FailStr => "FAIL";

            public Contact2(Contact copyFrom) : base(copyFrom) { }
        }

#if MOVING_TO_SEPARATE_UNITTEST
#if !SUPPRESS_OFFICE
        [TestMethod()]
        [DeploymentItem("Data\\Database1.sdf")]
        public void SerializeXLSTest()
        {
            var _contacts = GenerateList();
            //var contacts = from n in _contacts select new Contact2(n);

            var contacts = _contacts.Select((x, y) => new Contact2(x));

            IMatrixDeserializer input = new EnumerableMatrixDeserializer<Contact2>(contacts);
            IMatrixSerializer output = new ExcelMatrixSerializer(Assembly.GetExecutingAssembly().Location + @"_SerializeTest.xls");

            MatrixSerializer target = new MatrixSerializer(input, output, null); // TODO: Initialize to an appropriate value
            target.Serialize();
        }
#endif
#endif
    }
}