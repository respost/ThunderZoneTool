using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace 雷霆合区工具
{
    public partial class Form1 : Form
    {
        //代理
        private delegate void SetPos(string info);
        //数据库操作对象
        private MysqlBase Db;
        //线程数量
        private int threadNum = 5;
        private List<Object> okTaskList = new List<Object>();
        public Form1()
        {
            //加载嵌入资源
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            InitializeComponent();
            ReadAppConfig();
        }

        private void ReadAppConfig()
        {
            this.txtDbHost.Text = AppSettings.GetValue("DbHost");
            this.txtDbPort.Text = AppSettings.GetValue("DbPort");
            this.txtDbUser.Text = AppSettings.GetValue("DbUser");
            this.txtDbPassword.Text = AppSettings.GetValue("DbPassword");
            this.txtDbName.Text = AppSettings.GetValue("DbName");
        }
        /// <summary>
        /// 加载嵌入资源中的全部dll文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string dllName = args.Name.Contains(",") ? args.Name.Substring(0, args.Name.IndexOf(',')) : args.Name.Replace(".dll", "");
            dllName = dllName.Replace(".", "_");
            if (dllName.EndsWith("_resources")) return null;
            System.Resources.ResourceManager rm = new System.Resources.ResourceManager(GetType().Namespace + ".Properties.Resources", System.Reflection.Assembly.GetExecutingAssembly());
            byte[] bytes = (byte[])rm.GetObject(dllName);
            return System.Reflection.Assembly.Load(bytes);
        }
        /// <summary>
        /// 输出信息
        /// </summary>
        /// <param name="msg"></param>
        private void printLog(string msg)
        {
            if (this.InvokeRequired)
            {
                SetPos setpos = new SetPos(printLog);
                this.Invoke(setpos, new object[] { msg });
            }
            else
            {
                //this.txtLog.AppendText(string.Format("[{0}]{1}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msg));
                this.txtLog.AppendText(string.Format("{0}\r\n", msg));
            }
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            //数据库IP
            string txtDbHost = this.txtDbHost.Text.Trim();
            if (txtDbHost == string.Empty)
            {
                txtDbHost = "127.0.0.1";
            }
            //数据库端口
            string txtDbPort = this.txtDbPort.Text.Trim();
            if (txtDbPort == string.Empty)
            {
                txtDbPort = "3306";
            }
            //数据库用户
            string txtDbUser = this.txtDbUser.Text.Trim();
            if (txtDbUser == string.Empty)
            {
                txtDbUser = "root";
            }
            //数据库密码
            string txtDbPassword = this.txtDbPassword.Text.Trim();
            if (txtDbPassword == string.Empty)
            {
                txtDbPassword = "root";
            }
            //数据库名称
            string txtDbName = this.txtDbName.Text.Trim();
            if (txtDbName == string.Empty)
            {
                txtDbName = "globaldata";
            }
            try
            {
                AppSettings.SetValue("DbHost", txtDbHost);
                AppSettings.SetValue("DbPort", txtDbPort);
                AppSettings.SetValue("DbUser", txtDbUser);
                AppSettings.SetValue("DbPassword", txtDbPassword);
                AppSettings.SetValue("DbName", txtDbName);
                MessageBox.Show("设置成功");
            }
            catch (Exception)
            {
                MessageBox.Show("设置失败");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //设置线程池属性
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(threadNum, threadNum);
            //创建数据库连接
            string txtDbHost = this.txtDbHost.Text.Trim();
            string txtDbPort = this.txtDbPort.Text.Trim();
            string txtDbUser = this.txtDbUser.Text.Trim();
            string txtDbPassword = this.txtDbPassword.Text.Trim();
            string txtDName = this.txtDbName.Text.Trim();
            if (txtDName == string.Empty)
                return;
            Db = getDb(txtDName);
            if (Db == null)
                return;
            //检测数据库连接
            if (!Db.CheckConnectStatus())
                return;
            //加载开区列表
            getServerList();
        }

        private void getServerList()
        {
            string sql = "select serverid,`name`,`database` from serverroute;";
            DataSet serverDataSet = Db.GetDataSet(sql, "serverroute");
            DataTable dt = serverDataSet.Tables["serverroute"];
            if (dt.Rows.Count != 0)
            {
                //清空内容
                this.listView1.Items.Clear();
                foreach (DataRow dr in dt.Rows)
                {
                    ListViewItem lvi = new ListViewItem();
                    lvi.UseItemStyleForSubItems = false;//让单元格自定义样式
                    string serverId = dr[0].ToString();
                    lvi.Text = serverId;
                    lvi.SubItems.Add(dr[1].ToString());
                    lvi.SubItems.Add(dr[2].ToString());
                    lvi.Tag = serverId;
                    this.listView1.Items.Add(lvi);
                }
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            txtLog.Text = "";
            //创建数据库连接
            string txtDbHost = this.txtDbHost.Text.Trim();
            string txtDbPort = this.txtDbPort.Text.Trim();
            string txtDbUser = this.txtDbUser.Text.Trim();
            string txtDbPassword = this.txtDbPassword.Text.Trim();          
            if (!Tool.checkStr(this.txtDbName, "数据库名不能为空"))
                return;
            string txtDName = this.txtDbName.Text.Trim();
            Db = getDb(txtDName);
            if (Db == null)
                return;
            //检测数据库连接
            if (!Db.CheckConnectStatus())
            {
                printLog("数据库连接失败，请检查配置是否正确");
                return;
            }
            //加载开区列表
            getServerList();
        }

        private void btnMainQu_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
            {
                item.ForeColor = Color.Black;
                item.SubItems[1].ForeColor = Color.Black;
                item.SubItems[2].ForeColor = Color.Black;
            }
            if (listView1.SelectedItems.Count != 0)
            {
                string id = listView1.SelectedItems[0].Text;
                string name = listView1.SelectedItems[0].SubItems[1].Text;
                string db = listView1.SelectedItems[0].SubItems[2].Text;
                txtMainName.Text = name;
                txtMainDb.Text = db;
                listView1.Items[0].ForeColor = Color.Black;
                listView1.Items[0].SubItems[1].ForeColor = Color.Black;
                listView1.Items[0].SubItems[2].ForeColor = Color.Black;
                listView1.SelectedItems[0].ForeColor = Color.Red;
                listView1.SelectedItems[0].SubItems[1].ForeColor= Color.Red;
                listView1.SelectedItems[0].SubItems[2].ForeColor = Color.Red;
            }
            else
            {
                MessageBox.Show("请先选择主合区");
            }
        }

        private void btnSubQu_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
            {
                item.ForeColor = Color.Black;
                item.SubItems[1].ForeColor = Color.Black;
                item.SubItems[2].ForeColor = Color.Black;
            }
            if (listView1.SelectedItems.Count != 0)
            {
                string id = listView1.SelectedItems[0].Text;
                string name = listView1.SelectedItems[0].SubItems[1].Text;
                string db = listView1.SelectedItems[0].SubItems[2].Text;
                txtSubName.Text = name;
                txtSubDb.Text = db;
                
                listView1.SelectedItems[0].ForeColor = Color.Blue;
                listView1.SelectedItems[0].SubItems[1].ForeColor = Color.Blue;
                listView1.SelectedItems[0].SubItems[2].ForeColor = Color.Blue;
            }
            else
            {
                MessageBox.Show("请先选择被合区");
            }
        }

        private void btnMerge_Click(object sender, EventArgs e)
        {
            txtLog.Text = "";
            //清空任务
            okTaskList.Clear();
            if (!Tool.checkStr(this.txtMainDb, "请先选择主合区"))
                return;
            if (!Tool.checkStr(this.txtSubDb, "请先选择被合区"))
                return;
            //创建子线程执行线程池任务
            new Thread(new ThreadStart(this.hequ)) { IsBackground = true }.Start(); 
           
        }
        private void hequ()
        {
            //主合区数据库
            string txtMainDb = this.txtMainDb.Text.Trim();
            //被合区数据库
            string txtSubDb = this.txtSubDb.Text.Trim();
            MysqlBase subDb = getDb(txtSubDb);
            if (subDb == null)
                return;
            //检测数据库连接
            if (!subDb.CheckConnectStatus())
            {
                printLog("数据库[" + txtSubDb + "]连接失败");
                return;
            }
            //1.列出所有的数据表
            DataTable dt = subDb.query("show tables");
            if (dt!=null && dt.Rows.Count != 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    duoLine(dr[0].ToString(), txtMainDb, txtSubDb);
                }             
            }
        }

        private void duoLine(string table, string mainDb, string subDb)
        {
            
            for (int i = 0; i < threadNum; i++)
            {
                //QueueUserWorkItem把要执行的方法添加到线程池里
                ThreadPool.QueueUserWorkItem(delegate { task(table, mainDb, subDb); });
            }
        }

        private void task(string table, string mainDb, string subDb)
        {
            Object only = table;
            try
            {
                //当前任务未执行过，且未被其他线程占用
                if (!okTaskList.Contains(only) && Monitor.TryEnter(only, 1000))
                {
                    string sql = String.Format("insert into {1}.{0} select * from {2}.{0}", table, mainDb, subDb);
                    Db.commonExecute(sql);
                    printLog("数据表[" + table+"]合并成功");
                    //任务执行完添加到已执行的集合里
                    okTaskList.Add(only);
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                if (Monitor.TryEnter(only))
                    Monitor.Exit(only);//释放锁
            }   
        }

        /// <summary>
        /// 获取数据库连接对象
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        private MysqlBase getDb(string dbName)
        {
            if (dbName == string.Empty)
                return null; 
            string txtDbHost = this.txtDbHost.Text.Trim();
            string txtDbPort = this.txtDbPort.Text.Trim();
            string txtDbUser = this.txtDbUser.Text.Trim();
            string txtDbPassword = this.txtDbPassword.Text.Trim();
            string server = String.Format("Database={0};Data Source={1};User Id={2};Password={3};pooling=false;CharSet=utf8;port={4}", dbName, txtDbHost, txtDbUser, txtDbPassword, txtDbPort);
            return Db = new MysqlBase(server);
        }
    }
}
