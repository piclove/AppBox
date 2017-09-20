using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Linq;
using System.Data.Entity;
using FineUI;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace AppBox.admin
{
    public partial class dept_user_addnew : PageBase
    {
        #region ViewPower

        /// <summary>
        /// 本页面的浏览权限，空字符串表示本页面不受权限控制
        /// </summary>
        public override string ViewPower
        {
            get
            {
                return "CoreDeptUserNew";
            }
        }

        #endregion

        #region Page_Load

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadData();
            }
        }

        private void LoadData()
        {
            btnClose.OnClientClick = ActiveWindow.GetHideReference();

            int id = GetQueryIntValue("id");
            Dept current = DB.Depts.Find(id);
            if (current == null)
            {
                // 参数错误，首先弹出Alert对话框然后关闭弹出窗口
                Alert.Show("参数错误！", String.Empty, ActiveWindow.GetHideReference());
                return;
            }

            // 每页记录数
            Grid1.PageSize = ConfigHelper.PageSize;
            ddlGridPageSize.SelectedValue = ConfigHelper.PageSize.ToString();


            BindGrid();
        }


        private void BindGrid()
        {
            IQueryable<User> q = DB.Users.Include(u => u.Dept);

            // 在职务名称中搜索
            string searchText = ttbSearchMessage.Text.Trim();
            if (!String.IsNullOrEmpty(searchText))
            {
                q = q.Where(u => u.Name.Contains(searchText) || u.ChineseName.Contains(searchText) || u.EnglishName.Contains(searchText));
            }

            q = q.Where(u => u.Name != "admin");

            // 排除所有已经属于某个部门的用户
            q = q.Where(u => u.Dept == null);

            // 在查询添加之后，排序和分页之前获取总记录数
            Grid1.RecordCount = q.Count();

            // 排列和分页
            q = SortAndPage<User>(q, Grid1);


            Grid1.DataSource = q;
            Grid1.DataBind();


            // 重新绑定表格数据之后，更新选中行
            UpdateSelectedRowIndexArray(hfSelectedIDS, Grid1);
        }

        #endregion

        #region Events

        protected void btnSaveClose_Click(object sender, EventArgs e)
        {
            SyncSelectedRowIndexArrayToHiddenField();

            int deptID = GetQueryIntValue("id");

            // 从每个选中的行中获取ID（在Grid1中定义的DataKeyNames）
            List<int> ids = GetSelectedIDsFromHiddenField(hfSelectedIDS);


            Dept dept = Attach<Dept>(deptID);

            DB.Users.Where(u => ids.Contains(u.ID))
                .ToList()
                .ForEach(u => u.Dept = dept);

            DB.SaveChanges();
             

            PageContext.RegisterStartupScript(ActiveWindow.GetHidePostBackReference());
        }

        private void SyncSelectedRowIndexArrayToHiddenField()
        {
            // 重新绑定表格数据之前，将当前表格页选中行的数据同步到隐藏字段中
            SyncSelectedRowIndexArrayToHiddenField(hfSelectedIDS, Grid1);
        }



        protected void ttbSearchMessage_Trigger2Click(object sender, EventArgs e)
        {
            SyncSelectedRowIndexArrayToHiddenField();

            ttbSearchMessage.ShowTrigger1 = true;
            BindGrid();
        }

        protected void ttbSearchMessage_Trigger1Click(object sender, EventArgs e)
        {
            SyncSelectedRowIndexArrayToHiddenField();

            ttbSearchMessage.Text = String.Empty;
            ttbSearchMessage.ShowTrigger1 = false;
            BindGrid();
        }

        protected void Grid1_Sort(object sender, GridSortEventArgs e)
        {
            SyncSelectedRowIndexArrayToHiddenField();

			Grid1.SortDirection = e.SortDirection;
            Grid1.SortField = e.SortField;
            BindGrid();
        }

        protected void Grid1_PageIndexChange(object sender, GridPageEventArgs e)
        {
            SyncSelectedRowIndexArrayToHiddenField();

            Grid1.PageIndex = e.NewPageIndex;
            BindGrid();
        }


        protected void ddlGridPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            SyncSelectedRowIndexArrayToHiddenField();

            Grid1.PageSize = Convert.ToInt32(ddlGridPageSize.SelectedValue);

            BindGrid();
        }

        #endregion


    }
}
