using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using LumenWorks.Framework.IO.Csv;
using System.Text;

namespace Requests
{
    class UI : Form
    {
        private ListBox LoadedFilesList;
        private DataGridView OutputDataGridView;
        private ListBox CommandBox;
        private GroupBox IdentifierGroupBox;
        private DataTable dt = new DataTable();
        private DomainUpDown identifierDomainUpDown;
        private GroupBox BoundaryGroupBox;
        private NumericUpDown lowerBound;
        private NumericUpDown upperBound;
        // Fired for DT adjustment when identifier is changed
        private Action<int> indexChangedAction = (i) => { };
        private Button SaveButton;

        // Fired for DT adjustment when boundaries are chandged
        private Action<float, float> boundsChanged = (lower, upper) => { };

        public UI()
        {
            InitializeComponent();
            // Initialization of main datatable columns
            dt.Columns.Add(Request.CLIENTID, typeof(int));
            dt.Columns.Add(Request.REQUESTID, typeof(int));
            dt.Columns.Add(Request.NAME, typeof(string));
            dt.Columns.Add(Request.QUANTITY, typeof(int));
            dt.Columns.Add(Request.PRICE, typeof(float));
            identifierDomainUpDown.SelectedItemChanged += (sender, e) =>
            {
                indexChangedAction((int)identifierDomainUpDown.SelectedItem);
            };
            upperBound.ValueChanged += (sender, e) =>
            {
                lowerBound.Maximum = upperBound.Value;
                boundsChanged((float)lowerBound.Value, (float)upperBound.Value);
            };
            lowerBound.ValueChanged += (sender, e) =>
            {
                upperBound.Minimum = lowerBound.Value;
                boundsChanged((float)lowerBound.Value, (float)upperBound.Value);
            };
            // Little visual effect for dragging files over the window
            DragEnter += (sender, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    e.Effect = DragDropEffects.Copy;
            };
            DragDrop += (sender, e) =>
            {
                string[] extensions = new string[]{
                    ".csv",
                    ".xml",
                    ".json"
                };
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    if (!extensions.Contains(Path.GetExtension(file)))
                    {
                        MessageBox.Show($"File {Path.GetFileName(file)} has wrong extention!\nTry .csv, .json or .xml.");
                    }
                    else
                    {
                        // I prefer without extention, if disputed you are free to change it
                        object filename = Path.GetFileNameWithoutExtension(file);
                        LoadedFilesList.Items.Add(filename);
                        LoadedFilesList.SelectedIndexChanged += (selecter, ev) =>
                        {
                            if (LoadedFilesList.SelectedItem == filename)
                            {
                                var array = new RequestList();
                                switch (Path.GetExtension(file))
                                {
                                    case ".csv":
                                        {
                                            var requestMap = new Dictionary<string, int>();

                                            using (CsvReader r = new CsvReader(new StreamReader(file), true))
                                            {
                                                int fieldCount = r.FieldCount;
                                                string[] headers = r.GetFieldHeaders();

                                                for(int i = 0; i < fieldCount; i++)
                                                {
                                                    requestMap[headers[i]] = i;
                                                }

                                                while (r.ReadNextRecord())
                                                {
                                                    try
                                                    {
                                                        var request = new Request
                                                        {
                                                            // For some reason labels are different for .csv files
                                                            ClientId = int.Parse(r[requestMap["Client_Id"]]),
                                                            RequestId = int.Parse(r[requestMap["Request_id"]]),
                                                            Name = r[requestMap["Name"]],
                                                            Price = float.Parse(r[requestMap["Price"]]),
                                                            Quantity = int.Parse(r[requestMap["Quantity"]])
                                                        };

                                                        array.Requests.Add(request);
                                                    }
                                                    catch (Exception)
                                                    {
                                                        // Just ignore the line
                                                    }
                                                    
                                                }
                                            }
                                            break;
                                        }
                                    case ".json":
                                        {
                                            using (StreamReader r = new StreamReader(file))
                                            {
                                                array = new JavaScriptSerializer().Deserialize<RequestList>(r.ReadToEnd());
                                            }
                                            break;
                                        }
                                    case ".xml":
                                        {
                                            using (StreamReader r = new StreamReader(file))
                                            {
                                                array = (RequestList)new XmlSerializer(typeof(RequestList), new XmlRootAttribute("requests")).Deserialize(r);
                                            }
                                            break;
                                        }
                                }
                                dt.Rows.Clear();
                                array.RemoveIncompletes();
                                if (array.Requests.Count > 0)
                                {
                                    CommandBox.Enabled = true;
                                }
                                foreach (var item in array.Requests)
                                {
                                    dt.Rows.Add(new object[] { item.ClientId, item.RequestId, item.Name, item.Quantity, item.Price });
                                }
                                OutputDataGridView.DataSource = dt;
                            }
                        };
                    }
                    
                }
            };
            SaveButton.Click += (sender, e) =>
            {
                var sb = new StringBuilder();

                var headers = OutputDataGridView.Columns.Cast<DataGridViewColumn>();
                sb.AppendLine(string.Join(",", headers.Select(column => "\"" + column.HeaderText + "\"").ToArray()));

                foreach (DataGridViewRow row in OutputDataGridView.Rows)
                {
                    var cells = row.Cells.Cast<DataGridViewCell>();
                    sb.AppendLine(string.Join(",", cells.Select(cell => "\"" + cell.Value + "\"").ToArray()));
                }
                File.WriteAllText("raport.csv", sb.ToString(), Encoding.UTF8);
            };
            // Set all commands
            foreach (var item in new Dictionary<object, Action>()
            {
                { "Ilość zamówień", () =>{
                    int count = dt.AsEnumerable().Select(x => x.Field<int>(Request.REQUESTID)).Distinct().Count();
                    var table = new DataTable();
                    table.Columns.Add("Ilość zamówień", typeof(int));
                    table.Rows.Add(new object[] { count });
                    OutputDataGridView.DataSource = table;
                }},
                { "Ilość zamówień dla klienta o wskazanym identyfikatorze", () =>{
                    indexChangedAction = (i) =>
                    {
                        var count = dt.AsEnumerable().Where(x => (int)x[Request.CLIENTID] == i)
                            .Select(x => x.Field<int>(Request.REQUESTID)).Distinct().Count();
                        var countDT = new DataTable();
                        countDT.Columns.Add($"Ilość zamówień klienta {i}", typeof(int));
                        countDT.Rows.Add(new object[] { count });
                        OutputDataGridView.DataSource = countDT;
                    };
                    UpdateClientIdentifiers();
                } },
                { "Łączna kwota zamówień", () =>
                {
                    var total = dt.AsEnumerable().Select(x => x.Field<float>(Request.PRICE)).Aggregate((a, b) => a + b);
                    var totalDT = new DataTable();
                    totalDT.Columns.Add("Łączna kwota zamówień", typeof(float));
                    totalDT.Rows.Add(new object[] { total });
                    OutputDataGridView.DataSource = totalDT;
                } },
                { "Łączna kwota zamówień dla klienta o wskazanym identyfikatorze", () =>
                {
                    indexChangedAction = (i) =>
                    {
                        var total = dt.AsEnumerable().Where(x => (int)x[Request.CLIENTID] == i)
                            .Select(x => x.Field<float>(Request.PRICE)).Aggregate((a, b) => a + b);
                        var totalDT = new DataTable();
                        totalDT.Columns.Add($"Łączna kwota zamówień dla klienta {i}", typeof(int));
                        totalDT.Rows.Add(new object[] { total });
                        OutputDataGridView.DataSource = totalDT;
                    };
                    UpdateClientIdentifiers();
                } },
                { "Lista wszystkich zamówień", () =>
                {
                    OutputDataGridView.DataSource = dt;
                } },
                { "Lista wszystkich zamówień dla klienta o wskazanym identyfikatorze", () =>
                {
                    indexChangedAction = (i) =>
                    {
                        var rows = dt.AsEnumerable().Where(x => x.Field<int>(Request.CLIENTID) == i).ToList();
                        DataTable table = dt.Copy();
                        table.Rows.Clear();
                        rows.ForEach(row => table.ImportRow(row));
                        OutputDataGridView.DataSource = table;
                    };
                    UpdateClientIdentifiers();
                } },
                { "Średnia wartość zamówień", () =>
                {
                    var sum = dt.AsEnumerable().Select(x => x.Field<float>(Request.PRICE)).Aggregate((a, b) => a + b);
                    var table = new DataTable();
                    table.Columns.Add("Średnia wartość zamówień");
                    table.Rows.Add(new object[]{sum / dt.Rows.Count});
                    OutputDataGridView.DataSource = table;
                } },
                { "Średnia wartość zamówień dla klienta o zadanym identyfikatorze", () =>
                {
                    indexChangedAction = (i) =>
                    {
                        var sum = dt.AsEnumerable().Where(x => x.Field<int>(Request.CLIENTID) == i)
                            .Select(x => x.Field<float>(Request.PRICE)).Aggregate((a, b) => a + b);
                        var table = new DataTable();
                        table.Columns.Add($"Średnia wartość zamówień dla {i}");
                        table.Rows.Add(new object[]{sum / dt.Rows.Count});
                        OutputDataGridView.DataSource = table;
                    };
                    UpdateClientIdentifiers();
                } },
                { "Ilość zamówień pogrupowanych po nazwie", () => {
                    var names = dt.AsEnumerable().Select(x => x.Field<string>(Request.NAME)).Distinct().ToList();
                    var table = new DataTable();
                    table.Columns.Add("nazwa", typeof(string));
                    table.Columns.Add("ilość", typeof(int));
                    foreach (var name in names)
                    {
                        var count = dt.AsEnumerable().Where(x => x.Field<string>(Request.NAME) == name).Count();
                        table.Rows.Add(new object[] {name, count });
                    }
                    OutputDataGridView.DataSource = table;
                } },
                { "Ilość zamówień pogrupowanych po nazwie dla klienta o wskazanym identyfikatorze", () =>
                {
                    indexChangedAction = (i) =>
                    {
                        var names = dt.AsEnumerable().Where(x => x.Field<int>(Request.CLIENTID) == i)
                            .Select(x => x.Field<string>(Request.NAME)).Distinct().ToList();
                        var table = new DataTable();
                        table.Columns.Add("nazwa", typeof(string));
                        table.Columns.Add("ilość", typeof(int));
                        foreach (var name in names)
                        {
                            var count = dt.AsEnumerable().Where(x => 
                                x.Field<int>(Request.CLIENTID) == i 
                                && x.Field<string>(Request.NAME) == name).Count();
                            table.Rows.Add(new object[] {name, count });
                        }
                        OutputDataGridView.DataSource = table;
                    };
                    UpdateClientIdentifiers();
                } },
                { "Zamówienia w podanym przedziale cenowym", () =>
                {
                    upperBound.Enabled = lowerBound.Enabled = true;
                    boundsChanged = (lower, upper) =>
                    {
                        var rows = dt.AsEnumerable().Where(x => x.Field<float>(Request.PRICE) >= lower
                                                            && x.Field<float>(Request.PRICE) <= upper).ToList();
                        DataTable table = dt.Copy();
                        table.Rows.Clear();
                        rows.ForEach(row => table.ImportRow(row));
                        OutputDataGridView.DataSource = table;
                    };
                    boundsChanged((float)lowerBound.Value, (float)upperBound.Value);
                } }
            })
            {
                CommandBox.Items.Add(item.Key);
                CommandBox.SelectedIndexChanged += (sender, e) =>
                {
                    // The key is the command name, the value is the action fired when command is choosen
                    if(CommandBox.SelectedItem == item.Key)
                    {
                        // Both identifier and boundary controls are disabled and can only be enabled by certain commands
                        identifierDomainUpDown.Enabled = false;
                        upperBound.Enabled = lowerBound.Enabled = false;
                        item.Value();
                    }
                };
            }
        }

        private void InitializeComponent()
        {
            this.LoadedFilesList = new System.Windows.Forms.ListBox();
            this.OutputDataGridView = new System.Windows.Forms.DataGridView();
            this.CommandBox = new System.Windows.Forms.ListBox();
            this.IdentifierGroupBox = new System.Windows.Forms.GroupBox();
            this.identifierDomainUpDown = new System.Windows.Forms.DomainUpDown();
            this.BoundaryGroupBox = new System.Windows.Forms.GroupBox();
            this.lowerBound = new System.Windows.Forms.NumericUpDown();
            this.upperBound = new System.Windows.Forms.NumericUpDown();
            this.SaveButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.OutputDataGridView)).BeginInit();
            this.IdentifierGroupBox.SuspendLayout();
            this.BoundaryGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lowerBound)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.upperBound)).BeginInit();
            this.SuspendLayout();
            // 
            // LoadedFilesList
            // 
            this.LoadedFilesList.FormattingEnabled = true;
            this.LoadedFilesList.Location = new System.Drawing.Point(12, 12);
            this.LoadedFilesList.Name = "LoadedFilesList";
            this.LoadedFilesList.Size = new System.Drawing.Size(217, 160);
            this.LoadedFilesList.TabIndex = 1;
            // 
            // OutputDataGridView
            // 
            this.OutputDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.OutputDataGridView.Location = new System.Drawing.Point(12, 178);
            this.OutputDataGridView.Name = "OutputDataGridView";
            this.OutputDataGridView.Size = new System.Drawing.Size(877, 481);
            this.OutputDataGridView.TabIndex = 2;
            // 
            // CommandBox
            // 
            this.CommandBox.Enabled = false;
            this.CommandBox.FormattingEnabled = true;
            this.CommandBox.Location = new System.Drawing.Point(235, 12);
            this.CommandBox.Name = "CommandBox";
            this.CommandBox.Size = new System.Drawing.Size(417, 160);
            this.CommandBox.TabIndex = 3;
            // 
            // IdentifierGroupBox
            // 
            this.IdentifierGroupBox.Controls.Add(this.identifierDomainUpDown);
            this.IdentifierGroupBox.Location = new System.Drawing.Point(658, 12);
            this.IdentifierGroupBox.Name = "IdentifierGroupBox";
            this.IdentifierGroupBox.Size = new System.Drawing.Size(134, 55);
            this.IdentifierGroupBox.TabIndex = 4;
            this.IdentifierGroupBox.TabStop = false;
            this.IdentifierGroupBox.Text = "Wybrany identyfikator";
            // 
            // identifierDomainUpDown
            // 
            this.identifierDomainUpDown.Enabled = false;
            this.identifierDomainUpDown.Location = new System.Drawing.Point(7, 20);
            this.identifierDomainUpDown.Name = "identifierDomainUpDown";
            this.identifierDomainUpDown.Size = new System.Drawing.Size(120, 20);
            this.identifierDomainUpDown.TabIndex = 0;
            // 
            // BoundaryGroupBox
            // 
            this.BoundaryGroupBox.Controls.Add(this.lowerBound);
            this.BoundaryGroupBox.Controls.Add(this.upperBound);
            this.BoundaryGroupBox.Location = new System.Drawing.Point(658, 73);
            this.BoundaryGroupBox.Name = "BoundaryGroupBox";
            this.BoundaryGroupBox.Size = new System.Drawing.Size(137, 77);
            this.BoundaryGroupBox.TabIndex = 5;
            this.BoundaryGroupBox.TabStop = false;
            this.BoundaryGroupBox.Text = "Zakres";
            // 
            // lowerBound
            // 
            this.lowerBound.DecimalPlaces = 2;
            this.lowerBound.Enabled = false;
            this.lowerBound.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.lowerBound.Location = new System.Drawing.Point(7, 46);
            this.lowerBound.Name = "lowerBound";
            this.lowerBound.Size = new System.Drawing.Size(120, 20);
            this.lowerBound.TabIndex = 1;
            this.lowerBound.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // upperBound
            // 
            this.upperBound.DecimalPlaces = 2;
            this.upperBound.Enabled = false;
            this.upperBound.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.upperBound.Location = new System.Drawing.Point(7, 20);
            this.upperBound.Name = "upperBound";
            this.upperBound.Size = new System.Drawing.Size(120, 20);
            this.upperBound.TabIndex = 0;
            this.upperBound.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // SaveButton
            // 
            this.SaveButton.Location = new System.Drawing.Point(798, 12);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(91, 40);
            this.SaveButton.TabIndex = 6;
            this.SaveButton.Text = "ZAPISZ";
            this.SaveButton.UseVisualStyleBackColor = true;
            // 
            // UI
            // 
            this.AllowDrop = true;
            this.ClientSize = new System.Drawing.Size(901, 671);
            this.Controls.Add(this.SaveButton);
            this.Controls.Add(this.BoundaryGroupBox);
            this.Controls.Add(this.IdentifierGroupBox);
            this.Controls.Add(this.CommandBox);
            this.Controls.Add(this.OutputDataGridView);
            this.Controls.Add(this.LoadedFilesList);
            this.Name = "UI";
            this.Text = "Data Parser";
            ((System.ComponentModel.ISupportInitialize)(this.OutputDataGridView)).EndInit();
            this.IdentifierGroupBox.ResumeLayout(false);
            this.BoundaryGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.lowerBound)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.upperBound)).EndInit();
            this.ResumeLayout(false);

        }

        /// <summary>
        /// Enables and fills identifier domain control with all the client's identifier numbers
        /// </summary>
        private void UpdateClientIdentifiers()
        {
            identifierDomainUpDown.Enabled = true;
            var clients = dt.AsEnumerable().Select(x => x.Field<int>(Request.CLIENTID)).Distinct().ToList();
            identifierDomainUpDown.Items.Clear();
            identifierDomainUpDown.Items.AddRange(clients);
            identifierDomainUpDown.SelectedIndex = 0;
            indexChangedAction((int)identifierDomainUpDown.SelectedItem);
        }
    }
}
