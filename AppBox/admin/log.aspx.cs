using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Linq;
using System.Data.Entity;
using FineUI;
using EntityFramework.Extensions;

namespace AppBox.admin
{
    public partial class log : PageBase
    {
        #region ViewPower

        /// <summary>
        /// 本页面的浏览权限，空字符串表示本页面不受权限控制
        /// </summary>
        public override string ViewPower
        {
            get
            {
                return "CoreLogView";
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
            // 权限检查
            CheckPowerWithButton("CoreLogDelete", btnDeleteSelected);

            // 每页记录数
            Grid1.PageSize = ConfigHelper.PageSize;
            ddlGridPageSize.SelectedValue = ConfigHelper.PageSize.ToString();

            // 点击删除按钮时，至少选中一项
            ResolveDeleteButtonForGrid(btnDeleteSelected, Grid1);

            BindGrid();
        }

        private void BindGrid()
        {
            IQueryable<Log> q = DB.Logs;

            // 在错误信息中搜索
            string searchText = ttbSearchMessage.Text.Trim();
            if (!String.IsNullOrEmpty(searchText))
            {
                q = q.Where(l => l.Message.Contains(searchText));
            }

            // 过滤错误级别
            if (ddlSearchLevel.SelectedValue != "ALL")
            {
                q = q.Where(l => l.Level == ddlSearchLevel.SelectedValue);
            }

            // 过滤搜索范围
            if (ddlSearchRange.SelectedValue != "ALL")
            {
                DateTime today = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
                switch (ddlSearchRange.SelectedValue)
                {
                    case "TODAY":
                        q = q.Where(l => l.LogTime >= today);
                        break;
                    case "LAST3DAYS":
                        q = q.Where(l => l.LogTime >= today.AddDays(-3));
                        break;
                    case "LAST7DAYS":
                        q = q.Where(l => l.LogTime >= today.AddDays(-7));
                        break;
                    case "LASTMONTH":
                        q = q.Where(l => l.LogTime >= today.AddMonths(-1));
                        break;
                    case "LASTYEAR":
                        q = q.Where(l => l.LogTime >= today.AddYears(-1));
                        break;
                }
            }


            // 在查询添加之后，排序和分页之前获取总记录数
            Grid1.RecordCount = q.Count();


            // 排列和分页
            q = SortAndPage<Log>(q, Grid1);

            Grid1.DataSource = q;
            Grid1.DataBind();
        }

        #endregion

        #region Events

        protected void ttbSearchMessage_Trigger2Click(object sender, EventArgs e)
        {
            ttbSearchMessage.ShowTrigger1 = true;
            BindGrid();
        }

        protected void ttbSearchMessage_Trigger1Click(object sender, EventArgs e)
        {
            ttbSearchMessage.Text = String.Empty;
            ttbSearchMessage.ShowTrigger1 = false;
            BindGrid();
        }

        protected void ddlSearchLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindGrid();
        }

        protected void ddlSearchRange_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindGrid();
        }


        protected void Grid1_PreDataBound(object sender, EventArgs e)
        {
            // 数据绑定之前，进行权限检查
            CheckPowerWithLinkButtonField("CoreLogDelete", Grid1, "deleteField");
        }


        protected void Grid1_Sort(object sender, GridSortEventArgs e)
        {
			Grid1.SortDirection = e.SortDirection;
            Grid1.SortField = e.SortField;
            BindGrid();
        }

        protected void Grid1_PageIndexChange(object sender, GridPageEventArgs e)
        {
            Grid1.PageIndex = e.NewPageIndex;

            BindGrid();
        }

        protected void btnDeleteSelected_Click(object sender, EventArgs e)
        {
            // 在操作之前进行权限检查
            if (!CheckPower("CoreLogDelete"))
            {
                CheckPowerFailWithAlert();
                return;
            }

            // 从每个选中的行中获取ID（在Grid1中定义的DataKeyNames）
            List<int> ids = GetSelectedDataKeyIDs(Grid1);

            // 执行数据库操作
            DB.Logs.Delete(l => ids.Contains(l.ID));

            // 重新绑定表格
            BindGrid();
        }

        protected void Grid1_RowCommand(object sender, GridCommandEventArgs e)
        {
            int logID = GetSelectedDataKeyID(Grid1);

            if (e.CommandName == "Delete")
            {
                // 在操作之前进行权限检查
                if (!CheckPower("CoreLogDelete"))
                {
                    CheckPowerFailWithAlert();
                    return;
                }

                DB.Logs.Delete(l => l.ID == logID);
                

                BindGrid();
            }
        }

        protected void ddlGridPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            Grid1.PageSize = Convert.ToInt32(ddlGridPageSize.SelectedValue);

            BindGrid();
        }

        #endregion

    }
}
