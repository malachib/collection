#if NETSTANDARD1_3 || NETSTANDARD1_4
#define NETCORE
#else
#define TRANSACTIONS_ENABLED
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

using System.Data;

using Fact.Extensions.Collection;
using Fact.Extensions.Configuration;

#if TRANSACTIONS_ENABLED
using System.Transactions;
#endif
#if !MONODROID && !NETCORE
using System.Data.OleDb;
#endif
//using System.Data.Common;
using System.Collections;
//using Fact.Apprentice.Core.DAL;
//using Fact.Apprentice.Core.Validation;

/*
 * Important to know: ColumnDescriptor and associated interfaces from MatrixSerializer hierarchy also DIRECTLY get used
 * in MatrixDeserializer classes
 */
namespace Fact.Extensions.Serialization.Matrix
{
    /// <summary>
    /// Pull grid-based information from a data store into memory
    /// </summary>
    public interface IMatrixDeserializer
    {
        int ColumnCount { get; }

        // can return null if length not available
        int? Length { get; }

        /// <summary>
        /// Return the name of the column at the given index
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        string GetColumnName(int columnIndex);

        /// <summary>
        /// Return all data pulled from data store
        /// </summary>
        IEnumerable<IEnumerable<object>> List { get; }

        void Cleanup();
    }

    /// <summary>
    /// Take stuff in memory, flush it to a persist store
    /// </summary>
    public interface IMatrixSerializer
    {
        int ColumnCount { set; }

        // optional parameter to help output, if length is available
        int? Length { set; }

        string NotifierName { get; }

        void SetColumnName(int columnIndex, string name);

        /// <summary>
        /// Append actual data row to data store
        /// </summary>
        /// <param name="value"></param>
        void Append(IEnumerable value);

        /// <summary>
        /// Flush/commit to store
        /// </summary>
        /// <remarks>
        /// TODO: Document why specifically we have this + Close vs. just a close.  
        /// Not all serializers support a 'flush' operation, but if memory serves, others really need
        /// an explicit call for the flush
        /// </remarks>
        void Commit();

        /// <summary>
        /// Complete and close out underlying store
        /// </summary>
        void Close();
    }


    public interface IMatrixColumnAccessor
    {
        MatrixSerializer.ColumnDescriptor this[int index] { get; set; }
    }


    public interface IMatrixSerializerColumns : IMatrixColumnAccessor, IMatrixSerializer
    {
        IEnumerable<MatrixSerializer.ColumnDescriptor> Columns { get; }
    }

    public class DataReaderMatrixDeserializer : IMatrixDeserializer
    {
        IDataReader reader;

        public DataReaderMatrixDeserializer(IDataReader reader)
        {
            this.reader = reader;
        }

        #region IMatrixSerializerInput Members

        public int ColumnCount
        {
            get { return reader.FieldCount; }
        }

        public string GetColumnName(int columnIndex)
        {
            return reader.GetName(columnIndex);
        }

        public int? Length
        {
            get { return null; }
        }

        public IEnumerable<IEnumerable<object>> List
        {
            get
            {
                object[] result = new object[ColumnCount];

                while (reader.Read())
                {
                    reader.GetValues(result);
                    yield return result;
                }
            }
        }

        public void Cleanup()
        {
        }

        #endregion
    }


#if UNUSED
    public class MatrixDeserializer
    {
    }

    public class EnumerableMatrixDeserializer : IMatrixDeserializer
    {
        public int ColumnCount
        {
            get { throw new NotImplementedException(); }
        }

        public int? Length
        {
            get { throw new NotImplementedException(); }
        }

        public string GetColumnName(int columnIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object[]> List
        {
            get { throw new NotImplementedException(); }
        }

        public void Cleanup()
        {
            throw new NotImplementedException();
        }
    }
#endif


    public class EnumerableMatrixDeserializer<T> : IMatrixDeserializer
    {
        PropertyInfo[] properties = typeof(T).GetTypeInfo().DeclaredProperties.ToArray();
        IEnumerable<T> enumerable;

        public EnumerableMatrixDeserializer(IEnumerable<T> enumerable)
        {
            this.enumerable = enumerable;
        }

        #region IMatrixSerializerInput Members

        public int? Length
        {
            get
            {
                var collection = enumerable as ICollection<T>;

                if (collection != null)
                {
                    return collection.Count;
                }
                return null;
            }
        }

        public int ColumnCount
        {
            get { return properties.Length; }
        }

        public string GetColumnName(int columnIndex)
        {
            return properties[columnIndex].Name;
        }

        public IEnumerable<IEnumerable<object>> List
        {
            get
            {
                var result = new object[ColumnCount];

                foreach (var item in enumerable)
                {
                    for (int i = 0; i < properties.Length; i++)
                    {
                        var prop = properties[i];

                        result[i] = prop.GetValue(item, null);
                    }

                    yield return result;
                }
            }
        }

        public void Cleanup()
        {
        }

        #endregion
    }




    public class CSVMatrixSerializer : StringMatrixSerializer, IMatrixSerializer
    {
        string delimiter = "\"";
        string quoteEscape = "\"\"";

        /// <summary>
        /// String quote delimiter
        /// </summary>
        public string Delimiter { get { return delimiter; } set { delimiter = value; } }
        /// <summary>
        /// Inspect and quote-encode encountered strings
        /// </summary>
        public bool DelimitAll { get; set; }

        public CSVMatrixSerializer(StreamWriter writer) : base(writer) { }

        public CSVMatrixSerializer(string filename) :
            base(new StreamWriter(new FileStream(filename, FileMode.Create)))
        {
        }

        #region IMatrixSerializerOutput Members

        public override int? Length { set; protected get; }

        protected string quoteEncode(string s)
        {
            var delimiter = Delimiter;
            return delimiter + s.Replace(delimiter, quoteEscape) +
                delimiter;
        }

        protected override string Format(ColumnDescriptor columnDescriptor, string value)
        {
            // FIX: kludgey, delimit flag should touch this. Make a delimit-behavior-enum
            //if (value.Contains(','))
            //  return quoteEncode(value);

            if (DelimitAll)
                return quoteEncode(value);

            return value;
        }

        public override void SetColumnName(int columnIndex, string name)
        {
            base.SetColumnName(columnIndex, name);

            // If we're setting column names, then write the header
            writeHeader = true;
        }

        bool writeHeader = false;

        public bool WriteHeader { get { return writeHeader; } set { writeHeader = value; } }

        protected override void AppendField(int index, object value)
        {
            if (index > 0)
                output.Write(',');

            base.AppendField(index, value);
        }

        public override void Append(IEnumerable value)
        {
            if (writeHeader)
            {
                output.WriteLine(Columns.Select(x => x.Name).ToString(","));
                writeHeader = false;
            }

            base.Append(value);

            /*
            string result = null;

            for (int i = 0; i < value.Length; i++)
            {
                ColumnDescriptor cd;
                
                if (columns != null)
                    cd = columns[i];

                object _value = value[i];
                string coerced;

                if (DelimitAll || _value is string)
                    coerced = Delimiter + _value.ToString().Replace(Delimiter, quoteEscape) + Delimiter;
                else
                    coerced = (_value ?? "").ToString();

                if (result != null)
                    result += "," + coerced;
                else
                    result = coerced;
            }

            output.WriteLine(result);*/
        }

        public override string NotifierName
        {
            get
            {
                var fs = output.BaseStream as FileStream;

                if (fs != null) return fs.Name;

                return "";
            }
        }


        #endregion
    }


    public class StringMatrixSerializer : MatrixSerializerBase, IMatrixSerializerColumns
    {
        IEnumerable<MatrixSerializer.ColumnDescriptor> IMatrixSerializerColumns.Columns { get { return Columns; } }

        protected StringMatrixSerializer() { }

        public StringMatrixSerializer(StreamWriter output)
        {
            this.output = output;
        }

        protected StreamWriter output;

        public interface IFormatterColumnDescriptor
        {
            void AddFormatter(Func<string, string> formatter);
        }

        public class FormatterColumnDescriptor : MatrixSerializer.ColumnDescriptor,
            IFormatterColumnDescriptor
        {
            internal LinkedList<Func<string, string>> Formatters = new LinkedList<Func<string, string>>();

            public void AddFormatter(Func<string, string> formatter)
            {
                Formatters.AddLast(formatter);
            }
        }

        public class ColumnDescriptor : FormatterColumnDescriptor
        {
            /// <summary>
            /// Can convert, format or produce new data from the given input for this data column value
            /// </summary>
            public Func<object, string> Converter;

            public ColumnDescriptor()
            {
                Converter = StringMatrixSerializer.Convert;
            }

            public ColumnDescriptor(Func<object, string> converter)
            {
                this.Converter = converter;
            }

            public string Format(object value)
            {
                var columnValue = Converter(value);

                foreach (var formatter in Formatters)
                    columnValue = formatter(columnValue);

                return columnValue;
            }
        }

        public virtual int? Length
        {
            set { }
            protected get { return null; }
        }

        public virtual string NotifierName
        {
            get { throw new NotImplementedException(); }
        }

        protected override MatrixSerializer.ColumnDescriptor CreateColumnDescriptor()
        {
            return new ColumnDescriptor();
        }


        /// <summary>
        /// Global formatter, runs after local/column formatter does
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual string Format(ColumnDescriptor columnDescriptor, string value)
        {
            return value;
        }

        /// <summary>
        /// Convert from native format to a string, global version.  
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected static string Convert(object value)
        {
            if (value == null) return string.Empty;

            return value.ToString();
        }

        /// <summary>
        /// Appends given field to the output writer, calling both local (Column) and
        /// global (instance) formatters
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        protected virtual void AppendField(int index, object value)
        {
            // First, acquire column metadata if it's available
            var cd = (ColumnCount > index) ?
                Columns[index] as ColumnDescriptor :
                null;

            // Next, format incoming value according to column metadata, otherwise do standard global
            // conversion
            var formatted = cd != null ? cd.Format(value) : Convert(value);

            // Finally, take converted & formatted data and run it through a final global formatter,
            // and output it
            output.Write(Format(cd, formatted));
        }

        /// <summary>
        /// Write the matrix array line to the underlying output writer
        /// </summary>
        /// <param name="value"></param>
        public virtual void Append(IEnumerable value)
        {
            int i = 0;
            foreach (var item in value) AppendField(i++, item);
            /*
        for (int i = 0; i < value.Length; i++)
            AppendField(i, value[i]);*/

            AppendLine();
        }


        protected virtual void AppendLine()
        {
            output.WriteLine();
        }

        public void Commit()
        {
            output.Flush();
        }

        public void Close()
        {
#if NETCORE
            output.Dispose();
#else
            output.Close();
#endif
        }
    }


    /// <summary>
    /// Deserializer specifically targeting text-file sources which are read
    /// line by line and then split into constituent fields.  If no specific
    /// "Type" or formatter clues are given to the field column, the field will remain as
    /// a string
    /// </summary>
    public abstract class StringMatrixDeserializer : IMatrixDeserializer, IMatrixColumnAccessor
    {
        protected List<ColumnDescriptor> columns = new List<ColumnDescriptor>();

        public ColumnDescriptor this[string name]
        {
            get { return columns.First(x => x.Name == name); }
        }


        public ColumnDescriptor this[int index]
        {
            get { return columns[index]; }
        }

        MatrixSerializer.ColumnDescriptor IMatrixColumnAccessor.this[int index]
        {
            get { return columns[index]; }
            set { throw new InvalidOperationException(); }
        }

        //public List<ColumnDescriptor> Columns { get { return columns; } }

        /// <summary>
        /// Mimics behavior of StringMatrixSerializer.ColumnDescriptor, but obviously
        /// in the other direction
        /// </summary>
        public abstract class ColumnDescriptor : StringMatrixSerializer.FormatterColumnDescriptor
        {
            /// <summary>
            /// Can convert, format or produce new data from the given input for this data column value
            /// param 1 = input string value
            /// param 2 = expected format
            /// return = converted value
            /// </summary>
            public Func<string, Type, object> Converter;

            /// <summary>
            /// When Type is set, conversion from string to specified type is attempted
            /// Setting Converter will bypass this automatic conversion with a specific one
            /// </summary>
            public Type Type { get; set; }

            // TODO: Would be expedient to have a glue connect EntityBuilder directly
            // to the ColumnDescriptor.  TBD how to do this
            public object Parse(string value)
            {
                foreach (var formatter in Formatters)
                    value = formatter(value);

                if (Converter != null)
                {
                    var columnValue = Converter(value, Type);

                    return columnValue;
                }
                else if (Type != null)
                {
                    return StringMatrixDeserializer.Convert(value, Type);
                }
                else
                {
                    return value;
                }
            }

            /// <summary>
            /// Given a particular starting position on the line, extract the value to be
            /// yielded in a column of IMatrixDeserializer.List row.  Note that conversion
            /// or formatting does not occur here, just value extraction
            /// </summary>
            /// <param name="line">full line to be parsed</param>
            /// <param name="start">position within line to parse</param>
            /// <param name="end">new start position for next column</param>
            /// <returns>extracted value (typically similar to a substring between start/end)</returns>
            public abstract string Extract(string line, int start, out int end);
        }

        /// <summary>
        /// Convert/parses from a string to a native format of specified type
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected static object Convert(string value, Type type)
        {
            // NOTE: If converting from a string to a string (could happen, although
            // pointless) this will "eat up" empty strings and convert them to null
            if (string.IsNullOrEmpty(value))
                return null;

            return System.Convert.ChangeType(value, type);
        }


        public int ColumnCount
        {
            get { return columns.Count; }
        }

        protected TextReader reader;

        public StringMatrixDeserializer(StreamReader reader)
        {
            this.reader = reader;
        }


        public int? Length
        {
            get { return null; }
        }

        public string GetColumnName(int columnIndex)
        {
            return columns[columnIndex].Name;
        }

        protected virtual string ReadLine()
        {
            return reader.ReadLine().Trim();
        }

        /// <summary>
        /// Take a single line and split it apart
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        protected virtual IEnumerable<object> SplitAndParse(string line)
        {
            int start = 0;

            foreach (var c in columns)
            {
                var value = c.Extract(line, start, out start);
                yield return c.Parse(value);
            }
        }


        public virtual IEnumerable<IEnumerable<object>> List
        {
            get
            {
                var streamReader = reader as StreamReader;

                // If this is a TextReader but not a StreamReader, then this is an infinite loop
                // If this is a StreamReader, then we know how to check for end of stream
                // logic breaks down as:
                // while streamReader == null, continue.  streamReader.EndOfStream won't even be evaluated
                // while streamReader != null, then evaluate for streamReader.EndOfStream
                while (streamReader == null || !streamReader.EndOfStream)
                {
                    var line = ReadLine();

                    if (string.IsNullOrEmpty(line))
                        continue;

                    var row = SplitAndParse(line);
                    yield return row;
                }

                // EXPERIMENTAL
                if (streamReader.EndOfStream)
                    Cleanup();
            }
        }

        public void Cleanup()
        {
#if NETCORE
            reader.Dispose();
#else
            reader.Close();
#endif
        }
    }

    public class CSVMatrixDeserializer : StringMatrixDeserializer
    {
        new public class ColumnDescriptor : StringMatrixDeserializer.ColumnDescriptor
        {
            CSVMatrixDeserializer parent;

            char[] StringDelimiters { get { return parent.StringDelimiters; } }
            char ColumnDelimiter { get { return parent.ColumnDelimiter; } }

            public ColumnDescriptor(CSVMatrixDeserializer parent)
            {
                this.parent = parent;
            }

            public override string Extract(string line, int start, out int end)
            {
                bool inDelimiters = false;
                bool inString = false; // manual trim function
                // actual start & end are the start and end inside any potential delimiters
                int actualStart = -1;
                int actualEnd = -1;
                int i = start;

                if (start > line.Length)
                {
                    if (parent.MissingColumnsAsEmpty)
                    {
                        end = start;
                        return "";
                    }
                    else
                        throw new IndexOutOfRangeException("Attempting to read columns which aren't there.  Are all trailing columns present?");
                }

                for (; i < line.Length; i++)
                {
                    var c = line[i];

                    // TODO: no support yet for nested quotes
                    if (StringDelimiters.Contains(c))
                    {
                        inDelimiters = !inDelimiters;
                        if (!inDelimiters)
                            actualEnd = i;
                        else
                        {
                            inString = true; // don't eat spaces if we're contained within quotes
                            actualStart = i + 1;
                            if (actualStart >= line.Length)
                                throw new InvalidOperationException("Rogue double-quote ending the line");
                        }
                    }
                    else
                    {
                        // implicit trim
                        if (!inString)
                        {
                            // if not yet beginning string, eat spaces before the beginning of the value
                            if (c == ' ')
                                continue;

                            inString = true;
                        }

                        if (actualStart == -1)
                            actualStart = i;

                        if (inDelimiters)
                        {
                            // ignore everything within string delimiters until we get
                            // another string delimiter
                        }
                        else
                        {
                            if (c == ColumnDelimiter)
                            {
                                // actualEnd will be assigned if there was an inString condition
                                // and if this condition exists, we've already got the end of the
                                // value, so this part can skip over any possible whitespace
                                if (actualEnd == -1)
                                    actualEnd = i;

                                break;
                            }
                        }
                    }
                }
                end = i + 1;

                // if at the very end of a CSV row with no value (a comma followed by nothing), 
                // we will have actualStart == -1
                if (actualStart == -1)
                    actualStart = i;

                if (actualEnd == -1)
                    actualEnd = i;

                // FIX: glitch with empty strings i.e. ""
                var value = line.Substring(actualStart, actualEnd - actualStart);
                return value.Trim();
            }
        }

        /// <summary>
        /// This constructor does NOT perform auto initialization.  Use this
        /// to set instance flags, then subsequently call Initialize to 
        /// properly build columns and other initialization activities
        /// </summary>
        /// <param name="reader"></param>
        public CSVMatrixDeserializer(StreamReader reader) : base(reader)
        {
        }


        /// <summary>
        /// This constructor does NOT perform auto initialization.  Use this
        /// to set instance flags, then subsequently call Initialize to 
        /// properly build columns and other initialization activities
        /// </summary>
        /// <param name="reader"></param>
        public CSVMatrixDeserializer(string filename)
            : base(new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read)))
        {
        }


        /// <summary>
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="hasHeader"></param>
        public CSVMatrixDeserializer(string filename, bool hasHeader) :
            this(new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read)), hasHeader)
        {
        }

        /// <summary>
        /// Subsequent Initialize() call not required
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="hasHeader">When TRUE, first line will be evaluated as a header and not
        /// returned in List
        /// When FALSE, first line will still be treated as a header to find column count,
        /// but returned as part of List</param>
        public CSVMatrixDeserializer(StreamReader reader, bool hasHeader) :
            base(reader)
        {
            HasHeader = hasHeader;
            Initialize();
            /*
            var line = ReadLine();

            if (hasHeader)
            {
                InitHeader(line);
            }
            else
            {
                InitFakeHeader(line);
                firstLine = line;
            }*/
        }

        public int LinesToSkip { get; set; }
        public bool HasHeader { get; set; }

        /// <summary>
        /// If a line ends early, as in not enough CSV values, treat the line as trailing with empty values (true).
        /// Default of false throws an exception as it can't find a proper value for the column
        /// </summary>
        public bool MissingColumnsAsEmpty { get; set; }

#if DEBUG
        bool isInitialized;
#endif

        public virtual void Initialize()
        {
#if DEBUG
            isInitialized = true;
#endif
            string line = null; // = null just to suppress uninitialized complaint.  LinesToSkip + 1 ensures it gets actually initialized

            for (int i = 0; i < LinesToSkip + 1; i++)
                line = ReadLine();

            if (HasHeader)
            {
                InitHeader(line);
            }
            else if (columns.Count == 0) // if column order has been specified, then we actually have columns here already
            {
                InitFakeHeader(line);
                firstLine = line;
            }
        }

        /// <summary>
        /// This constructor may have bugs.  Initialize() call not required
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="hasHeader"></param>
        /// <param name="columnNames"></param>
        public CSVMatrixDeserializer(StreamReader reader, bool hasHeader, params string[] columnNames) :
            base(reader)
        {
#if DEBUG
            isInitialized = true;
#endif
            // For now, hasHeader just indicates to read and skip the first line, since
            // we are manually specifying column names here
            if (hasHeader)
            {
                ReadLine();
            }

            foreach (var name in columnNames)
                columns.Add(new ColumnDescriptor(this) { Name = name });
        }

        protected void InitFakeHeader(string line)
        {
            int start = 0;
            int count = 0;

            while (start < line.Length)
            {
                count++;
                var c = new ColumnDescriptor(this) { Name = count.ToString() };
                c.Extract(line, start, out start);

                columns.Add(c);
            }
        }

        protected void InitHeader(string line)
        {
            int start = 0;

            while (start < line.Length)
            {
                var c = new ColumnDescriptor(this);
                var columnName = c.Extract(line, start, out start);
                c.Name = columnName;

                columns.Add(c);
            }
        }

        char[] StringDelimiters = new char[] { '"' };
        char ColumnDelimiter = ',';
        string firstLine;

        public override IEnumerable<IEnumerable<object>> List
        {
            get
            {
#if DEBUG
                if (!isInitialized)
                    throw new InvalidOperationException("Must call Initialize() method first!");
#endif
                if (firstLine != null)
                    return base.List.Prepend(SplitAndParse(firstLine));
                else
                    return base.List;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// TODO: Can make a width-deducer if a proper header is present
    /// </remarks>
    public class FixedLengthMatrixDeserializer : StringMatrixDeserializer,
        IMatrixFixedLengthColumnHandler
    {
        new public class ColumnDescriptor : StringMatrixDeserializer.ColumnDescriptor,
            FixedLengthMatrixSerializer.IColumnDescriptor
        {
            public int Length { get; set; }

            public ColumnDescriptor(int length)
            {
                Length = length;

                Formatters.AddLast(s => s.Trim());
            }

            public override string Extract(string line, int start, out int end)
            {
                end = start + Length;
                return line.Substring(start, Length);
            }
        }

        public FixedLengthMatrixDeserializer(string filename, bool hasHeader, params int[] lengths)
            : this(new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read)), hasHeader, lengths) { }

        public FixedLengthMatrixDeserializer(StreamReader input, bool hasHeader, params int[] lengths)
            : base(input)
        {
            // Just eat up first line if there's a header
            if (hasHeader)
                ReadLine();

            foreach (var length in lengths)
            {
                var c = new ColumnDescriptor(length);
                columns.Add(c);
            }
        }

        /// <summary>
        /// Add column with name and width for fixed length serializer to use
        /// Be sure to add in proper order
        /// </summary>
        /// <param name="name"></param>
        /// <param name="width"></param>
        public ColumnDescriptor AddColumn(string name, int length)
        {
            var cd = new ColumnDescriptor(length) { Name = name };
            columns.Add(cd);
            return cd;
        }

        void IMatrixFixedLengthColumnHandler.AddColumn(string name, int length)
        {
            this.AddColumn(name, length);
        }

        public new ColumnDescriptor this[string columnName]
        {
            get
            {
                return (ColumnDescriptor)base[columnName];
            }
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// TODO: still need non-clumsy way of manually assigning lengths to particular columns
    /// </remarks>
    public class FixedLengthMatrixSerializer : StringMatrixSerializer,
        IMatrixFixedLengthColumnHandler
    {
        protected FixedLengthMatrixSerializer() { }

        public FixedLengthMatrixSerializer(StreamWriter output) : base(output) { }

        public FixedLengthMatrixSerializer(string filename, params int[] lengths) :
            base(new StreamWriter(new FileStream(filename, FileMode.Create)))
        {
            foreach (var length in lengths)
            {
                AddColumn(length);
            }
        }

        public void AddColumn(string name, int length)
        {
            Columns.Add(new ColumnDescriptor(length) { Name = name });
        }

        public void AddColumn(int length)
        {
            AddColumn(null, length);
        }

        protected override string Format(StringMatrixSerializer.ColumnDescriptor columnDescriptor, string value)
        {
            var cd = (IColumnDescriptor)columnDescriptor;

            // auto-truncates values to length maximum 
            if (value.Length > cd.Length)
                return value.Substring(0, cd.Length);

            return value;
        }

        protected override MatrixSerializer.ColumnDescriptor CreateColumnDescriptor()
        {
            return new ColumnDescriptor();
        }

        /// <summary>
        /// Contains just the information that FixedLengthMatrix(De)serializer needs
        /// for its formatting
        /// </summary>
        public interface IColumnDescriptor
        {
            int Length { get; set; }
        }

        public new class ColumnDescriptor : StringMatrixSerializer.ColumnDescriptor, IColumnDescriptor
        {
            int length;

            public int Length
            {
                get
                {
#if DEBUG
                    if (length == 0)
                        throw new InvalidOperationException("Length not initialized for column: " + Name);
#endif
                    return length;
                }
                set
                {
                    if (length != 0 && Formatters.Count > 1)
                        throw new InvalidOperationException("Padding formatter already initialized");
                    else if (Formatters.Count == 1)
                        // If only one formatter, we assume (perhaps incorrectly, be careful) we can clear
                        // and create a new one
                        Formatters.Clear();

                    length = value;
                    assignFormatter(value, true, ' ');
                }
            }

            internal ColumnDescriptor() { }

            void assignFormatter(int length, bool fromLeft, char padChar)
            {
                if (fromLeft)
                    AddFormatter(s => s.PadLeft(length, padChar));
                else
                    AddFormatter(s => s.PadRight(length, padChar));
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="length">total width</param>
            /// <param name="padChar"></param>
            /// <param name="fromLeft">true = pad from left, false = pad from right</param>
            public ColumnDescriptor(int length, bool fromLeft, char padChar)
            {
                this.length = length;
                assignFormatter(length, fromLeft, padChar);
            }


            /// <summary>
            /// Initializes descriptor padding from left
            /// </summary>
            /// <param name="length"></param>
            public ColumnDescriptor(int length) : this(length, true, ' ') { }

            /// <summary>
            /// </summary>
            /// <param name="length"></param>
            /// <param name="fromLeft">true = pad from left, false = pad from right</param>
            public ColumnDescriptor(int length, bool fromLeft) : this(length, fromLeft, ' ') { }
        }


        public new ColumnDescriptor this[string columnName]
        {
            get
            {
                return (ColumnDescriptor)base[columnName];
            }
        }
    }

    public static class FixedLengthMatrixSerializer_Extensions
    {
        /// <summary>
        /// Obscures -count- number of characters on the right side of the string
        /// </summary>
        /// <param name="value"></param>
        /// <param name="count"></param>
        /// <param name="obscureCharacter"></param>
        /// <returns></returns>
        public static string ObscureRight(this string value, int count, char obscureCharacter)
        {
            value = value.Substring(0, value.Length - count);
            value += new string(Enumerable.Repeat(obscureCharacter, count).ToArray());
            return value;
        }

        /// <summary>
        /// Obscures -count- number of characters on the left side of the string
        /// </summary>
        /// <param name="value"></param>
        /// <param name="count"></param>
        /// <param name="obscureCharacter"></param>
        /// <returns></returns>
        public static string ObscureLeft(this string value, int count, char obscureCharacter)
        {
            value = value.Substring(count);
            value = new string(Enumerable.Repeat(obscureCharacter, count).ToArray()) + value;
            return value;
        }
    }

#if UNUSED
    public class FixedLengthMatrixSerializer : IMatrixSerializer
    {
        public FixedLengthMatrixSerializer(StreamWriter output)
        {
            this.output = output;
        }

        StreamWriter output;

        public enum PadLocation
        {
            Right,
            Left
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Refactor this to be reusable across Serializers (and maybe deserializers too?)
        /// Refactor FixedLengthSerializer & CSVSerializer to utilize a base StringSerializer
        /// which in turn use the fancy pluggable formatters
        /// </remarks>
        public class ColumnDescriptor
        {
            public int Length { get; set; }
            public char PadCharacter { get; set; }
            public PadLocation PadLocation { get; set; }
            public string Name { get; set; }
            /// <summary>
            /// How many characters from the right to reach out and obscure sensitive
            /// data underneath.  Negative means reach out from the left instead of the right
            /// </summary>
            public int Obscure { get; set; }
            public char ObscureCharacter { get; set; }

            public string Format(string value)
            {
                if (value.Length > Length)
                    return value.Substring(0, Length);

                // This is an "if" statement and an "assignement" operation combined
                // (unary operation)
                var result = 
                    PadLocation == FixedLengthMatrixSerializer.PadLocation.Left ?
                        value.PadLeft(Length, PadCharacter) :
                        value.PadRight(Length, PadCharacter); 

                if (Obscure > 0)
                {
                    result = result.Substring(0, Length - Obscure);
                    result += new string(Enumerable.Repeat(ObscureCharacter, Obscure).ToArray());
                }
                else if (Obscure < 0)
                {
                    var obscure = Math.Abs(Obscure);
                    result = result.Substring(obscure);
                    result = new string(Enumerable.Repeat(ObscureCharacter, obscure).ToArray()) + result;
                }

                return result;
            }
        }

        ColumnDescriptor[] columns;

        public int ColumnCount
        {
            set 
            {
                columns = new ColumnDescriptor[value];
            }
        }

        public int? Length
        {
            set {  }
        }

        public string NotifierName
        {
            get 
            {
                var fs = output.BaseStream as FileStream;

                if (fs != null) return fs.Name;

                return "";
            }
        }

        public void SetColumn(int columnIndex, string name)
        {
            SetColumn(columnIndex, new ColumnDescriptor() { Name = name });
        }

        public void SetColumn(int columnIndex, ColumnDescriptor descriptor)
        {
            columns[columnIndex] = descriptor;
        }

        public void Append(object[] value)
        {
            string resultLine = "";

            for (int i = 0; i < columns.Length; i++)
            {
                var columnValue = (value[i] ?? "").ToString();

                resultLine += columns[i].Format(columnValue);
            }

            output.WriteLine(resultLine);
        }

        public void Commit()
        {
            output.Flush();
        }

        public void Cleanup()
        {
            output.Close();
        }
    }

#endif

    // In MONO, we don't have ORM available at this stage
#if !MONO && !NETCORE
    public class ExcelMatrixSerializer : MatrixSerializerBase, IMatrixSerializer
    {
        // HDR= header yes/no
        //const string OLEDB4_CONNSTR = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=\"{0}\";Mode=ReadWrite;ReadOnly=false;Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=1\";";
        const string OLEDB4_CONNSTR = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=\"{0}\";Mode=ReadWrite;Extended Properties=\"Excel 8.0;HDR=Yes\";";
        const string OLEDB12_CONNSTR = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=\"{0}\";Mode=ReadWrite;Extended Properties=\"Excel 12.0;HDR=Yes;\";";

        protected override MatrixSerializer.ColumnDescriptor CreateColumnDescriptor()
        {
            return new MatrixSerializer.ColumnDescriptor();
        }

        public ExcelMatrixSerializer(string filename)
        {
            var connectionString = string.Format(OLEDB4_CONNSTR, filename);
            var orm = new ORM(connectionString, Constants.InvariantProvider_OLEDB);
            ORM = orm;
        }

        internal ORM ORM { get; set; }

        public int? Length
        {
            set { throw new NotImplementedException(); }
        }

        public string NotifierName
        {
            get { throw new NotImplementedException(); }
        }

        public void Append(IEnumerable value)
        {
            throw new NotImplementedException();
        }

        public void Commit()
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }
    }
#endif

    // MONODROID doesn't have OleDb access
#if !MONODROID && !NETCORE
    public class ExcelMatrixDeserializer : IMatrixDeserializer, IResettable
    {
        /// <summary>
        /// Number of rows to skip before actually inspecting data
        /// Should be named SkipRows actually
        /// </summary>
        public int HeaderRows { get; set; }

        public bool HasHeader { get; set; }

        OleDbConnection connection;
        OleDbDataReader reader;
        string worksheet;

        /// <summary>
        /// Not used just yet
        /// </summary>
        public string[] Columns { get; set; }

        public ExcelMatrixDeserializer(string filename, string worksheet)
            : this(filename)
        {
            Worksheet = worksheet;
        }

        const string OLEDB4_CONNSTR = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=\"{0}\";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=1\";";
        const string OLEDB12_CONNSTR = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=\"{0}\";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=1\";";

        /// <summary>
        /// If false, uses Microsoft.Jet.OLEDB.4.0, which is fairly universally installed
        /// If true, uses Microsoft.ACE.OLEDB.12.0, which must be installed via office or a distinct MS download
        /// </summary>
        /// <remarks>
        /// Can be downloaded
        ///    here: http://www.microsoft.com/en-us/download/details.aspx?id=23734
        /// or here: http://www.microsoft.com/en-us/download/details.aspx?id=13255
        /// </remarks>
        public static bool IsXLSXSupported = false;

        public ExcelMatrixDeserializer(string filename)
        {
            var connectionString = string.Format(IsXLSXSupported ? OLEDB12_CONNSTR : OLEDB4_CONNSTR, filename);

            connection = new OleDbConnection(connectionString);
            connection.Open();
        }

        public string Worksheet
        {
            set
            {
                worksheet = value;
                var command = new OleDbCommand("select * from [" + value + "$]", connection);

                reader = command.ExecuteReader();
            }
        }

        public int ColumnCount
        {
            get { return reader.FieldCount; }
        }

        public int? Length
        {
            get { return null; }
        }

        public string GetColumnName(int columnIndex)
        {
            return Columns[columnIndex];
        }


        bool initialized = false;

        public void Initialize()
        {
            if (initialized)
                return;

            initialized = true;

            for (int i = 0; i < HeaderRows; i++)
            {
                reader.Read();
            }

            if (HasHeader)
            {
                reader.Read();
                object[] headerColumns = new object[reader.FieldCount];
                reader.GetValues(headerColumns);
                Columns = headerColumns.Cast<string>().ToArray();
            }
        }

        public IEnumerable<IEnumerable<object>> List
        {
            get
            {
                Initialize();

                foreach (DbDataRecord row in reader)
                {
                    object[] rowValues = new object[row.FieldCount];
                    row.GetValues(rowValues);
                    yield return rowValues;
                }
            }
        }

        public void Cleanup()
        {
            connection.Close();
        }

        public void Reset()
        {
            if (Resetting != null)
                Resetting(this);

            Worksheet = worksheet;
        }

        public event Action<IResettable> Resetting;
    }
#endif // MONODROID



    // TODO: Put this in its own file/area to be picked up by Mono Fact.Apprentice.Core.DAL
#if !MONO && !NETCORE
    public class SQLSerializer<T> : IMatrixSerializer
        where T : new()
    {
        string[] columns;
        Fact.Apprentice.Core.DAL.ORM orm = Fact.Apprentice.Core.DAL.ORM.Default;

        #region IMatrixSerializer Members

        public int ColumnCount
        {
            set { columns = new string[value]; }
            protected get { return columns.Length; }
        }

        public int? Length
        {
            protected get;
            set;
        }

        public string NotifierName
        {
            get { return "N/A"; }
        }

        public void SetColumnName(int columnIndex, string name)
        {
            columns[columnIndex] = name;
        }

        TransactionScope ts = null;

        public void Append(IEnumerable value)
        {
            if (ts == null)
                ts = new TransactionScope();

            // Looks like this method has a missing piece...

        }

        public void Commit()
        {
            ts.Complete();
        }

        public void Close()
        {
            ts.Dispose();
        }

        #endregion
    }
#endif

    public abstract class MatrixSerializerBase
    {
        // TODO: Perhaps use this if we're really interested in re-using ColumnDescriptor instances.  
        // Could be more trouble than it's worth.  Dormant for now
        protected List<string> columnNames;

        List<MatrixSerializer.ColumnDescriptor> columns;

        protected List<MatrixSerializer.ColumnDescriptor> Columns
        {
            get
            {
                if (columns == null)
                    columns = new List<MatrixSerializer.ColumnDescriptor>();

                return columns;
            }
        }

        public int ColumnCount
        {
            set
            {
                if (columns == null)
                    columns = new List<MatrixSerializer.ColumnDescriptor>(value);
                else
                    expandColumns(value, null);
            }
            get
            {
                return columns == null ? 0 : columns.Count;
            }
        }

        /// <summary>
        /// Assign column descriptor value to a particular column index, expanding the columns
        /// list with nulls if necessary
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <param name="value">column descriptor to assign at index, or null to merely expand index</param>
        protected void expandColumns(int columnIndex, MatrixSerializer.ColumnDescriptor value)
        {
            if (columns == null)
                columns = new List<MatrixSerializer.ColumnDescriptor>(columnIndex + 1);

            // if this index is past the existing size of column count
            if (columns.Count <= columnIndex)
            {
                // expand the list size
                for (int i = columns.Count; i < columnIndex; i++)
                    columns.Add(null);

                columns.Add(value);
            }
            else if (value != null)
                // if index is within existing column size, overwrite if not null
                columns[columnIndex] = value;
        }

        public virtual void SetColumnName(int columnIndex, string name)
        {
            expandColumns(columnIndex, null);

            if (columns[columnIndex] == null)
                columns[columnIndex] = CreateColumnDescriptor();

            columns[columnIndex].Name = name;
            //return columns[columnIndex];
        }

        protected abstract MatrixSerializer.ColumnDescriptor CreateColumnDescriptor();


        /// <summary>
        /// Reorders the serializer's column names to match the specified columnNames order
        /// </summary>
        /// <param name="columnNames"></param>
        /// <remarks>Existing column count must match columnName count</remarks>
        public void AlignColumnNames(IEnumerable<string> columnNames)
        {
            var newColumns = new List<MatrixSerializer.ColumnDescriptor>(columns.Count);

            foreach (var name in columnNames)
                newColumns.Add(columns.First(x => x.Name == name));

            columns = newColumns;
        }


        /// <summary>
        /// Zero based column setter/accessor
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public MatrixSerializer.ColumnDescriptor this[int columnIndex]
        {
            get
            {
                return columns[columnIndex];
            }
            set
            {
                expandColumns(columnIndex, value);
            }
        }


        public MatrixSerializer.ColumnDescriptor this[string name]
        {
            get
            {
                return columns.First(x => x.Name == name);
            }
        }
    }

    public class MatrixSerializer
    {
        IMatrixDeserializer input;
        IMatrixSerializer output;
        event Action<string, System.Diagnostics.TraceEventType> notify;

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// TODO: May be better to track name in parallel with actual ColumnDescriptor, so that we may re-use column descriptors
        /// for different column names
        /// </remarks>
        public class ColumnDescriptor
        {
            /// <summary>
            /// Column header name
            /// </summary>
            public string Name { get; set; }
        }

        /// <summary>
        /// This descriptor has typing information associated with it
        /// </summary>
        public interface ITypedColumnDescriptor
        {
            Type Type { get; set; }
        }

        /// <summary>
        /// How many rows are processed per notification
        /// </summary>
        public int NotifyGranularity { get; set; }

        /// <summary>
        /// Overrides the length reported by input
        /// </summary>
        public int? Length { protected get; set; }

        public MatrixSerializer(IMatrixDeserializer input, IMatrixSerializer output)
        {
            NotifyGranularity = 300;
            this.input = input;
            this.output = output;
        }

        public void Serialize()
        {
            try
            {
                output.Length = Length.HasValue ? Length.Value : input.Length;

                int row = 0;

                output.ColumnCount = input.ColumnCount;

                for (int i = 0; i < input.ColumnCount; i++)
                {
                    output.SetColumnName(i, input.GetColumnName(i));
                }

                foreach (var item in input.List)
                {
                    output.Append(item);

                    if (row % NotifyGranularity == 0) if (notify != null) notify("At row: " + row, System.Diagnostics.TraceEventType.Verbose);

                    row++;
                }

                if (notify != null) notify(row + " rows written into: " + output.NotifierName, System.Diagnostics.TraceEventType.Verbose);

                output.Commit();
            }
            finally
            {
                input.Cleanup();
                output.Close();
            }
        }
    }



    public interface IMatrixFixedLengthColumnHandler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Allowed to be null</param>
        /// <param name="length"></param>
        void AddColumn(string name, int length);
    }


    /// <summary>
    /// The inverse of the ColumnBuilder, this exists to assists IMatrixDeserializers
    /// to reconsitute entities from the raw indexed columns
    /// </summary>
    /// <remarks>At some point, perhaps marry this to ORM's version</remarks>
    public abstract class EntityBuilder
    {
        public abstract object RowToEntity(IEnumerable row);
    }


    public class PropertyEntityBuilder : EntityBuilder
    {
        Type type;

        public Type Type { get { return type; } }

        IEnumerable<PropertyInfo> sortedProperties;

        /// <summary>
        /// Defaults to false.  If columns are specified in CSV header or on manual "property order" but cannot be found
        /// in the matching entity, default behavior is to throw an exception.  If this value is set to true,
        /// then these unmatched properties are simply ignored.
        /// </summary>
        public bool IgnoreMissingProperties { get; set; }

        /// <summary>
        /// Type t must align to a specific header in the CSV
        /// </summary>
        /// <param name="t"></param>
        public PropertyEntityBuilder(IMatrixDeserializer deserializer, Type t)
        {
            type = t;
            var propertyOrder = deserializer.GetColumnNames();
            sortedProperties = InitializeProperties(propertyOrder);
        }

        /// <summary>
        /// Type t aligns ordinally with the CSV contents, CSV header if present is
        /// ignored
        /// </summary>
        /// <param name="t"></param>
        /// <param name="propertyOrder">Specify the properties in type t, in the 
        /// order matching appearance of data in CSV row</param>
        public PropertyEntityBuilder(Type t, params string[] propertyOrder)
        {
            type = t;
            sortedProperties = InitializeProperties(propertyOrder);
        }

        /// <summary>
        /// Gathers and sorts properties based on propertyOrder
        /// </summary>
        /// <returns></returns>
        IEnumerable<PropertyInfo> InitializeProperties(IEnumerable<string> propertyOrder)
        {
            var properties = (from n in type.GetTypeInfo().DeclaredProperties
                              let columnNameAttribute = n.GetCustomAttribute<AliasAttribute>()
                              let columnName = columnNameAttribute != null ? columnNameAttribute.Name : n.Name
                              where n.CanWrite
                              select new { n, columnName }).ToArray();

            foreach (var column in propertyOrder)
            {
                var property = properties.SingleOrDefault(x =>
                    x.columnName.Equals(column, StringComparison.CurrentCultureIgnoreCase));

                if (property == null)
                {
                    if (!IgnoreMissingProperties)
                        throw new KeyNotFoundException("Cannot match column: " + column);

                    yield return null;
                }
                else
                    yield return property.n;
            }
        }

        public override object RowToEntity(IEnumerable row)
        {
            var rowEnumerator = row.GetEnumerator();
            var entity = Activator.CreateInstance(type);

            // defer enumeration until this point so that we have a chance to initialize
            // the IgnoreMissingProperties flag
            sortedProperties = sortedProperties.AsArray();

            foreach (var property in sortedProperties)
            {
                // if we get an error here, it's likely because we've manually specified too many sorted columns
                rowEnumerator.MoveNext();

                if (property == null)
                    continue;

                var rawValue = rowEnumerator.Current;

                if (rawValue == DBNull.Value || rawValue == null)
                {
                    property.SetValue(entity, null, null);
                }
                else
                {

                    var _type = property.PropertyType;
                    var __type = Nullable.GetUnderlyingType(_type);

                    if (__type != null) _type = null;

                    var value = Convert.ChangeType(rawValue, _type);
                    property.SetValue(entity, value, null);
                }
            }

            return entity;
        }
    }


    public class PropertyEntityBuilder<T> : PropertyEntityBuilder
    {
        public PropertyEntityBuilder(IMatrixDeserializer deserializer)
            : base(deserializer, typeof(T))
        { }

        new public T RowToEntity(IEnumerable row)
        {
            return (T)base.RowToEntity(row);
        }
    }

    /// <summary>
    /// Exists to assist an IMatrixSerializer's building and populating of columns.  Architecture
    /// leans towards a hard type (class) coming in to be converted to the matrix serializer's desired
    /// object[] format
    /// </summary>
    /// <remarks>
    /// Decoupling of Column creation from MatrixSerializer into ColumnBuilder still
    /// clumsy
    /// </remarks>
    public abstract class ColumnBuilder
    {
        public IMatrixSerializer Serializer { get; protected set; }

        public void Initialize() { }

        /// <summary>
        /// Converts a some format (architecture leans towards native entity/join format)
        /// to matrix serializer desired format (ordered list of column data)
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public abstract IEnumerable EntityToRow(object entity);
    }

    public class PropertyColumnBuilder : ColumnBuilder
    {
        Type type;
        Func<PropertyInfo, bool> propFilter;

        public PropertyColumnBuilder(IMatrixSerializerColumns serializer, Type type,
            Func<PropertyInfo, bool> propFilter = null)
        {
            this.Serializer = serializer;
            this.type = type;

            this.propFilter = propFilter = propFilter ?? (x => true);

            int i = 0;
            foreach (var prop in type.GetTypeInfo().DeclaredProperties.Where(propFilter))
            {
                serializer.SetColumnName(i, prop.Name);

                // +++
                // TODO: hardwiring this stuff here isn't quite right
                var c = serializer[i] as FixedLengthMatrixSerializer.IColumnDescriptor;
                if (c != null)
                {
                    var lengthAttrib = prop.CustomAttributes.OfType<LengthAttribute>().FirstOrDefault();
                    if (lengthAttrib != null)
                    {
                        c.Length = lengthAttrib.Max;
                    }
                }

                var c2 = serializer[i] as MatrixSerializer.ITypedColumnDescriptor;
                if (c2 != null)
                {
                    c2.Type = prop.PropertyType;
                }
                // ---

                i++;
            }
        }



        public override IEnumerable EntityToRow(object entity)
        {
            // NOTE: be careful with this code, this particular variant of
            // GoLightweight produces items in the same order every time,
            // but others may not
#if FEATURE_WALKER
            foreach (var prop in Walker.GoLightweight(type, propFilter))
#endif
            foreach (var prop in type.GetTypeInfo().DeclaredProperties.Where(propFilter) )
            {
                yield return prop.GetValue(entity, null);
            }
        }
    }


    public static class IMatrixSerializer_extensions
    {
        /// <summary>
        /// Extension helper to acquire enumeration of column names in order from
        /// the deserializer
        /// </summary>
        /// <param name="This"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetColumnNames(this IMatrixDeserializer This)
        {
            for (int i = 0; i < This.ColumnCount; i++)
                yield return This.GetColumnName(i);
        }

        /// <summary>
        /// Copy rows from deserializer to serializer, assumes columns have already been prepared
        /// </summary>
        /// <param name="This"></param>
        /// <param name="output"></param>
        public static void CopyTo(this IMatrixDeserializer This, IMatrixSerializer output)
        {
            foreach (var row in This.List)
                output.Append(row);

            This.Cleanup();
            output.Close();
        }

        public static void SetColumns(this IMatrixSerializer This, params string[] columns)
        {
            This.ColumnCount = columns.Length;
            for (int i = 0; i < columns.Length; i++)
            {
                This.SetColumnName(i, columns[i]);
            }
        }


        /// <summary>
        /// Copy columns from deserializer to serializer.  Includes order, names and types
        /// if both sides support types
        /// </summary>
        /// <param name="deserializer"></param>
        /// <param name="serializer">the destination for columns to be copied to</param>
        /// <remarks>
        /// A bit kludgey, TSource is assembled of interfaces here,
        /// but destination is a "fixed" interface.
        /// </remarks>
        public static void CopyColumns<TSource, TDest>(this TSource deserializer, TDest serializer)
            where TSource : IMatrixColumnAccessor, IMatrixDeserializer
            where TDest : IMatrixColumnAccessor, IMatrixSerializer
        {
            for (int i = 0; i < deserializer.ColumnCount; i++)
            {
                var column = deserializer[i];
                serializer.SetColumnName(i, column.Name);// setting the column name also adds it.  a little kludgey but won't break

                // FIX: should use a pluggable model here of some kind
                var sColumn = serializer[i] as MatrixSerializer.ITypedColumnDescriptor;
                var dColumn = column as MatrixSerializer.ITypedColumnDescriptor;

                if (sColumn != null && dColumn != null) sColumn.Type = dColumn.Type;
            }
        }


        /// <summary>
        /// Utilizes PropertyEntityBuilder.  Presumes no header is in CSV.  Defaults to ignore missing properties
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="This"></param>
        /// <param name="columnOrder">column names matching the properties in T</param>
        /// <returns></returns>
        public static IEnumerable<T> List<T>(this IMatrixDeserializer This, params string[] columnOrder)
            where T : new()
        {
            var eb = new PropertyEntityBuilder(typeof(T), columnOrder) { IgnoreMissingProperties = true };
            foreach (var row in This.List)
                yield return (T)eb.RowToEntity(row);
        }


        /// <summary>
        /// Utilizes PropertyEntityBuilder.  Presumes header is in CSV
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="This"></param>
        /// <param name="ignoreMissingProperties"></param>
        /// <returns></returns>
        public static IEnumerable<T> List<T>(this IMatrixDeserializer This, bool ignoreMissingProperties)
            where T : new()
        {
            var eb = new PropertyEntityBuilder(This, typeof(T));
            eb.IgnoreMissingProperties = ignoreMissingProperties;
            foreach (var row in This.List)
                yield return (T)eb.RowToEntity(row);
        }



        /// <summary>
        /// Utilizes PropertyEntityBuilder.  Presumes header is in CSV.  Does not ignore missing properties
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="This"></param>
        /// <returns></returns>
        public static IEnumerable<T> List<T>(this IMatrixDeserializer This)
            where T : new()
        {
            return List<T>(This, false);
        }


        /// <summary>
        /// Uses PropertyColumnBuilder to determine column information
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializer"></param>
        /// <param name="e"></param>
        public static void AppendAndClose<T>(this StringMatrixSerializer serializer, IEnumerable<T> e)
        {
            var cb = new PropertyColumnBuilder(serializer, typeof(T));
            cb.AppendAndClose(e, null);
        }
    }


    public static class EntityBuilder_Extensions
    {/*
        public static void List(this EntityBuilder entityBuilder)
        {
        }*/
    }


    public static class ColumnBuilder_Extensions
    {
        /// <summary>
        /// Take an entity, convert it to the proper IMatrixSerializer row structure, and append
        /// it to the IMatrixSerializer
        /// </summary>
        /// <param name="columnBuilder"></param>
        /// <param name="entity"></param>
        public static void Append(this ColumnBuilder columnBuilder, object entity)
        {
            columnBuilder.Serializer.Append(columnBuilder.EntityToRow(entity));
        }


        /// <summary>
        /// Take an entity list, converts it per entity to the proper IMatrixSerializer row structure, and append
        /// it to the IMatrixSerializer
        /// </summary>
        /// <param name="columnBuilder"></param>
        /// <param name="entity"></param>
        /// <param name="notify">Notifies which row # is being processed</param>
        public static void Append(this ColumnBuilder columnBuilder, IEnumerable entities, Action<int> notify)
        {
            int count = 0;
            var serializer = columnBuilder.Serializer;
            foreach (var entity in entities)
            {
                serializer.Append(columnBuilder.EntityToRow(entity));
                if (notify != null)
                    notify(count++);
            }
        }


        /// <summary>
        /// Take an entity list, converts it per entity to the proper IMatrixSerializer row structure, and append
        /// it to the IMatrixSerializer.  Then closes out the underlying IMatrixSerializer
        /// </summary>
        /// <param name="columnBuilder"></param>
        /// <param name="entities"></param>
        /// <param name="notify">Notifies which row # is being processed</param>
        public static void AppendAndClose(this ColumnBuilder columnBuilder, IEnumerable entities, Action<int> notify)
        {
            Append(columnBuilder, entities, notify);
            columnBuilder.Serializer.Commit();
            columnBuilder.Serializer.Close();
        }
    }

}