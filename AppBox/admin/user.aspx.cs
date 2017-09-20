using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using FineUI;
using System.Linq;
using System.Data.Entity;
using EntityFramework.Extensions;

namespace AppBox.admin
{
    public partial class user : PageBase
    {
        #region ViewPower

        /// <summary>
        /// 本页面的浏览权限，空字符串表示本页面不受权限控制
        /// </summary>
        public override string ViewPower
        {
            get
            {
                return "CoreUserView";
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
            CheckPowerWithButton("CoreUserEdit", btnChangeEnableUsers);
            CheckPowerWithButton("CoreUserDelete", btnDeleteSelected);
            CheckPowerWithButton("CoreUserNew", btnNew);



            ResolveDeleteButtonForGrid(btnDeleteSelected, Grid1);

            ResolveEnableStatusButtonForGrid(btnEnableUsers, Grid1, true);
            ResolveEnableStatusButtonForGrid(btnDisableUsers, Grid1, false);

            btnNew.OnClientClick = Window1.GetShowReference("~/admin/user_new.aspx", "新增用户");

            // 每页记录数
            Grid1.PageSize = ConfigHelper.PageSize;
            ddlGridPageSize.SelectedValue = ConfigHelper.PageSize.ToString();

            BindGrid();
        }

        private void ResolveEnableStatusButtonForGrid(MenuButton btn, Grid grid, bool enabled)
        {
            string enabledStr = "启用";
            if (!enabled)
            {
                enabledStr = "禁用";
            }
            btn.OnClientClick = grid.GetNoSelectionAlertInParentReference("请至少应该选择一项记录！");
            btn.ConfirmText = String.Format("确定要{1}选中的<span class=\"highlight\"><script>{0}</script></span>项记录吗？", grid.GetSelectedCountReference(), enabledStr);
            btn.ConfirmTarget = FineUI.Target.Top;
        }

        private void BindGrid()
        {
            IQueryable<User> q = DB.Users; //.Include(u => u.Dept);

            // 在用户名称中搜索
            string searchText = ttbSearchMessage.Text.Trim();
            if (!String.IsNullOrEmpty(searchText))
            {
                q = q.Where(u => u.Name.Contains(searchText) || u.ChineseName.Contains(searchText) || u.EnglishName.Contains(searchText));
            }

            if (GetIdentityName() != "admin")
            {
                q = q.Where(u => u.Name != "admin");
            }

            // 过滤启用状态
            if (rblEnableStatus.SelectedValue != "all")
            {
                q = q.Where(u => u.Enabled == (rblEnableStatus.SelectedValue == "enabled" ? true : false));
            }

            // 在查询添加之后，排序和分页之前获取总记录数
            Grid1.RecordCount = q.Count();

            // 排列和数据库分页
            q = SortAndPage<User>(q, Grid1);

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

        protected void Grid1_PreDataBound(object sender, EventArgs e)
        {
            // 数据绑定之前，进行权限检查
            CheckPowerWithWindowField("CoreUserEdit", Grid1, "editField");
            CheckPowerWithLinkButtonField("CoreUserDelete", Grid1, "deleteField");
            CheckPowerWithWindowField("CoreUserChangePassword", Grid1, "changePasswordField");

        }

        protected void Grid1_PreRowDataBound(object sender, FineUI.GridPreRowEventArgs e)
        {
            User user = e.DataItem as User;

            // 不能删除超级管理员
            if (user.Name == "admin")
            {
                FineUI.LinkButtonField deleteField = Grid1.FindColumn("deleteField") as FineUI.LinkButtonField;
                deleteField.Enabled = false;
                deleteField.ToolTip = "不能删除超级管理员！";
            }

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
            if (!CheckPower("CoreUserDelete"))
            {
                CheckPowerFailWithAlert();
                return;
            }

            // 从每个选中的行中获取ID（在Grid1中定义的DataKeyNames）
            List<int> ids = GetSelectedDataKeyIDs(Grid1);

            // 执行数据库操作
            //DB.Users.Where(u => ids.Contains(u.UserID)).ToList().ForEach(u => DB.Users.Remove(u));
            //DB.SaveChanges();
            DB.Users.Delete(u => ids.Contains(u.ID));


            // 重新绑定表格
            BindGrid();
        }

        protected void btnEnableUsers_Click(object sender, EventArgs e)
        {
            SetSelectedUsersEnableStatus(true);
        }

        protected void btnDisableUsers_Click(object sender, EventArgs e)
        {
            SetSelectedUsersEnableStatus(false);
        }


        private void SetSelectedUsersEnableStatus(bool enabled)
        {
            // 在操作之前进行权限检查
            if (!CheckPower("CoreUserEdit"))
            {
                CheckPowerFailWithAlert();
                return;
            }

            // 从每个选中的行中获取ID（在Grid1中定义的DataKeyNames）
            List<int> ids = GetSelectedDataKeyIDs(Grid1);

            // 执行数据库操作
            //DB.Users.Where(u => ids.Contains(u.UserID)).ToList().ForEach(u => u.Enabled = enabled);
            //DB.SaveChanges();
            DB.Users.Update(u => ids.Contains(u.ID), u => new User { Enabled = enabled });

            // 重新绑定表格
            BindGrid();
        }

        protected void Grid1_RowCommand(object sender, GridCommandEventArgs e)
        {
            int userID = GetSelectedDataKeyID(Grid1);
            string userName = GetSelectedDataKey(Grid1, 1);

            if (e.CommandName == "Delete")
            {
                // 在操作之前进行权限检查
                if (!CheckPower("CoreUserDelete"))
                {
                    CheckPowerFailWithAlert();
                    return;
                }

                if (userName == "admin")
                {
                    Alert.ShowInTop("不能删除默认的系统管理员（admin）！");
                }
                else
                {
                    DB.Users.Delete(u => u.ID == userID);

                    BindGrid();
                }
            }
        }

        protected void Window1_Close(object sender, EventArgs e)
        {
            BindGrid();
        }

        protected void rblEnableStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindGrid();
        }


        protected void ddlGridPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            Grid1.PageSize = Convert.ToInt32(ddlGridPageSize.SelectedValue);

            BindGrid();
        }

        #endregion

    }
}
