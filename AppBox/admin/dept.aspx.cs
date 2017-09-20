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
    public partial class dept : PageBase
    {
        #region ViewPower

        /// <summary>
        /// 本页面的浏览权限，空字符串表示本页面不受权限控制
        /// </summary>
        public override string ViewPower
        {
            get
            {
                return "CoreDeptView";
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
            CheckPowerWithButton("CoreDeptNew", btnNew);


            btnNew.OnClientClick = Window1.GetShowReference("~/admin/dept_new.aspx", "新增部门");

            BindGrid();
        }

        private void BindGrid()
        {
            Grid1.DataSource = DeptHelper.Depts;
            Grid1.DataBind();
        }

        #endregion

        #region Events

        protected void Grid1_PreDataBound(object sender, EventArgs e)
        {
            // 数据绑定之前，进行权限检查
            CheckPowerWithWindowField("CoreDeptEdit", Grid1, "editField");
            CheckPowerWithLinkButtonField("CoreDeptDelete", Grid1, "deleteField");
        }

        protected void Grid1_RowCommand(object sender, GridCommandEventArgs e)
        {
            int deptID = GetSelectedDataKeyID(Grid1);

            if (e.CommandName == "Delete")
            {
                // 在操作之前进行权限检查
                if (!CheckPower("CoreDeptDelete"))
                {
                    CheckPowerFailWithAlert();
                    return;
                }

                int userCount = DB.Users.Where(u => u.Dept.ID == deptID).Count();
                if (userCount > 0)
                {
                    Alert.ShowInTop("删除失败！需要先清空属于此部门的用户！");
                    return;
                }

                int childCount = DB.Depts.Where(d => d.Parent.ID == deptID).Count();
                if (childCount > 0)
                {
                    Alert.ShowInTop("删除失败！请先删除子部门！");
                    return;
                }

                DB.Depts.Delete<Dept>(d => d.ID == deptID);

                DeptHelper.Reload();
                BindGrid();
            }
        }

        protected void Window1_Close(object sender, EventArgs e)
        {
            DeptHelper.Reload();
            BindGrid();
        }

        #endregion

    }
}
