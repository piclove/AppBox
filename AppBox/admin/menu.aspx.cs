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
    public partial class menu : PageBase
    {
        #region ViewPower

        /// <summary>
        /// 本页面的浏览权限，空字符串表示本页面不受权限控制
        /// </summary>
        public override string ViewPower
        {
            get
            {
                return "CoreMenuView";
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
            CheckPowerWithButton("CoreMenuNew", btnNew);


            btnNew.OnClientClick = Window1.GetShowReference("~/admin/menu_new.aspx", "新增菜单");


            BindGrid();
        }

        private void BindGrid()
        {
            List<Menu> menus = MenuHelper.Menus;
            Grid1.DataSource = menus;
            Grid1.DataBind();
        }


        protected string GetModuleName(object moduleNameObj)
        {
            string moduleName = moduleNameObj.ToString();
            if (moduleName == "None")
            {
                return String.Empty;
            }
            return moduleName;
        }

        #endregion

        #region Events

        protected void Grid1_PreDataBound(object sender, EventArgs e)
        {
            // 数据绑定之前，进行权限检查
            CheckPowerWithWindowField("CoreMenuEdit", Grid1, "editField");
            CheckPowerWithLinkButtonField("CoreMenuDelete", Grid1, "deleteField");
        }


        protected void Grid1_RowCommand(object sender, GridCommandEventArgs e)
        {
            int menuID = GetSelectedDataKeyID(Grid1);

            if (e.CommandName == "Delete")
            {
                // 在操作之前进行权限检查
                if (!CheckPower("CoreMenuDelete"))
                {
                    CheckPowerFailWithAlert();
                    return;
                }

                int childCount = DB.Menus.Where(m => m.Parent.ID == menuID).Count();
                if (childCount > 0)
                {
                    Alert.ShowInTop("删除失败！请先删除子菜单！");
                    return;
                }

                DB.Menus.Delete<Menu>(m => m.ID == menuID);

                MenuHelper.Reload();
                BindGrid();
            }
        }

        protected void Window1_Close(object sender, EventArgs e)
        {
            MenuHelper.Reload();
            BindGrid();
        }

        #endregion

    }
}
