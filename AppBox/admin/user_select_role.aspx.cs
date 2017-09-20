using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Linq;
using System.Data.Entity;
using FineUI;

namespace AppBox.admin
{
    public partial class user_select_role : PageBase
    {
        #region ViewPower

        /// <summary>
        /// 本页面的浏览权限，空字符串表示本页面不受权限控制
        /// </summary>
        public override string ViewPower
        {
            get
            {
                return "CoreRoleView";
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

            string ids = GetQueryValue("ids");
            
            // 绑定角色复选框列表
            BindDDLRole();

            // 初始化角色复选框列表的选择项
            cblRole.SelectedValueArray = ids.Split(',');
        }

        private void BindDDLRole()
        {
            cblRole.DataTextField = "Name";
            cblRole.DataValueField = "ID";
            cblRole.DataSource = DB.Roles;
            cblRole.DataBind();
        }

        #endregion

        #region Events

        protected void btnSaveClose_Click(object sender, EventArgs e)
        {
            string roleValues = String.Join(",", cblRole.SelectedItemArray.Select(c => c.Value));
            string roleTexts = String.Join(",", cblRole.SelectedItemArray.Select(c => c.Text));

            PageContext.RegisterStartupScript(ActiveWindow.GetWriteBackValueReference(roleValues, roleTexts)
                + ActiveWindow.GetHideReference());
        }

        #endregion

    }
}
